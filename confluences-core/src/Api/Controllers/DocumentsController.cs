using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Api.Documents;
using Confluences.Persistence;
using System;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using QuestPDF.Fluent;
using Microsoft.Extensions.Logging;
using Confluences.Domain.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using Api.Models.Dto.Attendance;

namespace Api.Controllers
{
    // Bilans, attestations et fiches de présence : données personnelles, réservées aux
    // formatrices connectées (le contrôleur était jusqu'ici accessible sans connexion).
    [Authorize(Policy = "Teacher")]
    [Route("api/documents")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly ConfluencesDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<DocumentsController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public DocumentsController(
            ConfluencesDbContext db,
            IWebHostEnvironment env,
            ILogger<DocumentsController> logger,
            UserManager<ApplicationUser> userManager)
        {
            _db          = db;
            _env         = env;
            _logger      = logger;
            _userManager = userManager;
        }

        // ──────────────────────────────────────────────────────────────────────
        // ATTESTATION TRIMESTRE DE STAGE (conservé)
        // ──────────────────────────────────────────────────────────────────────
        [HttpGet("attestation-trimestre/{stagiaireId}")]
        public IActionResult AttestationTrimestre(
            string stagiaireId,
            [FromQuery] int year,
            [FromQuery] string trimestre)
        {
            var stages = _db.Stages
                .Include(s => s.Stagiaire)
                .Include(s => s.TypeIntershipActivity)
                .Include(s => s.TypeStage)
                .Include(s => s.Entreprise)
                .Where(s => s.StagiaireId == stagiaireId)
                .ToList();

            if (!stages.Any())
                return NotFound("Aucun stage trouvé pour ce trimestre.");

            var doc = new AttestationTrimestreDocument(
                stages.First().Stagiaire, stages, year, trimestre, GetLogoBytes());
            return File(doc.GeneratePdf(), "application/pdf",
                $"attestation-{stagiaireId}-{year}-{trimestre}.pdf");
        }

        // ──────────────────────────────────────────────────────────────────────
        // BILAN DE STAGE (conservé)
        // ──────────────────────────────────────────────────────────────────────
        [HttpGet("bilan/{id}")]
        public IActionResult BilanStage(int id)
        {
            var stage = _db.Stages
                .Include(s => s.Stagiaire)
                .Include(s => s.TypeIntershipActivity)
                .Include(s => s.TypeMetier)
                .Include(s => s.TypeStage)
                .Include(s => s.Entreprise)
                .FirstOrDefault(s => s.StageId == id);

            if (stage == null || stage.Stagiaire == null)
                return NotFound($"Stage ID {id} non trouvé.");

            var doc = new BilanStageDocument(stage, stage.Stagiaire, GetLogoBytes());
            return File(doc.GeneratePdf(), "application/pdf",
                $"bilan-stage-{id}-{stage.Stagiaire.LastName}.pdf");
        }

        // ──────────────────────────────────────────────────────────────────────
        // SAUVEGARDE BILAN DE SESSION
        // ──────────────────────────────────────────────────────────────────────
        [HttpPost("save-bilan-session")]
        public async Task<IActionResult> SaveBilanSession([FromBody] SaveBilanSessionRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest("Données invalides (ID utilisateur manquant).");

            if (!request.Quarter.StartsWith("T"))
                request.Quarter = $"T{request.Quarter}";

            var report = await _db.SessionReports.FirstOrDefaultAsync(r =>
                r.StagiaireId == request.UserId &&
                r.Year        == request.Year   &&
                r.Quarter     == request.Quarter);

            bool isNew = (report == null);
            if (isNew)
            {
                report = new SessionReport
                {
                    StagiaireId = request.UserId,
                    Year        = request.Year,
                    Quarter     = request.Quarter,
                    CreatedAt   = DateTime.Now,
                };
                _db.SessionReports.Add(report);
            }

            report.UpdatedAt                  = DateTime.Now;
            report.EvaluationText             = request.EvaluationText;
            report.FollowUpActions            = request.FollowUpActions;
            report.GlobalRate                 = request.GlobalRate;
            report.SocialInsertionText        = request.SocialInsertionText;
            report.DigitalSkillsText          = request.DigitalSkillsText;
            report.IndividualAccompanyingText = request.IndividualAccompanyingText;
            report.HomeVisitsText             = request.HomeVisitsText;
            report.ObjectivesText             = request.ObjectivesText;

            if (request.SelectedWorkshops != null)
                report.WorkshopsJson = JsonSerializer.Serialize(request.SelectedWorkshops);

            try
            {
                await _db.SaveChangesAsync();
                return Ok(new { message = isNew ? "Rapport créé." : "Rapport mis à jour." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur BDD: " + ex.Message);
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        // LECTURE DONNÉES BILAN DE SESSION
        // ──────────────────────────────────────────────────────────────────────
        [HttpGet("bilan-session-data/{userId}")]
        public async Task<IActionResult> GetBilanSessionData(
            string userId,
            [FromQuery] int year,
            [FromQuery] string quarter)
        {
            if (!quarter.StartsWith("T")) quarter = $"T{quarter}";

            var report = await _db.SessionReports
                .AsNoTracking()
                .Where(r =>
                    r.StagiaireId == userId &&
                    r.Year        == year   &&
                    r.Quarter     == quarter)
                .Select(r => new
                {
                    r.StagiaireId,
                    r.Year,
                    r.Quarter,
                    r.EvaluationText,
                    r.FollowUpActions,
                    r.GlobalRate,
                    r.SocialInsertionText,
                    r.DigitalSkillsText,
                    r.IndividualAccompanyingText,
                    r.HomeVisitsText,
                    r.ObjectivesText,
                    r.WorkshopsJson,
                })
                .FirstOrDefaultAsync();

            return Ok(report);
        }

        // ──────────────────────────────────────────────────────────────────────
        // PDF — BILAN DE SESSION
        // ──────────────────────────────────────────────────────────────────────
        [HttpGet("bilan-session/{userId}")]
        public async Task<IActionResult> GenerateBilanSessionPdf(
            string userId,
            [FromQuery] int year,
            [FromQuery] string trimestre,
            [FromQuery] string mesure,
            [FromQuery] string startDate  = null,   // date début session du calendrier
            [FromQuery] string endDate    = null,   // date fin session du calendrier
            [FromQuery] string sessionName = null)
        {
            var user = await _db.Users
                .Include(u => u.Gender)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound("Utilisateur introuvable.");

            if (!trimestre.StartsWith("T")) trimestre = $"T{trimestre}";

            var report = await _db.SessionReports.FirstOrDefaultAsync(r =>
                r.StagiaireId == userId &&
                r.Year        == year   &&
                r.Quarter     == trimestre);

            if (report == null)
                return NotFound($"Aucun bilan trouvé pour {trimestre}/{year}.");

            // Niveaux spécifiques à la période (StagiaireNiveaux), avec fallback sur ApplicationUser
            if (!int.TryParse(trimestre.Replace("T", ""), out int quarterNum)) quarterNum = 1;
            var niveauPeriode = await _db.StagiaireNiveaux
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.StagiaireId == userId && n.Annee == year && n.Trimestre == quarterNum);
            string oralLevel    = niveauPeriode?.OralLevel    ?? user.OralLevel;
            string writtenLevel = niveauPeriode?.WrittenLevel ?? user.WrittenLevel;

            var (periodStart, periodEnd) = ResolveSessionDates(startDate, endDate, year, trimestre);
            var activitiesSummary = await BuildActivitiesSummary(userId, periodStart, periodEnd);

            // Le niveau du titre vient de la mesure suivie (classe/mesure transmise par le
            // frontend), pas du niveau personnel du/de la participant·e — voir commentaire dans
            // GenerateAttestationSessionPdf pour la même logique appliquée aux attestations.
            string mesureLevel = "";
            foreach (var src in new[] { mesure ?? "", oralLevel ?? "" })
            {
                if (string.IsNullOrWhiteSpace(src)) continue;
                var lm = System.Text.RegularExpressions.Regex.Match(src, @"[A-Za-z]\d");
                if (lm.Success) { mesureLevel = lm.Value.ToUpper(); break; }
            }

            var doc = new BilanSessionDocument(
                user,
                report.EvaluationText,
                report.FollowUpActions,
                oralLevel,
                writtenLevel,
                report.GlobalRate,
                year,
                trimestre,
                GetLogoBytes(),
                DetermineCourseName(mesure, null),
                periodStart,
                periodEnd,
                report.SocialInsertionText,
                report.DigitalSkillsText,
                report.IndividualAccompanyingText,
                report.HomeVisitsText,
                activitiesSummary,
                mesureLevel,
                report.ObjectivesText
            );

            return File(doc.GeneratePdf(), "application/pdf",
                $"Bilan-Session-{user.LastName}.pdf");
        }

        // ──────────────────────────────────────────────────────────────────────
        // PDF — ATTESTATION DE SESSION
        // ──────────────────────────────────────────────────────────────────────
        [HttpGet("attestation-session/{userId}")]
        public async Task<IActionResult> GenerateAttestationSessionPdf(
            string userId,
            [FromQuery] int year,
            [FromQuery] string trimestre,
            [FromQuery] string mesure,
            [FromQuery] string startDate   = null,   // date début session du calendrier
            [FromQuery] string endDate     = null,   // date fin session du calendrier
            [FromQuery] string sessionName = null)
        {
            if (!trimestre.StartsWith("T")) trimestre = $"T{trimestre}";

            var (periodStart, periodEnd) = ResolveSessionDates(startDate, endDate, year, trimestre);

            var user = await _db.Users
                .Include(u => u.SessionStudents)
                    .ThenInclude(ss => ss.Session)
                    .ThenInclude(s  => s.SchoolClassRoom)
                .SingleOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound("Stagiaire introuvable.");

            // Cherche la session sur la période, sinon prend la plus récente (même logique que UsersController)
            var matchedSession = user.SessionStudents
                .Select(ss => ss.Session)
                .FirstOrDefault(s => s.DateStart <= periodEnd && s.DateEnd >= periodStart)
                ?? user.SessionStudents
                    .OrderByDescending(ss => ss.Session.DateStart)
                    .Select(ss => ss.Session)
                    .FirstOrDefault();

            string classRoomName = matchedSession?.SchoolClassRoom?.SchoolClassRoomName ?? "";

            string courseName = DetermineCourseName(mesure, classRoomName);

            // Le niveau sur l'attestation vient de la formation, pas du niveau oral/écrit personnel.
            // Priorité : classRoomName (= "niveau" affiché dans l'app, ex: "Télécours-A2"),
            //            puis mesure (paramètre frontend), puis OralLevel en dernier recours.
            string niveauAttestation = "";
            foreach (var src in new[] { classRoomName, mesure ?? "", user.OralLevel ?? "" })
            {
                if (string.IsNullOrWhiteSpace(src)) continue;
                var lm = System.Text.RegularExpressions.Regex.Match(src, @"[A-Za-z]\d");
                if (lm.Success) { niveauAttestation = lm.Value.ToUpper(); break; }
            }

            var report = await _db.SessionReports.FirstOrDefaultAsync(r =>
                r.StagiaireId == userId &&
                r.Year        == year   &&
                r.Quarter     == trimestre);

            var allColumns = await _db.MatrixColumns
                .Select(c => new { c.Key, c.Label, c.Type })
                .OrderBy(c => c.Type).ThenBy(c => c.Label)
                .ToListAsync();

            var presencesCount = await _db.AccompanyingPresences
                .Where(ap => ap.UserId == userId && ap.Date >= periodStart && ap.Date <= periodEnd)
                .GroupBy(ap => ap.ActivityName)
                .Select(g => new { Key = g.Key, Count = g.Count() })
                .ToListAsync();
            var countByKey = presencesCount.ToDictionary(p => p.Key, p => p.Count);

            var ateliersSuivis = allColumns
                .Select(c => {
                    int cnt = countByKey.TryGetValue(c.Key, out var n) ? n : 0;
                    return $"{c.Label}|{c.Type}|{cnt}";
                })
                .ToList();

            // Taux de participation : même calcul que la fiche de présence (qui fait référence),
            // sur toute la session (= "part.3", cumulatif).
            bool isTeleCoursCalc = courseName.Contains("Télécours", StringComparison.OrdinalIgnoreCase)
                                || courseName.Contains("TéléCours", StringComparison.OrdinalIgnoreCase);
            int targetDaysCalc = CalculerJoursCibles(isTeleCoursCalc ? "tele" : "fi", 3); // 3 = session complète
            double tauxParticipation = await CalculerTauxCommeFicheAsync(user, isTeleCoursCalc, periodStart, periodEnd, targetDaysCalc);

            var doc = new AttestationSessionDocument(
                user, courseName, niveauAttestation, user.WrittenLevel,
                ateliersSuivis, tauxParticipation, year, trimestre,
                GetLogoBytes(), GetSignatureBytes(), periodStart, periodEnd);

            return File(doc.GeneratePdf(), "application/pdf",
                $"Attestation-{user.LastName}.pdf");
        }

        // ──────────────────────────────────────────────────────────────────────
        // PDF — ATTESTATION DE PRÉSENCE
        // ──────────────────────────────────────────────────────────────────────
        [HttpGet("attestationpresence/{userId}")]
        public async Task<IActionResult> GenerateAttestationPresencePdf(
            string userId,
            [FromQuery] string startDate      = null,
            [FromQuery] string endDate        = null,
            [FromQuery] string sessionEndDate = null,   // fin globale de session (signature)
            [FromQuery] string sessionName    = null,
            [FromQuery] string sessionType    = "fi",
            [FromQuery] int?   sessionMonth   = null,
            [FromQuery] int?   year           = null,
            [FromQuery] int?   month          = null,
            // Périodes de la session jusqu'à celle de l'attestation : "aaaa-mm-jj_aaaa-mm-jj_semaines",
            // séparées par des virgules (découpage de la fiche de présence).
            [FromQuery] string parts          = null)
        {
            var frenchCulture = new System.Globalization.CultureInfo("fr-FR");
            DateTime periodStart;
            DateTime periodEnd;
            string   periodLabel;

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                if (!DateTime.TryParse(startDate, out periodStart) ||
                    !DateTime.TryParse(endDate,   out periodEnd))
                    return BadRequest("Dates invalides (attendu : yyyy-MM-dd).");

                periodStart = periodStart.Date;
                periodEnd   = periodEnd.Date;
                periodLabel = !string.IsNullOrEmpty(sessionName)
                    ? $"Session {sessionName}"
                    : $"{periodStart:dd/MM/yyyy} → {periodEnd:dd/MM/yyyy}";
            }
            else if (year.HasValue && month.HasValue)
            {
                if (month < 1 || month > 12)
                    return BadRequest("Le mois doit être compris entre 1 et 12.");
                periodStart = new DateTime(year.Value, month.Value, 1);
                periodEnd   = new DateTime(year.Value, month.Value,
                    DateTime.DaysInMonth(year.Value, month.Value));
                periodLabel = periodStart.ToString("MMMM yyyy", frenchCulture);
            }
            else
            {
                return BadRequest("Veuillez fournir soit (startDate + endDate) soit (year + month).");
            }

            // Mois dans la session
            int moisSession = sessionMonth ?? 1;
            int targetDays  = CalculerJoursCibles(sessionType ?? "fi", moisSession);

            // Date de fin de session globale — fournie par le front (ex: 18 juin pour S2)
            DateTime sessionEnd = DateTime.MinValue;
            if (!string.IsNullOrEmpty(sessionEndDate) && DateTime.TryParse(sessionEndDate, out var parsedSessionEnd))
                sessionEnd = parsedSessionEnd.Date;

            var sessionCtx = new SessionContext
            {
                SessionMonth      = moisSession,
                SessionType       = sessionType ?? "fi",
                SessionTargetDays = targetDays,
                SessionName       = sessionName ?? periodLabel,
                SessionStartDate  = periodStart,
                SessionEndDate    = sessionEnd,
                Periodes          = ParsePeriodes(parts, sessionType ?? "fi", periodStart, periodEnd),
            };

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound("Stagiaire introuvable.");

            var existingPresences = await _db.Presences
                .AsNoTracking()
                .Where(p => p.StagiaireId == userId && p.Date >= periodStart && p.Date <= periodEnd)
                .ToDictionaryAsync(p => p.Date.Date, p => p.Statut);

            var joursFeries = await _db.JoursFeries
                .Where(f => f.Date >= periodStart && f.Date <= periodEnd)
                .Select(f => f.Date.Date)
                .ToListAsync();

            // Vacances : périodes du calendrier + vacances inter-sessions saisies sur les sessions,
            // exactement comme la fiche de présence (vacancesRanges).
            var periodesVacances = (await _db.PeriodesVacances
                    .Where(v => v.EndDate >= periodStart && v.StartDate <= periodEnd)
                    .Select(v => new { Start = v.StartDate, End = v.EndDate })
                    .ToListAsync())
                .Concat((await _db.SessionFormations
                        .Where(s => s.VacanceStart != null && s.VacanceEnd != null
                                 && s.VacanceEnd >= periodStart && s.VacanceStart <= periodEnd)
                        .Select(s => new { Start = s.VacanceStart!.Value, End = s.VacanceEnd!.Value })
                        .ToListAsync()))
                .ToList();

            bool IsVacance(DateTime d) =>
                periodesVacances.Any(v => d >= v.Start.Date && d <= v.End.Date);

            // Jours de cours : télécours lun/mar/jeu, F&I lun–ven — les autres jours ne comptent
            // ni comme présence, ni comme absence, ni comme férié (même règle que la fiche de présence).
            bool isTelecoursSession = (sessionType ?? "").ToLower().Contains("tele");
            bool IsJourDeCours(DateTime d) => isTelecoursSession
                ? d.DayOfWeek is DayOfWeek.Monday or DayOfWeek.Tuesday or DayOfWeek.Thursday
                : d.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday);

            // Un·e stagiaire arrivé·e en cours de période ne doit pas être compté·e
            // "présent·e" par défaut sur les jours précédant son inscription. Symétriquement, en
            // cas d'interruption de la mesure, les jours après l'interruption sont traités pareil.
            var participationStart = user.DateDebutParticipation.HasValue && user.DateDebutParticipation.Value.Date > periodStart
                ? user.DateDebutParticipation.Value.Date
                : periodStart;
            var participationEnd = user.DateFinParticipation.HasValue && user.DateFinParticipation.Value.Date < periodEnd
                ? user.DateFinParticipation.Value.Date
                : periodEnd;

            var fullPresences = new List<AttendanceEntryDto>();
            for (var date = periodStart; date <= periodEnd; date = date.AddDays(1))
            {
                if (!IsJourDeCours(date))
                    continue;

                // Même ordre que la fiche de présence (référence) : fériés et vacances d'abord — ils
                // réduisent les jours cibles même avant l'inscription —, puis jours hors inscription.
                string status;
                if (joursFeries.Contains(date.Date))  status = "f";
                else if (IsVacance(date.Date))        status = "vac"; // ≠ "v" (visite à domicile)
                else if (date.Date < participationStart || date.Date > participationEnd) status = "n"; // hors période d'inscription
                else status = existingPresences.TryGetValue(date.Date, out var s)
                        ? (string.IsNullOrWhiteSpace(s) ? "p" : s.Trim().ToLower())
                        : "p";

                fullPresences.Add(new AttendanceEntryDto
                    { StagiaireId = userId, Date = date, Statut = status });
            }

            var docPdf = new AttestationPresenceDocument(
                periodStart, periodEnd, fullPresences,
                new List<ApplicationUser> { user },
                GetLogoBytes(), null, sessionCtx);

            var fileName = !string.IsNullOrEmpty(sessionName)
                ? $"Presence-{user.LastName}-{sessionName}-M{moisSession}.pdf"
                : $"Presence-{user.LastName}-{periodEnd:yyyy-MM}.pdf";

            return File(docPdf.GeneratePdf(), "application/pdf", fileName);
        }

        // ══════════════════════════════════════════════════════════════════════
        // HELPERS PRIVÉS
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Agrège les AccompanyingPresences sur une plage de dates et joint
        /// les libellés MatrixColumns pour produire la liste affichée dans le PDF.
        /// </summary>
        private async Task<List<ActivitySummaryItem>> BuildActivitiesSummary(
            string userId, DateTime startDate, DateTime endDate)
        {
            var presences = await _db.AccompanyingPresences
                .Where(ap =>
                    ap.UserId == userId &&
                    ap.Date   >= startDate &&
                    ap.Date   <= endDate)
                .GroupBy(ap => ap.ActivityName)
                .Select(g => new { ActivityKey = g.Key, Count = g.Count() })
                .ToListAsync();

            if (!presences.Any()) return new List<ActivitySummaryItem>();

            var keys = presences.Select(p => p.ActivityKey).ToList();
            var cols = await _db.MatrixColumns
                .Where(c => keys.Contains(c.Key))
                .Select(c => new { c.Key, c.Label, c.Type })
                .ToListAsync();

            return presences
                .Join(cols, p => p.ActivityKey, c => c.Key,
                      (p, c) => new ActivitySummaryItem
                          { Label = c.Label, Type = c.Type, Count = p.Count })
                .OrderBy(x => x.Type).ThenBy(x => x.Label)
                .ToList();
        }

        // Taux de participation calculé exactement comme la colonne "part." de la fiche de présence
        // (AttendanceRegister.vue, ratesByPart), qui fait référence :
        //  - seuls les jours de cours comptent (télécours lun/mar/jeu, F&I lun–ven) ;
        //  - fériés et vacances (calendrier + vacances inter-sessions) réduisent les jours cibles ;
        //  - absences = A + E + ½ R, comptées seulement pendant l'inscription (les jours avant
        //    l'inscription ou après une interruption ne sont ni des absences, ni déduits) ;
        //  - taux = 100 − taux d'absence arrondi demi-bas (12.5 → 12).
        private async Task<double> CalculerTauxCommeFicheAsync(
            ApplicationUser user, bool isTelecours, DateTime start, DateTime end, int targetDays)
        {
            var presences = await _db.Presences.AsNoTracking()
                .Where(p => p.StagiaireId == user.Id && p.Date >= start && p.Date <= end)
                .ToDictionaryAsync(p => p.Date.Date, p => p.Statut);
            var feries = (await _db.JoursFeries
                    .Where(f => f.Date >= start && f.Date <= end)
                    .Select(f => f.Date.Date)
                    .ToListAsync())
                .ToHashSet();
            var vacances = (await _db.PeriodesVacances
                    .Where(v => v.EndDate >= start && v.StartDate <= end)
                    .Select(v => new { Start = v.StartDate, End = v.EndDate })
                    .ToListAsync())
                .Concat(await _db.SessionFormations
                    .Where(s => s.VacanceStart != null && s.VacanceEnd != null && s.VacanceEnd >= start && s.VacanceStart <= end)
                    .Select(s => new { Start = s.VacanceStart!.Value, End = s.VacanceEnd!.Value })
                    .ToListAsync())
                .ToList();

            var debut = user.DateDebutParticipation?.Date;
            var fin   = user.DateFinParticipation?.Date;

            int deduits = 0;
            double absences = 0;
            for (var d = start.Date; d <= end.Date; d = d.AddDays(1))
            {
                bool jourDeCours = isTelecours
                    ? d.DayOfWeek is DayOfWeek.Monday or DayOfWeek.Tuesday or DayOfWeek.Thursday
                    : d.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday);
                if (!jourDeCours) continue;

                if (feries.Contains(d) || vacances.Any(v => d >= v.Start.Date && d <= v.End.Date)) { deduits++; continue; }
                if ((debut.HasValue && d < debut.Value) || (fin.HasValue && d > fin.Value)) continue;

                var st = presences.TryGetValue(d, out var sv) && !string.IsNullOrWhiteSpace(sv) ? sv.Trim().ToLower() : "p";
                // Congé autorisé : pas une absence, retiré des jours pris en compte.
                if (st == "c") { deduits++; continue; }
                if (st == "a" || st == "e") absences += 1;
                else if (st == "r")         absences += 0.5;
            }

            int joursReels = Math.Max(1, targetDays - deduits);
            double tauxAbsence = Math.Ceiling(absences / joursReels * 100.0 - 0.5);
            return Math.Max(0.0, 100.0 - tauxAbsence);
        }

        // Périodes envoyées par la fiche de présence → libellés "Semaines 1 à 4" et jours programmés
        // propres à chaque période (cibles cumulées 20/40/60 ou 10/20/32 → 20/20/20 ou 10/10/12).
        private static List<PeriodeReleve> ParsePeriodes(string parts, string sessionType, DateTime periodStart, DateTime periodEnd)
        {
            var list = new List<PeriodeReleve>();
            if (string.IsNullOrWhiteSpace(parts)) return list;
            int semaine = 1;
            foreach (var raw in parts.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var bits = raw.Split('_');
                if (bits.Length < 2 || !DateTime.TryParse(bits[0], out var s) || !DateTime.TryParse(bits[1], out var e)) return new List<PeriodeReleve>();
                int weeks = bits.Length > 2 && int.TryParse(bits[2], out var w) && w > 0 ? w : 4;
                int i = list.Count + 1;
                list.Add(new PeriodeReleve
                {
                    Label      = $"Semaines {semaine} à {semaine + weeks - 1}",
                    Start      = s.Date < periodStart ? periodStart : s.Date,
                    End        = e.Date > periodEnd ? periodEnd : e.Date,
                    TargetDays = CalculerJoursCibles(sessionType, i) - (i > 1 ? CalculerJoursCibles(sessionType, i - 1) : 0),
                });
                semaine += weeks;
            }
            return list;
        }

        private static int CalculerJoursCibles(string sessionType, int moisSession)
        {
            bool isTele = sessionType?.ToLower().Contains("tele") == true;
            return (isTele, moisSession) switch
            {
                (true,  1) => 10,
                (true,  2) => 20,
                (true,  _) => 32,
                (false, 1) => 20,
                (false, 2) => 40,
                (false, _) => 60,
            };
        }

        private string DetermineCourseName(string frontalMesure, string dbSessionName)
        {
            string n = !string.IsNullOrEmpty(frontalMesure) ? frontalMesure : dbSessionName ?? "";
            if (string.IsNullOrEmpty(n)) return "Français & Intégration";
            var l = n.ToLower();
            if (l.Contains("telecours") || l.Contains("télécours")) return "Français en Télécours";
            if (l.Contains("a2")) return "Français & Intégration (Niveau A2)";
            if (l.Contains("a1")) return "Français & Intégration (Niveau A1)";
            return "Français & Intégration";
        }

        private byte[] GetLogoBytes()
        {
            var p = Path.Combine(_env.WebRootPath, "Upload/images/logo.png");
            try { if (System.IO.File.Exists(p)) return System.IO.File.ReadAllBytes(p); }
            catch (Exception ex) { _logger.LogError(ex, "Erreur lecture logo"); }
            return Array.Empty<byte>();
        }

        private byte[] GetSignatureBytes()
        {
            var p = Path.Combine(_env.WebRootPath, "Upload/images/signature.png");
            try { if (System.IO.File.Exists(p)) return System.IO.File.ReadAllBytes(p); }
            catch (Exception ex) { _logger.LogError(ex, "Erreur lecture signature"); }
            return Array.Empty<byte>();
        }

        /// <summary>
        /// Résout les dates de session : utilise les dates du calendrier si fournies,
        /// sinon replie sur le trimestre calendaire (T1=jan-mar, etc.).
        /// </summary>
        private (DateTime Start, DateTime End) ResolveSessionDates(
            string startDateStr, string endDateStr, int year, string quarter)
        {
            if (!string.IsNullOrEmpty(startDateStr) && !string.IsNullOrEmpty(endDateStr)
                && DateTime.TryParse(startDateStr, out var s)
                && DateTime.TryParse(endDateStr,   out var e))
                return (s.Date, e.Date);

            // Fallback : trimestre calendaire
            if (!int.TryParse(quarter.Replace("T", ""), out int q)) q = 1;
            q = Math.Max(1, Math.Min(4, q));
            var start = new DateTime(year, (q - 1) * 3 + 1, 1);
            return (start.Date, start.AddMonths(3).AddDays(-1).Date);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // DTOs
    // ══════════════════════════════════════════════════════════════════════════

    public class SaveBilanSessionRequest
    {
        public string UserId      { get; set; }
        public string StagiaireId { get => UserId; set => UserId = value; }
        public int    Year        { get; set; }
        public string Quarter     { get; set; }
        public string EvaluationText             { get; set; }
        public string FollowUpActions            { get; set; }
        public double GlobalRate                 { get; set; }
        public string SocialInsertionText        { get; set; }
        public string DigitalSkillsText          { get; set; }
        public string IndividualAccompanyingText { get; set; }
        public string HomeVisitsText             { get; set; }
        public string ObjectivesText             { get; set; }
        public List<string> SelectedWorkshops    { get; set; }
    }

    /// <summary>
    /// Résumé d'une activité d'accompagnement passé au BilanSessionDocument.
    /// </summary>
    public class ActivitySummaryItem
    {
        public string Label { get; set; }
        public string Type  { get; set; }   // "acco" | "atelier"
        public int    Count { get; set; }
    }
}