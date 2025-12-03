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
        .Include(s => s.TypeIntershipActivity) // <-- FIX pour l'Activité
        .Include(s => s.TypeMetier)           // <-- Utile pour "Métier"
        .Include(s => s.TypeStage)            // <-- Utile pour "Taux"
        .Include(s => s.Entreprise)           // <-- Utile pour "Entreprise"
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
    if (request == null || string.IsNullOrEmpty(request.StagiaireId))
        return BadRequest("Données invalides.");

    // Chercher un rapport existant pour ce stagiaire et ce trimestre
    var report = await _db.SessionReports.FirstOrDefaultAsync(r => 
        r.StagiaireId == request.StagiaireId && 
        r.Year == request.Year && 
        r.Quarter == request.Quarter);

    bool isNew = (report == null);

    if (isNew)
    {
        report = new SessionReport
        {
            StagiaireId = request.StagiaireId,
            Year = request.Year,
            Quarter = request.Quarter
        };
        _db.SessionReports.Add(report);
    }
    
    // Mettre à jour les champs
    report.EvaluationText = request.EvaluationText;
    report.FollowUpActions = request.FollowUpActions;
    report.GlobalRate = request.GlobalRate;
    // Sérialiser la liste des ateliers pour la stocker
    report.WorkshopsJson = System.Text.Json.JsonSerializer.Serialize(request.SelectedWorkshops);
    
    await _db.SaveChangesAsync();

    return Ok(new { message = isNew ? "Rapport créé et sauvegardé." : "Rapport mis à jour." });
}
        // === DOC 1 & 2 : DOCUMENTS DE SESSION (Nouveaux) ===
        // ============================================================

        // 1. BILAN DE SESSION (Le rapport textuel avec les commentaires)
       [HttpGet("bilan-session/{stagiaireId}")]
public async Task<IActionResult> GenerateBilanSessionPdf(
    string stagiaireId, 
    [FromQuery] int year, 
    [FromQuery] string trimestre)
{
    var stagiaire = await _userManager.FindByIdAsync(stagiaireId);
    if (stagiaire == null) return NotFound("Stagiaire introuvable.");

    // >>>>>>> CODE CLEF : RÉCUPÉRATION DU RAPPORT SAUVEGARDÉ <<<<<<<
    var report = await _db.SessionReports.FirstOrDefaultAsync(r =>
        r.StagiaireId == stagiaireId &&
        r.Year == year &&
        r.Quarter == trimestre);

    // Si aucun rapport n'existe, on peut utiliser des valeurs par défaut
    if (report == null)
    {
        // Gérer le cas où le rapport n'existe pas encore (retour 404 ou doc vide)
        return NotFound($"Aucun bilan trouvé pour le trimestre {trimestre}/{year}.");
    }
    // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

    // On utilise les VRAIES données du rapport
    var evaluationText = report.EvaluationText;
    var followUpActions = report.FollowUpActions;
    var taux = report.GlobalRate;

    var logoBytes = GetLogoBytes();

    // Création du document QuestPDF spécifique "BilanSessionDocument"
    var doc = new BilanSessionDocument(stagiaire, evaluationText, followUpActions, taux, year, trimestre, logoBytes);
    var pdfBytes = doc.GeneratePdf();

    return File(pdfBytes, "application/pdf", $"Bilan-Session-{stagiaire.LastName}-{year}-T{trimestre}.pdf");
}
        // --- Attestations de session ---
        // --------------------------------------------------

        [HttpGet("attestation-session/{stagiaireId}")]
        public async Task<IActionResult> GenerateAttestationSessionPdf(
            string stagiaireId, 
            [FromQuery] int year, 
            [FromQuery] string trimestre)
        {
            var stagiaire = await _userManager.FindByIdAsync(stagiaireId);
            if (stagiaire == null) return NotFound("Stagiaire introuvable.");

            // RÉCUPÉRATION DES ATELIERS SAUVEGARDÉS
            // var report = _db.SessionReports.FirstOrDefault(...);
            // List<string> ateliers = report.Workshops.Split(';').ToList();
            
            // Mockup :
            var ateliers = new List<string> { "Informatique", "Cuisine", "Théâtre" }; // Vient de la DB
            var taux = 91.0; // Vient de la DB

            var logoBytes = GetLogoBytes();

            // Création du document QuestPDF spécifique "AttestationSessionDocument"
            // Vous devez créer cette classe dans votre dossier Documents
            var doc = new AttestationSessionDocument(stagiaire, ateliers, taux, year, trimestre, logoBytes);
            var pdfBytes = doc.GeneratePdf();

            return File(pdfBytes, "application/pdf", $"Attestation-Session-{stagiaire.LastName}-{year}-T{trimestre}.pdf");
        }
    private byte[] GetLogoBytes()
        {
            var logoPath = Path.Combine(_env.WebRootPath, "Upload/images/logo.png");
            if (System.IO.File.Exists(logoPath))
            {
                try
                {
                    return System.IO.File.ReadAllBytes(logoPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lecture logo");
                }
            }
            return Array.Empty<byte>();
        }

    } // <--- Fin de la classe DocumentsController (Ne pas supprimer)

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
        public string[] SelectedWorkshops { get; set; }
        public double GlobalRate { get; set; }
    }
}
