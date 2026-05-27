using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Confluences.Persistence;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using ApiAttendanceDto = Api.Models.Dto.Attendance.AttendanceEntryDto; 
using Confluences.Domain.Entities;
using Api.Documents;
using QuestPDF.Fluent; 
using Microsoft.EntityFrameworkCore; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Api.Controllers
{
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
            ILogger<AttendanceController> logger)
        {
            _db = db;
            _env = env;
            _logger = logger;

            try
            {
                if (_db.Database.GetPendingMigrations().Any())
                    _db.Database.Migrate(); 
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
                    
                    var normalizedDate    = entry.Date.Date;
                    var stagiaireIdString = entry.StagiaireId;

                    var presence = await _db.Presences
                        .FirstOrDefaultAsync(p => p.StagiaireId == stagiaireIdString && p.Date == normalizedDate);

                    if (presence == null)
                    {
                        presence = new Presence
                        {
                            StagiaireId = stagiaireIdString,
                            Date        = normalizedDate,
                            Statut      = entry.Statut,
                            CreatedAt   = DateTime.UtcNow
                        };
                        _db.Presences.Add(presence);
                    }
                    else
                    {
                        presence.Statut         = entry.Statut;
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
                return StatusCode(500, new { Error = "Une erreur interne est survenue.", Detail = ex.Message });
            }
        }

        // --------------------------------------------------
        // --- GET ATTENDANCE BY WEEK ---
        // --------------------------------------------------
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
                var endDate   = end.Date;   // inclusif

                var presences = await _db.Presences
                    .Where(p => p.Date >= startDate && p.Date <= endDate) // <= inclusif
                    .Select(p => new ApiAttendanceDto 
                    {
                        StagiaireId = p.StagiaireId,
                        Date        = p.Date,
                        Statut      = p.Statut
                    })
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation(
                    "Récupération de {Count} entrées entre {Start} et {End}.", 
                    presences.Count, startDate.ToShortDateString(), endDate.ToShortDateString());

                return Ok(presences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur interne lors de la récupération des présences.");
                return StatusCode(500, "Erreur interne du serveur.");
            }
        }

        // --------------------------------------------------
        // --- ATTESTATION MENSUELLE ---
        // --------------------------------------------------
        [HttpGet("attestation-mensuelle/{stagiaireId}")]
        public async Task<IActionResult> GenerateAttestationMensuelle(
            string stagiaireId,
            [FromQuery] int year, 
            [FromQuery] int month) 
        {
            if (string.IsNullOrEmpty(stagiaireId) || year == 0 || month < 1 || month > 12)
                return BadRequest("Paramètres requis : stagiaireId, year, month.");

            if (!TryGetDateRangeForMonth(year, month, out DateTime startDate, out DateTime exclusiveEndDate))
                return StatusCode(500, "Erreur calcul plage de dates.");
            
            var stagiaire = await _db.Users
                .Where(u => u.Id == stagiaireId)
                .ToListAsync();
                
            if (stagiaire == null || !stagiaire.Any())
                return NotFound("Stagiaire non trouvé.");

            var presences = await _db.Presences
                .Where(p => p.StagiaireId == stagiaireId 
                            && p.Date >= startDate.Date 
                            && p.Date <= exclusiveEndDate.AddDays(-1).Date) // inclusif
                .Select(p => new ApiAttendanceDto 
                {
                    StagiaireId = p.StagiaireId,
                    Date        = p.Date,
                    Statut      = p.Statut
                })
                .ToListAsync();

            var logoPath  = Path.Combine(_env.WebRootPath, "Upload/images/logo.png");
            byte[] logoBytes = Array.Empty<byte>();
            if (System.IO.File.Exists(logoPath))
            {
                try { logoBytes = System.IO.File.ReadAllBytes(logoPath); }
                catch (Exception ex) { _logger.LogError(ex, "Erreur lecture logo."); }
            }

            var doc = new AttestationPresenceDocument(
                startDate,
                exclusiveEndDate.AddDays(-1),
                presences,
                stagiaire
            );

            var pdfBytes = doc.GeneratePdf();
            var filename = $"Attestation_Presence_{stagiaire[0].UserName}_{year}_{month:D2}.pdf";

            return File(pdfBytes, "application/pdf", filename);
        }

        private bool TryGetDateRangeForMonth(int year, int month, out DateTime startDate, out DateTime exclusiveEndDate)
        {
            try
            {
                startDate        = new DateTime(year, month, 1).Date;
                exclusiveEndDate = startDate.AddMonths(1).Date; 
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                startDate = exclusiveEndDate = default;
                return false;
            }
        }
    }
}