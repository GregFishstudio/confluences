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
            _db = db;
            _env = env;
            _logger = logger;
            _userManager = userManager;
        }

        // --------------------------------------------------
        // --- ATTESTATION TRIMESTRE DE STAGE (CONSERVÉ) ---
        // --------------------------------------------------
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

            var stagiaire = stages.First().Stagiaire;
            var logoBytes = GetLogoBytes();

            var doc = new AttestationTrimestreDocument(stagiaire, stages, year, trimestre, logoBytes);
            var pdfBytes = doc.GeneratePdf();

            return File(pdfBytes, "application/pdf", $"attestation-{stagiaireId}-{year}-{trimestre}.pdf");
        }

        // --------------------------------------------------
        // --- BILAN DE STAGE (CONSERVÉ) ---
        // --------------------------------------------------
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

            var logoBytes = GetLogoBytes();
            var doc = new BilanStageDocument(stage, stage.Stagiaire, logoBytes);
            var pdfBytes = doc.GeneratePdf();

            return File(pdfBytes, "application/pdf", $"bilan-stage-{id}-{stage.Stagiaire.LastName}.pdf");
        }

        // --------------------------------------------------
        // --- SAUVEGARDE DES BILANS DE SESSION (CONSERVÉ) ---
        // --------------------------------------------------
        [HttpPost("save-bilan-session")]
        public async Task<IActionResult> SaveBilanSession([FromBody] SaveBilanSessionRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest("Données invalides (ID utilisateur manquant).");

            if (!request.Quarter.StartsWith("T"))
                request.Quarter = $"T{request.Quarter}";

            var report = await _db.SessionReports.FirstOrDefaultAsync(r =>
                r.StagiaireId == request.UserId &&
                r.Year == request.Year &&
                r.Quarter == request.Quarter);

            bool isNew = (report == null);
            if (isNew)
            {
                report = new SessionReport
                {
                    StagiaireId = request.UserId,
                    Year = request.Year,
                    Quarter = request.Quarter,
                    CreatedAt = DateTime.Now
                };
                _db.SessionReports.Add(report);
            }

            report.UpdatedAt = DateTime.Now;
            report.EvaluationText = request.EvaluationText;
            report.FollowUpActions = request.FollowUpActions;
            report.GlobalRate = request.GlobalRate;
            report.SocialInsertionText = request.SocialInsertionText;
            report.DigitalSkillsText = request.DigitalSkillsText;
            report.IndividualAccompanyingText = request.IndividualAccompanyingText;
            report.HomeVisitsText = request.HomeVisitsText;

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

        [HttpGet("bilan-session-data/{userId}")]
        public async Task<IActionResult> GetBilanSessionData(
            string userId,
            [FromQuery] int year,
            [FromQuery] string quarter)
        {
            if (!quarter.StartsWith("T")) quarter = $"T{quarter}";

            var report = await _db.SessionReports
                .AsNoTracking()
                .Where(r => r.StagiaireId == userId && r.Year == year && r.Quarter == quarter)
                .Select(r => new
                {
                    r.StagiaireId, r.Year, r.Quarter,
                    r.EvaluationText, r.FollowUpActions, r.GlobalRate,
                    r.SocialInsertionText, r.DigitalSkillsText,
                    r.IndividualAccompanyingText, r.HomeVisitsText, r.WorkshopsJson
                })
                .FirstOrDefaultAsync();

            return Ok(report);
        }

        // --------------------------------------------------
        // --- GÉNÉRATION PDF BILAN DE SESSION ---
        // Utilise les vraies dates de la session en DB
        // --------------------------------------------------
        [HttpGet("bilan-session/{userId}")]
        public async Task<IActionResult> GenerateBilanSessionPdf(
            string userId,
            [FromQuery] int year,
            [FromQuery] string trimestre,
            [FromQuery] string mesure,
            [FromQuery] string? sessionName = null)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("Utilisateur introuvable.");

            if (!trimestre.StartsWith("T")) trimestre = $"T{trimestre}";

            var report = await _db.SessionReports.FirstOrDefaultAsync(r =>
                r.StagiaireId == userId && r.Year == year && r.Quarter == trimestre);

            if (report == null) return NotFound($"Aucun bilan trouvé pour {trimestre}/{year}.");

            string courseName = DetermineCourseName(mesure, null);

            // ── Vraies dates depuis la session en DB ──────────────────────────
            var (startDate, endDate) = await ResolveSessionDates(year, trimestre, mesure, sessionName);

            var doc = new BilanSessionDocument(
                user,
                report.EvaluationText,
                report.FollowUpActions,
                user.OralLevel,
                user.WrittenLevel,
                report.GlobalRate,
                year,
                trimestre,
                GetLogoBytes(),
                courseName,
                startDate,
                endDate,
                report.SocialInsertionText,
                report.DigitalSkillsText,
                report.IndividualAccompanyingText,
                report.HomeVisitsText
            );

            return File(doc.GeneratePdf(), "application/pdf", $"Bilan-Session-{user.LastName}.pdf");
        }

        // --------------------------------------------------
        // --- ATTESTATION DE SESSION ---
        // Utilise les vraies dates de la session en DB
        // --------------------------------------------------
        [HttpGet("attestation-session/{userId}")]
        public async Task<IActionResult> GenerateAttestationSessionPdf(
            string userId,
            [FromQuery] int year,
            [FromQuery] string trimestre,
            [FromQuery] string mesure,
            [FromQuery] string? sessionName = null)
        {
            if (!trimestre.StartsWith("T")) trimestre = $"T{trimestre}";

            // ── Vraies dates depuis la session en DB ──────────────────────────
            var (startDate, endDate) = await ResolveSessionDates(year, trimestre, mesure, sessionName);

            var user = await _db.Users
                .Include(u => u.SessionStudents).ThenInclude(ss => ss.Session).ThenInclude(s => s.SchoolClassRoom)
                .SingleOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound("Stagiaire introuvable.");

            string courseName = DetermineCourseName(mesure,
                user.SessionStudents
                    .Select(ss => ss.Session)
                    .FirstOrDefault(s => s.DateStart <= endDate && s.DateEnd >= startDate)
                    ?.SchoolClassRoom?.SchoolClassRoomName);

            var report = await _db.SessionReports.FirstOrDefaultAsync(r =>
                r.StagiaireId == userId && r.Year == year && r.Quarter == trimestre);

            var presencesParActivite = await _db.AccompanyingPresences
                .Where(ap => ap.UserId == userId && ap.Date >= startDate && ap.Date <= endDate)
                .GroupBy(ap => ap.ActivityName)
                .Select(g => new { ActivityKey = g.Key, Count = g.Count() })
                .ToListAsync();

            var activityKeys = presencesParActivite.Select(p => p.ActivityKey).ToList();
            var columns = await _db.MatrixColumns
                .Where(c => activityKeys.Contains(c.Key))
                .Select(c => new { c.Key, c.Label, c.Type })
                .ToListAsync();

            var ateliersSuivis = presencesParActivite
                .Join(columns, p => p.ActivityKey, c => c.Key,
                      (p, c) => $"{c.Label}|{c.Type}|{p.Count}")
                .OrderBy(a => a)
                .ToList();

            var doc = new AttestationSessionDocument(
                user,
                courseName,
                user.OralLevel,
                user.WrittenLevel,
                ateliersSuivis,
                report?.GlobalRate ?? 0,
                year,
                trimestre,
                GetLogoBytes(),
                GetSignatureBytes(),
                startDate,   // ← vraies dates
                endDate      // ← vraies dates
            );

            return File(doc.GeneratePdf(), "application/pdf", $"Attestation-{user.LastName}.pdf");
        }

        // --------------------------------------------------
        // --- ATTESTATION DE PRÉSENCE ---
        // --------------------------------------------------
        [HttpGet("attestationpresence/{userId}")]
        public async Task<IActionResult> GenerateAttestationPresencePdf(
            string userId,
            [FromQuery] int?    year         = null,
            [FromQuery] int?    month        = null,
            [FromQuery] string? startDate    = null,
            [FromQuery] string? endDate      = null,
            [FromQuery] string? sessionName  = null,
            [FromQuery] string? sessionType  = "fi",
            [FromQuery] int?    sessionMonth = null)
        {
            var frenchCulture = new System.Globalization.CultureInfo("fr-FR");

            // ── 1. Résolution de la période ──────────────────────────────────
            DateTime periodStart;
            DateTime printedMonthEnd;
            string   periodLabel;

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                if (!DateTime.TryParse(startDate, out periodStart) ||
                    !DateTime.TryParse(endDate,   out var sessionEndDate))
                    return BadRequest("Dates de session invalides (attendu : yyyy-MM-dd).");

                periodStart     = periodStart.Date;
                printedMonthEnd = new DateTime(sessionEndDate.Year, sessionEndDate.Month,
                    DateTime.DaysInMonth(sessionEndDate.Year, sessionEndDate.Month));

                periodLabel = !string.IsNullOrEmpty(sessionName)
                    ? $"Session {sessionName}"
                    : $"{periodStart:dd/MM/yyyy} → {printedMonthEnd:dd/MM/yyyy}";
            }
            else if (year.HasValue && month.HasValue)
            {
                if (month < 1 || month > 12)
                    return BadRequest("Le mois doit être compris entre 1 et 12.");

                printedMonthEnd = new DateTime(year.Value, month.Value,
                    DateTime.DaysInMonth(year.Value, month.Value));
                periodStart = new DateTime(year.Value, month.Value, 1);
                periodLabel = periodStart.ToString("MMMM yyyy", frenchCulture);
            }
            else
            {
                return BadRequest("Veuillez fournir soit (year + month) soit (startDate + endDate).");
            }

            // ── 2. Recherche de la session en DB ─────────────────────────────
            // Priorité : nom exact → type+chevauchement → pas de session
            SessionFormation? dbSession = null;

            if (!string.IsNullOrEmpty(sessionName))
            {
                dbSession = await _db.SessionFormations
                    .FirstOrDefaultAsync(s => s.Name == sessionName);
            }

            if (dbSession == null)
            {
                dbSession = await _db.SessionFormations
                    .Where(s =>
                        printedMonthEnd >= s.StartDate &&
                        printedMonthEnd <= s.EndDate &&
                        (
                            s.Type == (sessionType ?? "") ||
                            (s.Type.Contains("fi") && !(sessionType ?? "").Contains("tele")) ||
                            (s.Type == "telecours" && (sessionType ?? "").Contains("tele"))
                        ))
                    .OrderByDescending(s => s.StartDate)
                    .FirstOrDefaultAsync();
            }

            // ── 3. Les vraies dates de la session remplacent le fallback ──────
            if (dbSession != null)
            {
                periodStart = dbSession.StartDate.Date;
                // Fin imprimée = fin de la session entière (pas juste fin du mois)
                printedMonthEnd = dbSession.EndDate.Date;
            }

            // ── 4. Contexte session ──────────────────────────────────────────
            int moisSession = sessionMonth ?? DeduireMonsDansSession(printedMonthEnd, dbSession);
            int targetDays  = CalculerJoursCibles(dbSession?.Type ?? sessionType ?? "fi", moisSession);

            if (!sessionMonth.HasValue && dbSession != null)
            {
                var monthsIn = ((printedMonthEnd.Year  - dbSession.StartDate.Year) * 12
                               + printedMonthEnd.Month - dbSession.StartDate.Month) + 1;
                moisSession = Math.Max(1, Math.Min(3, monthsIn));
                targetDays  = CalculerJoursCibles(dbSession.Type ?? sessionType ?? "fi", moisSession);
            }

            var sessionCtx = new SessionContext
            {
                SessionMonth      = moisSession,
                SessionType       = dbSession?.Type ?? sessionType ?? "fi",
                SessionTargetDays = targetDays,
                SessionName       = sessionName ?? dbSession?.Name ?? periodLabel,
                SessionStartDate  = periodStart,
            };

            // ── 5. Utilisateur ───────────────────────────────────────────────
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound("Stagiaire introuvable.");

            // ── 6. Présences sur toute la plage de la session ────────────────
            var displayStart = periodStart;
            var displayEnd   = printedMonthEnd;

            var existingPresences = await _db.Presences
                .AsNoTracking()
                .Where(p => p.StagiaireId == userId
                            && p.Date >= displayStart
                            && p.Date <= displayEnd)
                .ToDictionaryAsync(p => p.Date.Date, p => p.Statut);

            var joursFeries = await _db.JoursFeries
                .Where(f => f.Date >= displayStart && f.Date <= displayEnd)
                .Select(f => f.Date.Date)
                .ToListAsync();

            var periodesVacances = await _db.PeriodesVacances
                .Where(v => v.EndDate >= displayStart && v.StartDate <= displayEnd)
                .ToListAsync();

            bool IsVacance(DateTime d) =>
                periodesVacances.Any(v => d >= v.StartDate.Date && d <= v.EndDate.Date) ||
                (dbSession?.VacanceStart.HasValue == true &&
                 d >= dbSession.VacanceStart!.Value.Date &&
                 dbSession.VacanceEnd.HasValue &&
                 d <= dbSession.VacanceEnd!.Value.Date);

            var fullMonthPresences = new List<AttendanceEntryDto>();
            for (var date = displayStart; date <= displayEnd; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                string status;
                if (joursFeries.Contains(date.Date))
                    status = "f";
                else if (IsVacance(date.Date))
                    status = "v";
                else
                    status = existingPresences.TryGetValue(date.Date, out var s)
                        ? (string.IsNullOrWhiteSpace(s) ? "p" : s.Trim().ToLower())
                        : "p";

                fullMonthPresences.Add(new AttendanceEntryDto
                {
                    StagiaireId = userId,
                    Date        = date,
                    Statut      = status
                });
            }

            // ── 7. Génération PDF ────────────────────────────────────────────
            var doc = new AttestationPresenceDocument(
                periodStart,
                printedMonthEnd,
                fullMonthPresences,
                new List<ApplicationUser> { user },
                GetLogoBytes(),
                null,
                sessionCtx
            );

            var fileName = string.IsNullOrEmpty(sessionName)
                ? $"Presence-{user.LastName}-{printedMonthEnd:yyyy-MM}.pdf"
                : $"Presence-{user.LastName}-{sessionName}.pdf";

            return File(doc.GeneratePdf(), "application/pdf", fileName);
        }

        // --------------------------------------------------
        // --- HELPERS PRIVÉS ---
        // --------------------------------------------------

        /// <summary>
        /// Résout les dates réelles d'une session :
        /// 1) par nom exact   2) par type + chevauchement trimestre   3) fallback calendaire
        /// </summary>
        private async Task<(DateTime Start, DateTime End)> ResolveSessionDates(
            int year, string trimestre, string mesure, string? sessionName)
        {
            if (!string.IsNullOrEmpty(sessionName))
            {
                var byName = await _db.SessionFormations
                    .FirstOrDefaultAsync(s => s.Name == sessionName);
                if (byName != null)
                    return (byName.StartDate.Date, byName.EndDate.Date);
            }

            var (fallbackStart, fallbackEnd) = GetQuarterDates(year, trimestre);
            bool isTele = (mesure ?? "").ToLower().Contains("tele");

            var byType = await _db.SessionFormations
                .Where(s =>
                    s.EndDate >= fallbackStart &&
                    s.StartDate <= fallbackEnd &&
                    (isTele
                        ? s.Type == "telecours"
                        : (s.Type == "fi_a1" || s.Type == "fi_a2" || s.Type.StartsWith("fi"))))
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync();

            if (byType != null)
                return (byType.StartDate.Date, byType.EndDate.Date);

            return (fallbackStart, fallbackEnd);
        }

        private static int DeduireMonsDansSession(DateTime date, SessionFormation? session = null)
        {
            if (session?.StartDate != null)
            {
                var months = ((date.Year  - session.StartDate.Year) * 12
                             + date.Month - session.StartDate.Month) + 1;
                return Math.Max(1, Math.Min(3, months));
            }
            int quarterStartMonth = ((date.Month - 1) / 3) * 3 + 1;
            return Math.Max(1, Math.Min(3, date.Month - quarterStartMonth + 1));
        }

        private static int CalculerJoursCibles(string sessionType, int moisSession)
        {
            bool isTelecours = sessionType?.ToLower().Contains("tele") == true;
            return (isTelecours, moisSession) switch
            {
                (true,  1) => 10,
                (true,  2) => 20,
                (true,  _) => 32,
                (false, 1) => 20,
                (false, 2) => 40,
                (false, _) => 60,
            };
        }

        private string DetermineCourseName(string frontalMesure, string? dbSessionName)
        {
            string nameToAnalyze = !string.IsNullOrEmpty(frontalMesure) ? frontalMesure : dbSessionName ?? "";
            if (string.IsNullOrEmpty(nameToAnalyze)) return "Français & Intégration";
            var lower = nameToAnalyze.ToLower();
            if (lower.Contains("telecours") || lower.Contains("télécours")) return "Français en Télécours";
            if (lower.Contains("a2")) return "Français & Intégration (Niveau A2)";
            if (lower.Contains("a1")) return "Français & Intégration (Niveau A1)";
            return "Français & Intégration";
        }

        private byte[] GetLogoBytes()
        {
            var logoPath = Path.Combine(_env.WebRootPath, "Upload/images/logo.png");
            try { if (System.IO.File.Exists(logoPath)) return System.IO.File.ReadAllBytes(logoPath); }
            catch (Exception ex) { _logger.LogError(ex, "Erreur lecture logo"); }
            return Array.Empty<byte>();
        }

        private byte[] GetSignatureBytes()
        {
            var path = Path.Combine(_env.WebRootPath, "Upload/images/signature.png");
            try { if (System.IO.File.Exists(path)) return System.IO.File.ReadAllBytes(path); }
            catch (Exception ex) { _logger.LogError(ex, "Erreur lecture signature"); }
            return Array.Empty<byte>();
        }

        private (DateTime Start, DateTime End) GetQuarterDates(int year, string quarter)
        {
            if (!int.TryParse(quarter.Replace("T", ""), out int q)) q = 1;
            DateTime start = new DateTime(year, (q - 1) * 3 + 1, 1);
            return (start.Date, start.AddMonths(3).AddDays(-1).Date);
        }
    }

    // --------------------------------------------------
    // --- DTO POUR LA SAUVEGARDE ---
    // --------------------------------------------------
    public class SaveBilanSessionRequest
    {
        public string UserId { get; set; }
        public string StagiaireId { get => UserId; set => UserId = value; }
        public int Year { get; set; }
        public string Quarter { get; set; }
        public string EvaluationText { get; set; }
        public string FollowUpActions { get; set; }
        public double GlobalRate { get; set; }
        public string SocialInsertionText { get; set; }
        public string DigitalSkillsText { get; set; }
        public string IndividualAccompanyingText { get; set; }
        public string HomeVisitsText { get; set; }
        public List<string> SelectedWorkshops { get; set; }
    }
}