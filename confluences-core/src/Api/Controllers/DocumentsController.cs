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
        // --- ATTESTATION TRIMESTRE ---
        // --------------------------------------------------
        [HttpGet("attestation-trimestre/{stagiaireId}")]
        public IActionResult AttestationTrimestre(
            string stagiaireId,
            [FromQuery] int year,
            [FromQuery] string trimestre)
        {
            var stages = _db.Stages
                .Include(s => s.Stagiaire)
                // >>> AJOUTER L'INCLUSION DES PROPRIÉTÉS CLÉS DU STAGE <<<
                .Include(s => s.TypeIntershipActivity)
                .Include(s => s.TypeStage)
                .Include(s => s.Entreprise)
                // --------------------------------------------------------
                .Where(s => s.StagiaireId == stagiaireId)
                .ToList();

            if (!stages.Any())
                return NotFound("Aucun stage trouvé pour ce trimestre.");

            var stagiaire = stages.First().Stagiaire;

            var logoPath = Path.Combine(_env.WebRootPath, "Upload/images/logo.png");
            byte[] logoBytes = Array.Empty<byte>();

            if (System.IO.File.Exists(logoPath))
            {
                try
                {
                    logoBytes = System.IO.File.ReadAllBytes(logoPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lecture logo");
                }
            }

            var doc = new AttestationTrimestreDocument(stagiaire, stages, year, trimestre, logoBytes);
            var pdfBytes = doc.GeneratePdf();

            return File(
                pdfBytes,
                "application/pdf",
                $"attestation-{stagiaireId}-{year}-{trimestre}.pdf"
            );
        }

        // --------------------------------------------------
        // --- BILAN DE STAGE ---
        // --------------------------------------------------
        [HttpGet("bilan/{id}")]
        public IActionResult BilanStage(int id)
        {
            var stage = _db.Stages
                .Include(s => s.Stagiaire)
                // >>> AJOUTER L'INCLUSION DES PROPRIÉTÉS MANQUANTES UTILISÉES DANS LE PDF <<<
                .Include(s => s.TypeIntershipActivity)
                .Include(s => s.TypeMetier)          
                .Include(s => s.TypeStage)            
                .Include(s => s.Entreprise)           
                // -------------------------------------------------------------------------
                .FirstOrDefault(s => s.StageId == id);

            if (stage == null)
            {
                _logger.LogWarning("Stage ID {StageId} non trouvé pour génération du bilan.", id);
                return NotFound($"Stage ID {id} non trouvé.");
            }

            if (stage.Stagiaire == null)
            {
                _logger.LogError("Stagiaire non chargé pour le stage ID {StageId}.", id);
                return StatusCode(500, "Erreur: Stagiaire manquant.");
            }

            var logoPath = Path.Combine(_env.WebRootPath, "Upload/images/logo.png");
            byte[] logoBytes = Array.Empty<byte>();

            if (System.IO.File.Exists(logoPath))
            {
                try
                {
                    logoBytes = System.IO.File.ReadAllBytes(logoPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lecture logo pour le bilan");
                }
            }

            var doc = new BilanStageDocument(stage, stage.Stagiaire, logoBytes);
            var pdfBytes = doc.GeneratePdf();

            return File(
                pdfBytes,
                "application/pdf",
                $"bilan-stage-{id}-{stage.Stagiaire.LastName}.pdf"
            );
        }

        // ============================================================
        // === SAUVEGARDE DES DONNÉES (Venant de la Modale Vue.js) ===
        // ============================================================
        [HttpPost("save-bilan-session")]
public async Task<IActionResult> SaveBilanSession([FromBody] SaveBilanSessionRequest request)
{
    // 1. Validation basique
    if (request == null || string.IsNullOrWhiteSpace(request.StagiaireId))
    return BadRequest("Données invalides (ID manquant).");

    if (!request.Quarter.StartsWith("T"))
    request.Quarter = $"T{request.Quarter}";


    // 2. Recherche du rapport existant
    var report = await _db.SessionReports.FirstOrDefaultAsync(r =>
        r.StagiaireId == request.StagiaireId &&
        r.Year == request.Year &&
        r.Quarter == request.Quarter);

    bool isNew = (report == null);

    // 3. Création si n'existe pas
    if (isNew)
    {
        report = new SessionReport
        {
            StagiaireId = request.StagiaireId,
            Year = request.Year,
            Quarter = request.Quarter,
            CreatedAt = DateTime.Now // Pensez à initialiser la date
        };
        _db.SessionReports.Add(report);
    }

    // 4. MAPPING DES CHAMPS (Mise à jour)
    report.UpdatedAt = DateTime.Now; // Bonne pratique
    
    report.EvaluationText = request.EvaluationText;
    report.FollowUpActions = request.FollowUpActions;
    report.GlobalRate = request.GlobalRate;

    // --- SAUVEGARDE DES NOUVEAUX CHAMPS ---
    report.SocialInsertionText = request.SocialInsertionText;
    report.DigitalSkillsText = request.DigitalSkillsText;
    report.IndividualAccompanyingText = request.IndividualAccompanyingText;
    report.HomeVisitsText = request.HomeVisitsText;

    // Gestion des ateliers (si applicable)
    if (request.SelectedWorkshops != null)
    {
        report.WorkshopsJson = System.Text.Json.JsonSerializer.Serialize(request.SelectedWorkshops);
    }

    // 5. Sauvegarde en BDD
    try 
    {
        await _db.SaveChangesAsync();
        return Ok(new { message = isNew ? "Rapport créé." : "Rapport mis à jour." });
    }
    catch (Exception ex)
    {
        // Utile pour le débogage (voir l'erreur précise dans la console réseau du navigateur)
        return StatusCode(500, "Erreur BDD: " + ex.Message);
    }
}


[HttpGet("bilan-session-data/{stagiaireId}")]
public async Task<IActionResult> GetBilanSessionData(
    string stagiaireId,
    [FromQuery] int year,
    [FromQuery] string quarter)
{
    if (!quarter.StartsWith("T"))
        quarter = $"T{quarter}";

    var report = await _db.SessionReports
        .AsNoTracking()
        .Where(r =>
            r.StagiaireId == stagiaireId &&
            r.Year == year &&
            r.Quarter == quarter)
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
            r.WorkshopsJson
        })
        .FirstOrDefaultAsync();

    if (report == null)
        return Ok(null);

    return Ok(report);
}



        // === DOC 1 & 2 : DOCUMENTS DE SESSION (Nouveaux) ===
        // ============================================================

        // 1. BILAN DE SESSION (Le rapport textuel avec les commentaires)
        [HttpGet("bilan-session/{stagiaireId}")]
        public async Task<IActionResult> GenerateBilanSessionPdf(
            string stagiaireId,
            [FromQuery] int year,
            [FromQuery] string trimestre,
            [FromQuery] string mesure) // Paramètre passé depuis le Front
        {
            var stagiaire = await _userManager.FindByIdAsync(stagiaireId);
            if (stagiaire == null) return NotFound("Stagiaire introuvable.");
            
            if (!trimestre.StartsWith("T")) trimestre = $"T{trimestre}";

            var report = await _db.SessionReports.FirstOrDefaultAsync(r =>
                r.StagiaireId == stagiaireId && r.Year == year && r.Quarter == trimestre);

            if (report == null) return NotFound($"Aucun bilan trouvé pour {trimestre}/{year}.");

            // Détermination du nom du cours (Priorité au Front)
            string courseName = DetermineCourseName(mesure, null); // On peut passer la session ici si besoin
var (startDate, endDate) = GetQuarterDates(year, trimestre); // Récupérer les dates
            var logoBytes = GetLogoBytes();

            var doc = new BilanSessionDocument(
                stagiaire,
                report.EvaluationText,
                report.FollowUpActions,
                stagiaire.OralLevel,
                stagiaire.WrittenLevel,
                report.GlobalRate,
                year,
                trimestre,
                logoBytes,
                courseName,   // Passé au constructeur
    startDate,    // Passé au constructeur
    endDate       // Passé au constructeur
            );
            
            var pdfBytes = doc.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"Bilan-Session-{stagiaire.LastName}-{year}-{trimestre}.pdf");
        }
        // --- Attestations de session ---
        // --------------------------------------------------

