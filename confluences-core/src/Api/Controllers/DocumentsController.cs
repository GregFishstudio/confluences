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
                activitiesSummary
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

            // Calcul du taux identique à l'attestation de présence :
            // dénominateur = targetDays (fixe par type/session) − fériés réels de la période
            var rawPresences = await _db.Presences
                .AsNoTracking()
                .Where(p => p.StagiaireId == userId && p.Date >= periodStart && p.Date <= periodEnd)
                .ToDictionaryAsync(p => p.Date.Date, p => p.Statut);

            var feriesSession = await _db.JoursFeries
                .Where(f => f.Date >= periodStart && f.Date <= periodEnd)
                .Select(f => f.Date.Date)
                .ToListAsync();

            var vacancesSession = await _db.PeriodesVacances
                .Where(v => v.EndDate >= periodStart && v.StartDate <= periodEnd)
                .ToListAsync();

            bool IsVacanceSession(DateTime d) =>
                vacancesSession.Any(v => d >= v.StartDate.Date && d <= v.EndDate.Date);

            bool isTeleCoursCalc = courseName.Contains("Télécours", StringComparison.OrdinalIgnoreCase)
                                || courseName.Contains("TéléCours", StringComparison.OrdinalIgnoreCase);
            string sessionTypeCalc = isTeleCoursCalc ? "tele" : "fi";
            int targetDaysCalc = CalculerJoursCibles(sessionTypeCalc, 3); // 3 = trimestre complet

            double absScore = 0;
            int feriesCount = 0;
            for (var d = periodStart; d <= periodEnd; d = d.AddDays(1))
            {
                if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday) continue;

                if (feriesSession.Contains(d.Date))
                {
                    // TéléCours : pas de cours le mercredi ni le vendredi
                    if (!(isTeleCoursCalc && (d.DayOfWeek == DayOfWeek.Wednesday
                                          || d.DayOfWeek == DayOfWeek.Friday)))
                        feriesCount++;
                    continue;
                }
                if (IsVacanceSession(d.Date)) continue;

                string st = rawPresences.TryGetValue(d.Date, out var sv)
                    ? (string.IsNullOrWhiteSpace(sv) ? "p" : sv.Trim().ToLower()) : "p";
                if (st == "a" || st == "e") absScore += 1;
                else if (st == "r")         absScore += 0.5;
            }
            int    totalAffiche      = Math.Max(1, targetDaysCalc - feriesCount);
            double presentsExact     = Math.Max(0.0, totalAffiche - absScore);
            double tauxParticipation = Math.Min(100.0,
                Math.Ceiling(presentsExact / totalAffiche * 100.0 - 0.5));

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
            [FromQuery] int?   month          = null)
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

            var periodesVacances = await _db.PeriodesVacances
                .Where(v => v.EndDate >= periodStart && v.StartDate <= periodEnd)
                .ToListAsync();

            bool IsVacance(DateTime d) =>
                periodesVacances.Any(v => d >= v.StartDate.Date && d <= v.EndDate.Date);

            var fullPresences = new List<AttendanceEntryDto>();
            for (var date = periodStart; date <= periodEnd; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                string status;
                if (joursFeries.Contains(date.Date))   status = "f";
                else if (IsVacance(date.Date))         status = "v";
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