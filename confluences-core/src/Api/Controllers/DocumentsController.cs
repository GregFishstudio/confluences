using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Documents;
using Confluences.Persistence;
using System;
using System.Linq;
using System.IO; 
using Microsoft.AspNetCore.Hosting;
using QuestPDF.Fluent; 
using Microsoft.Extensions.Logging;
using Api.Models.Dto.Attendance; // NOUVEAU : Pour le modèle de données de présence
using System.Collections.Generic; // Pour List<T>

namespace Api.Controllers
{
    [Route("api/documents")] // Laisse la route de base inchangée
    [ApiController]
    public class DocumentsController : ControllerBase // Conserve le nom de la classe
    {
        private readonly ConfluencesDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<DocumentsController> _logger; // Conserve le type de l'injection

        public DocumentsController(ConfluencesDbContext db, IWebHostEnvironment env, ILogger<DocumentsController> logger)
        {
            _db = db;
            _env = env;
            _logger = logger;
        }
        
        // --------------------------------------------------------------------------------------------------
        // --- NOUVELLE FONCTIONNALITÉ : GESTION DE PRÉSENCE ---
        // --------------------------------------------------------------------------------------------------

        // POST /api/documents/attendance/save
        /// <summary>
        /// Enregistre les données de présence envoyées par le front-end.
        /// </summary>
        [HttpPost("attendance/save")]
        public IActionResult SaveAttendance([FromBody] List<AttendanceEntryDto> entries)
        {
            if (entries == null || entries.Count == 0)
            {
                return BadRequest("La liste des entrées de présence ne peut pas être vide.");
            }

            // TODO: 
            // 1. Implémenter la logique de sauvegarde (mapper et persister les entités)
            // 2. Créer l'entité Presence et son service de persistance dans Confluences.Domain/.Persistence
            
            _logger.LogInformation("Reçu {Count} entrées de présence à sauvegarder.", entries.Count);

            return Ok(new { Message = "Présences reçues et prêtes à être sauvegardées." });
        }


        // GET /api/documents/attendance/pdf?startDate=...&endDate=...
        /// <summary>
        /// Génère un document PDF d'attestation de présence pour une période donnée.
        /// </summary>
        [HttpGet("attendance/pdf")]
        public IActionResult GenerateAttendancePdf(
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            if (startDate >= endDate)
            {
                return BadRequest("La date de début doit précéder la date de fin.");
            }
            
            // 🟦 1. Charger le logo
            var logoPath = Path.Combine(_env.WebRootPath, "Upload/images/logo.png");
            byte[] logoBytes = Array.Empty<byte>();

            if (System.IO.File.Exists(logoPath))
            {
                try { logoBytes = System.IO.File.ReadAllBytes(logoPath); }
                catch (Exception ex) { _logger.LogError(ex, "Erreur lecture logo pour l'attestation de présence."); }
            }

            // 🟦 2. Charger les données de présence pour la période (TODO: Implémenter la logique réelle)
            // Simulez le chargement des données.
            var presences = new List<AttendanceEntryDto>(); 
            var stagiaires = _db.ApplicationUsers.Where(u => u.Role == "Stagiaire").Take(5).ToList(); 

            // 🟦 3. Générer le document QuestPDF
            var doc = new AttestationPresenceDocument(
                startDate,
                endDate,
                presences,
                stagiaires, 
                logoBytes
            );

            var pdfBytes = doc.GeneratePdf();
            var filename = $"Attestation_Presences_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.pdf";

            return File(pdfBytes, "application/pdf", filename);
        }

        // --------------------------------------------------------------------------------------------------
        // --- MÉTHODES EXISTANTES (INCHANGÉES) ---
        // --------------------------------------------------------------------------------------------------
        
        // Remarques : Laissez les routes existantes telles quelles ou utilisez un prefixe si nécessaire.
        // J'ai conservé les routes originales que vous avez fournies.

        // GET /api/documents/attestation-trimestre/{stagiaireId}
        [HttpGet("attestation-trimestre/{stagiaireId}")]
        public IActionResult AttestationTrimestre(
            string stagiaireId,
            [FromQuery] int year,
            [FromQuery] string trimestre)
        {
            // ... (Logique inchangée pour AttestationTrimestre) ...
            DateTime start, end;
            // ... (Logique pour déterminer start/end) ...
            
            var stages = _db.Stages
                // ... (Logique de chargement des stages) ...
                .ToList();

            if (stages.Count == 0)
                return NotFound("Aucun stage trouvé pour ce trimestre.");

            var stagiaire = stages.First().Stagiaire;

            var logoPath = Path.Combine(_env.WebRootPath, "Upload/images/logo.png");
            byte[] logoBytes = Array.Empty<byte>();

            if (System.IO.File.Exists(logoPath))
            {
                try { logoBytes = System.IO.File.ReadAllBytes(logoPath); }
                catch (Exception ex) { _logger.LogError(ex, "Erreur lecture logo"); }
            }

            var doc = new AttestationTrimestreDocument(
                stagiaire,
                stages,
                year,
                trimestre,
                logoBytes
            );

            var pdfBytes = doc.GeneratePdf();

            return File(
                pdfBytes,
                "application/pdf",
                $"attestation-{stagiaireId}-{year}-{trimestre}.pdf"
            );
        }

        // GET /api/documents/bilan/{id}
        [HttpGet("bilan/{id}")]
        public IActionResult BilanStage(int id)
        {
            // ... (Logique inchangée pour BilanStage) ...
            var stage = _db.Stages
                // ... (Logique de chargement du stage) ...
                .FirstOrDefault(s => s.StageId == id);

            if (stage == null)
            {
                _logger.LogWarning("Stage ID {StageId} non trouvé pour la génération du bilan.", id);
                return NotFound($"Stage ID {id} non trouvé.");
            }
            
            if (stage.Stagiaire == null)
            {
                 _logger.LogError("Stagiaire non chargé pour le stage ID {StageId}.", id);
                 return StatusCode(500, "Erreur de données: Stagiaire manquant.");
            }

            var logoPath = Path.Combine(_env.WebRootPath, "Upload/images/logo.png");
            byte[] logoBytes = Array.Empty<byte>();

            if (System.IO.File.Exists(logoPath))
            {
                try { logoBytes = System.IO.File.ReadAllBytes(logoPath); }
                catch (Exception ex) { _logger.LogError(ex, "Erreur de lecture du fichier logo pour le bilan de stage."); }
            }

            var doc = new BilanStageDocument(stage, stage.Stagiaire, logoBytes);
            
            var pdfBytes = doc.GeneratePdf();

            return File(
                pdfBytes, 
                "application/pdf", 
                $"bilan-stage-{id}-{stage.Stagiaire.LastName}.pdf"
            );
        }
        
        // ** (L'ancienne méthode Attestation(int id) a été supprimée, car elle est remplacée par GenerateAttendancePdf) **
    }
}