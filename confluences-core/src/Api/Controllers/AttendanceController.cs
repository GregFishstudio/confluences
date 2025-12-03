using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Confluences.Persistence;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Collections.Generic;
// FIX: Use explicit namespace reference (alias) instead of simple using statement
using ApiAttendanceDto = Api.Models.Dto.Attendance.AttendanceEntryDto; 
using Confluences.Domain.Entities;
using Api.Documents;
using QuestPDF.Fluent; 
using Microsoft.EntityFrameworkCore; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Api.Controllers
{
    // FIX: Enforce Bearer scheme explicitly on the controller level
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/attendance")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly ConfluencesDbContext _db;
        private readonly IWebHostEnvironment _env; 
        private readonly ILogger<AttendanceController> _logger;

        public AttendanceController(
            ConfluencesDbContext db,
            IWebHostEnvironment env,
            ILogger<AttendanceController> logger
            )
        {
            _db = db;
            _env = env;
            _logger = logger;

            // Tentative d'application des migrations (pour le développement)
            try
            {
                if (_db.Database.GetPendingMigrations().Any())
                {
                    _db.Database.Migrate(); 
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to apply pending migrations during controller initialization.");
            }
        }

        // --------------------------------------------------
        // --- SAVE ATTENDANCE ---
        // --------------------------------------------------
        [HttpPost("save")]
        public async Task<IActionResult> SaveAttendance([FromBody] List<ApiAttendanceDto> entries) 
        {
            if (entries == null || entries.Count == 0)
                return BadRequest("La liste des entrées de présence ne peut pas être vide.");

            _logger.LogInformation("Reçu {Count} entrées de présence à sauvegarder.", entries.Count);

            try
            {
                foreach (var entry in entries)
                {
                    if (string.IsNullOrEmpty(entry.StagiaireId)) 
                    {
                        _logger.LogWarning("Skipping attendance entry due to missing or empty StagiaireId.");
                        continue;
                    }
                    
                    var normalizedDate = entry.Date.Date;
                    var stagiaireIdString = entry.StagiaireId;

                    var presence = await _db.Presences
                        .FirstOrDefaultAsync(p => p.StagiaireId == stagiaireIdString && p.Date == normalizedDate);

                    if (presence == null)
                    {
                        presence = new Presence
                        {
                            StagiaireId = stagiaireIdString,
                            Date = normalizedDate,
                            Statut = entry.Statut,
                            CreatedAt = DateTime.UtcNow
                        };
                        _db.Presences.Add(presence);
                    }
                    else
                    {
                        presence.Statut = entry.Statut;
                        presence.LastModifiedAt = DateTime.UtcNow;
                        _db.Presences.Update(presence);
                    }
                }
                
                await _db.SaveChangesAsync();
                return Ok(new { Message = $"Sauvegarde réussie de {entries.Count} entrées de présence." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur interne lors de la sauvegarde des présences.");
                return StatusCode(500, new { Error = "Une erreur interne est survenue lors de la sauvegarde des présences (DB Error).", Detail = ex.Message });
            }
        }

        // --------------------------------------------------
        // --- GET ATTENDANCE BY WEEK (Pour le Frontend) ---
        // --------------------------------------------------
        // Endpoint: GET /api/attendance/byWeek?start=YYYY-MM-DDT00:00:00Z&end=YYYY-MM-DDT00:00:00Z
        [HttpGet("byWeek")]
        public async Task<ActionResult<List<ApiAttendanceDto>>> GetAttendancesByWeek(
            [FromQuery] DateTime start, 
            [FromQuery] DateTime end)
        {
            if (start > end)
                return BadRequest("La date de début ne peut pas être postérieure à la date de fin.");

            try
            {
                var startDate = start.Date; 
                var endDate = end.Date; 

                // 1. Récupérer toutes les présences pour la période demandée.
                // CORRECTION: Suppression de la référence erronée à 'stagiaireId' et utilisation de la plage de dates correcte.
                var presences = await _db.Presences
                    // Utiliser >= startDate et <= endDate (inclusif) pour couvrir tous les jours.
                    .Where(p => p.Date >= startDate && p.Date < endDate)
                    // 2. Projeter vers le DTO
                    .Select(p => new ApiAttendanceDto 
                    {
                        StagiaireId = p.StagiaireId,
                        Date = p.Date,
                        Statut = p.Statut
                    })
                    .AsNoTracking()
                    .ToListAsync();

                // CORRECTION CS1503: Log correct
                _logger.LogInformation(
                    "Récupération de {Count} entrées de présence entre {Start} et {End}.", 
                    presences.Count, 
                    startDate.ToShortDateString(), 
                    endDate.ToShortDateString());

                return Ok(presences);
            }
            catch (Exception ex)
            {
                // CORRECTION CS1503: Log correct
                _logger.LogError(ex, "Erreur interne lors de la récupération des présences par semaine.");
                return StatusCode(500, "Erreur interne du serveur lors de la récupération des présences.");
            }
        }


        // --------------------------------------------------
        // --- ATTESTATION MENSUELLE (Pour le PDF) ---
        // --------------------------------------------------
        // Endpoint: GET /api/attendance/attestation-mensuelle/{stagiaireId}?year=YYYY&month=MM
        [HttpGet("attestation-mensuelle/{stagiaireId}")]
        public async Task<IActionResult> GenerateAttestationMensuelle(
            string stagiaireId,
            [FromQuery] int year, 
            [FromQuery] int month) 
        {
            if (string.IsNullOrEmpty(stagiaireId) || year == 0 || month < 1 || month > 12)
                return BadRequest("Les paramètres ID du stagiaire, Année, et Mois sont requis.");

            // Helper: Détermine la plage de dates du mois
            if (!TryGetDateRangeForMonth(year, month, out DateTime startDate, out DateTime exclusiveEndDate))
            {
                return StatusCode(500, "Erreur interne lors du calcul de la plage de dates.");
            }
            
            // 1. Charger les informations du stagiaire
            var stagiaire = await _db.Users
                .Where(u => u.Id == stagiaireId)
                .ToListAsync();
                
            if (stagiaire == null || !stagiaire.Any())
                return NotFound("Stagiaire non trouvé.");

            // 2. Charger les présences UNIQUEMENT pour ce stagiaire dans la période
            // Utilisation de la borne exclusive (p.Date < exclusiveEndDate) pour inclure tout le dernier jour
            var presences = await _db.Presences
                .Where(p => p.StagiaireId == stagiaireId 
                            && p.Date >= startDate.Date 
                            && p.Date < exclusiveEndDate.Date) // IMPORTANT: Utilisation de la borne exclusive
                .Select(p => new ApiAttendanceDto 
                {
                    StagiaireId = p.StagiaireId,
                    Date = p.Date,
                    Statut = p.Statut
                })
                .ToListAsync();

            // 3. Charger le logo
            var logoPath = Path.Combine(_env.WebRootPath, "Upload/images/logo.png");
            byte[] logoBytes = Array.Empty<byte>();
            if (System.IO.File.Exists(logoPath))
            {
                try { logoBytes = System.IO.File.ReadAllBytes(logoPath); }
                catch (Exception ex) { _logger.LogError(ex, "Erreur lecture logo pour l'attestation."); }
            }

            // 4. Générer le document 
            var doc = new AttestationPresenceDocument(
                startDate,          // Date de début du mois (inclusive)
                exclusiveEndDate.AddDays(-1), // Date de fin du mois (pour l'affichage sur le PDF)
                presences, 
                stagiaire, 
                logoBytes
            );

            var pdfBytes = doc.GeneratePdf();

            var filename = $"Attestation_Presence_{stagiaire[0].UserName}_{year}_{month:D2}.pdf";

            return File(pdfBytes, "application/pdf", filename);
        }

        // --------------------------------------------------
        // --- Private Helper: Logique de conversion Mois -> Plage de dates ---
        // --------------------------------------------------
        /// <summary>
        /// Calcule la plage de dates d'un mois. La date de fin est le début du mois suivant (exclusive).
        /// </summary>
        /// <returns>True si les dates sont valides, False sinon. startDate est inclusive, endDate est exclusive (début du mois suivant).</returns>
        private bool TryGetDateRangeForMonth(int year, int month, out DateTime startDate, out DateTime exclusiveEndDate)
        {
            try
            {
                // Début du mois (inclusif)
                startDate = new DateTime(year, month, 1).Date;
                
                // Fin du mois (exclusive) : Début du mois suivant.
                exclusiveEndDate = startDate.AddMonths(1).Date; 
                
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                startDate = default;
                exclusiveEndDate = default;
                return false;
            }
        }
    }
}