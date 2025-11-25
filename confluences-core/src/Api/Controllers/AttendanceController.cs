// File: Api/Controllers/AttendanceController.cs

using Api.Models.Dto.Attendance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Mime;

// Assurez-vous d'avoir les 'using' nécessaires pour la génération de PDF
using QuestPDF.Fluent; 
using Api.Documents; // Assurez-vous que c'est le bon namespace pour AttestationPresenceDocument

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        // Dans un vrai projet, vous injecteriez ILogger et un service de données (IStageService ou IAttendanceService)
        // private readonly IAttendanceService _attendanceService;

        public AttendanceController(/* IAttendanceService attendanceService */)
        {
            // _attendanceService = attendanceService;
        }

        // POST /api/Attendance/save
        /// <summary>
        /// Enregistre les données de présence envoyées par le front-end.
        /// </summary>
        [HttpPost("save")]
        public async Task<IActionResult> SaveAttendance([FromBody] List<AttendanceEntryDto> entries)
        {
            if (entries == null || entries.Count == 0)
            {
                return BadRequest("La liste des entrées de présence ne peut pas être vide.");
            }

            // --- Logique métier à implémenter ---
            
            // 1. Validation : Vérifiez si toutes les données sont cohérentes (dates valides, stagiaires existants, etc.)
            // 2. Mapping : Mapper les DTO (AttendanceEntryDto) vers les entités de domaine (Presence).
            // 3. Persistance : Appelez le service pour sauvegarder/mettre à jour les données dans la base de données.
            //    await _attendanceService.SaveEntriesAsync(entries); 

            // Simplement pour confirmer la réception des données :
            Console.WriteLine($"Reçu {entries.Count} entrées de présence.");

            return Ok(new { Message = "Présences enregistrées avec succès." });
        }

        // GET /api/Attendance/pdf?startDate=...&endDate=...
        /// <summary>
        /// Génère un document PDF d'attestation de présence pour une période donnée.
        /// </summary>
        /// <param name="startDate">Date de début de la période.</param>
        /// <param name="endDate">Date de fin de la période.</param>
        [HttpGet("pdf")]
        public async Task<IActionResult> GenerateAttendancePdf(
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            if (startDate >= endDate)
            {
                return BadRequest("La date de début doit précéder la date de fin.");
            }
            
            // --- Logique métier à implémenter ---
            
            // 1. Récupération des données :
            //    List<Presence> presences = await _attendanceService.GetPresencesByPeriod(startDate, endDate);
            //    List<ApplicationUser> stagiaires = await _attendanceService.GetStagiairesWithPresence(startDate, endDate);

            // --- Données de test (à remplacer par la récupération réelle) ---
            var presences = new List<AttendanceEntryDto> 
            {
                // Simulez quelques données réelles pour tester le PDF
                new AttendanceEntryDto { StagiaireId = Guid.NewGuid(), Date = startDate, Statut = "a" },
                // ...
            };
            var stagiaires = new List<ApplicationUser> 
            {
                // Assurez-vous d'avoir l'entité ApplicationUser disponible
                // Ex: new ApplicationUser { Firstname = "Nom", LastName = "Stagiaire" }
            };

            // 2. Génération du document :
            var document = new AttestationPresenceDocument(
                startDate,
                endDate,
                presences,
                stagiaires
                /*, _logoService.GetLogo() */
            );

            // 3. Rendu PDF
            var pdfBytes = document.GeneratePdf();

            var filename = $"Attestation_Presences_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.pdf";

            return File(pdfBytes, MediaTypeNames.Application.Pdf, filename);
        }
    }
}