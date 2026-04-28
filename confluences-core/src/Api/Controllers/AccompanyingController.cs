using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Confluences.Persistence;
using Confluences.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AccompanyingController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;
        private readonly ILogger<AccompanyingController> _logger;

        public AccompanyingController(ConfluencesDbContext context, ILogger<AccompanyingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // --- DTOs ---
        public class SyncRequestDto 
        {
            public string UserId { get; set; }
            public string ActivityKey { get; set; }
            public List<string> Dates { get; set; } 
            public string PeriodStart { get; set; }        // ← string au lieu de DateTime
    public string PeriodEnd { get; set; } 
        }

        // --- GESTION DES COLONNES ---

        [HttpGet("columns")]
        public async Task<IActionResult> GetColumns()
        {
            try 
            {
                var columns = await _context.MatrixColumns
                    .OrderBy(c => c.Type ?? "")
                    .ThenBy(c => c.DisplayOrder)
                    .Select(c => new {
                        id = c.Id,
                        key = c.Key ?? "", 
                        label = c.Label ?? "Sans nom",
                        type = c.Type ?? "acco",
                        displayOrder = c.DisplayOrder
                    })
                    .ToListAsync();

                return Ok(columns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des colonnes");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("columns/{id}")]
        public async Task<IActionResult> DeleteColumn(int id)
        {
            var col = await _context.MatrixColumns.FindAsync(id);
            if (col == null) return NotFound($"Colonne {id} introuvable.");
            _context.MatrixColumns.Remove(col);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("columns")]
        public async Task<IActionResult> SaveColumns([FromBody] List<MatrixColumn> columns)
        {
            if (columns == null) return BadRequest("Liste de colonnes vide.");

            try 
            {
                foreach (var col in columns)
                {
                    var existing = await _context.MatrixColumns
                        .FirstOrDefaultAsync(c => c.Key == col.Key);

                    if (existing != null)
                    {
                        existing.Label = col.Label;
                        existing.DisplayOrder = col.DisplayOrder;
                        existing.Type = col.Type;
                    }
                    else 
                    {
                        col.Id = 0; 
                        _context.MatrixColumns.Add(col);
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Structure mise à jour" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la sauvegarde des colonnes");
                return StatusCode(500, ex.Message);
            }
        }

        // --- GESTION DES DONNÉES DE PRÉSENCE ---

        [HttpGet("matrix")]
        public async Task<IActionResult> GetMatrix([FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            try
            {
                var data = await _context.AccompanyingPresences
                    .Where(ap => ap.Date >= start && ap.Date <= end)
                    .Select(ap => new { ap.UserId, ap.ActivityName, ap.Date })
                    .ToListAsync();

                var result = data
                    .GroupBy(x => new { x.UserId, x.ActivityName })
                    .Select(g => new 
                    {
                        UserId      = g.Key.UserId,
                        ActivityKey = g.Key.ActivityName,
                        Dates       = g.Select(x => x.Date.ToString("yyyy-MM-dd")).ToList()
                    });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la matrice");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("sync-dates")]
        public async Task<IActionResult> SyncDates([FromBody] SyncRequestDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.UserId))
                return BadRequest("Données invalides : UserId manquant.");

            try
            {
               var periodStart = DateTime.Parse(request.PeriodStart).Date;
var periodEnd   = DateTime.Parse(request.PeriodEnd).Date;

var existing = await _context.AccompanyingPresences
    .Where(ap =>
        ap.UserId == request.UserId &&
        ap.ActivityName == request.ActivityKey &&
        ap.Date >= periodStart &&
        ap.Date <= periodEnd)
    .ToListAsync();

var newDatesSet = request.Dates.Select(d => DateTime.Parse(d).Date).ToHashSet();

var toDelete = existing.Where(e => !newDatesSet.Contains(e.Date.Date)).ToList();
if (toDelete.Any()) _context.AccompanyingPresences.RemoveRange(toDelete);

var existingDatesSet = existing.Select(e => e.Date.Date).ToHashSet();
var toAdd = request.Dates
    .Select(d => DateTime.Parse(d).Date)
    .Where(d => !existingDatesSet.Contains(d))
    .Distinct()
    .Select(d => new AccompanyingPresence
    {
        UserId       = request.UserId,
        ActivityName = request.ActivityKey,
        Date         = d,
        IsPresent    = true,
        Notes        = ""
    }).ToList();

if (toAdd.Any()) await _context.AccompanyingPresences.AddRangeAsync(toAdd);

await _context.SaveChangesAsync();
return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la synchronisation des dates");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}