private string DetermineCourseName(string frontalMesure, string dbSessionName)
        {
            string nameToAnalyze = !string.IsNullOrEmpty(frontalMesure) ? frontalMesure : dbSessionName;
            if (string.IsNullOrEmpty(nameToAnalyze)) return "Français & Intégration";

            if (nameToAnalyze.ToLower().Contains("télécours") || nameToAnalyze.ToLower().Contains("telecours"))
                return "Français en Télécours";
            
            return "Français & Intégration";
        }

        private byte[] GetLogoBytes()
        {
            var logoPath = Path.Combine(_env.WebRootPath, "Upload/images/logo.png");
            try
            {
                if (System.IO.File.Exists(logoPath))
                {
                    return System.IO.File.ReadAllBytes(logoPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lecture logo");
            }
            return Array.Empty<byte>();
        }

        private (DateTime Start, DateTime End) GetQuarterDates(int year, string quarter)
        {
            if (!int.TryParse(quarter.Replace("T", ""), out int q)) q = 1;

            int startMonth = (q - 1) * 3 + 1;
            DateTime start = new DateTime(year, startMonth, 1);
            DateTime end = start.AddMonths(3).AddDays(-1);

            return (start.Date, end.Date);
        }

        // ============================================================
        // --- ATTESTATION DE SESSION (RÉVISÉE) ---
        // ============================================================

        [HttpGet("attestation-session/{stagiaireId}")]
public async Task<IActionResult> GenerateAttestationSessionPdf(
    string stagiaireId,
    [FromQuery] int year,
    [FromQuery] string trimestre,
    [FromQuery] string mesure) 
{
    // 1. Initialisation
    if (!trimestre.StartsWith("T")) trimestre = $"T{trimestre}";
    var (startDate, endDate) = GetQuarterDates(year, trimestre);
    var logoBytes = GetLogoBytes();
    
    // 2. Charger le stagiaire avec ses sessions pour la logique de secours
    var stagiaire = await _db.Users
        .Include(u => u.SessionStudents).ThenInclude(ss => ss.Session).ThenInclude(s => s.SchoolClassRoom)
        .SingleOrDefaultAsync(u => u.Id == stagiaireId);

    if (stagiaire == null) return NotFound("Stagiaire introuvable.");

    // 3. LOGIQUE DE DÉTERMINATION DU COURS (Front-end Priority)
    string courseName;
    if (!string.IsNullOrEmpty(mesure))
    {
        courseName = mesure; 
    }
    else 
    {
        var currentSession = stagiaire.SessionStudents
            .Select(ss => ss.Session)
            .FirstOrDefault(s => s.DateStart <= endDate && s.DateEnd >= startDate);

        courseName = currentSession?.SchoolClassRoom?.SchoolClassRoomName ?? "Français & Intégration";
    }

    // Normalisation pour QuestPDF
    if (courseName.ToLower().Contains("télécours") || courseName.ToLower().Contains("telecours"))
        courseName = "Français en Télécours";
    else
        courseName = "Français & Intégration";

    // 4. RÉCUPÉRATION DES DONNÉES DU RAPPORT (Taux et Ateliers cochés)
    var report = await _db.SessionReports.FirstOrDefaultAsync(r =>
        r.StagiaireId == stagiaireId &&
        r.Year == year &&
        r.Quarter == trimestre);

    double tauxParticipation = report?.GlobalRate ?? 0;
    List<string> ateliersSuivis = new List<string>();

    if (report != null && !string.IsNullOrEmpty(report.WorkshopsJson))
    {
        try {
            var workshopArray = JsonSerializer.Deserialize<string[]>(report.WorkshopsJson);
            if (workshopArray != null) ateliersSuivis.AddRange(workshopArray);
        } catch { _logger.LogWarning("Erreur désérialisation ateliers pour {Id}", stagiaireId); }
    }

    // 5. RÉCUPÉRATION DES PRÉSENCES MATRICE (Ateliers réels)
    var matrixActivities = await _db.AccompanyingPresences
        .Where(ap => ap.StagiaireId == stagiaireId && ap.Date >= startDate && ap.Date <= endDate && ap.IsPresent == true)
        .Select(ap => ap.ActivityName)
        .Distinct()
        .ToListAsync();

    // Fusion unique et triée
    ateliersSuivis = ateliersSuivis.Union(matrixActivities).Distinct().OrderBy(a => a).ToList();

    // 6. GÉNÉRATION DU DOCUMENT
    var doc = new AttestationSessionDocument(
        stagiaire,
        courseName,
        stagiaire.OralLevel,
        stagiaire.WrittenLevel,
        ateliersSuivis,
        tauxParticipation,
        year,
        trimestre,
        logoBytes,
        startDate,
        endDate
    );

    var pdfBytes = doc.GeneratePdf();
    return File(pdfBytes, "application/pdf", $"Attestation-{stagiaire.LastName}-{year}-{trimestre}.pdf");
}

    } // <--- Fin de la classe DocumentsController 

    // --------------------------------------------------
    // --- DTO POUR LA SAUVEGARDE (A mettre ici) ---
    // --------------------------------------------------
    public class SaveBilanSessionRequest
{
    public string StagiaireId { get; set; }  
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