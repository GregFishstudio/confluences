using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Confluences.Persistence;
using Confluences.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
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

        public AccompanyingController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // DTOs
        public class SyncRequestDto {
            public string StagiaireId { get; set; }
            public string ActivityKey { get; set; }
            public List<DateTime> Dates { get; set; }
        }

        // GET: api/accompanying/matrix
        [HttpGet("matrix")]
        public async Task<IActionResult> GetMatrix([FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            var data = await _context.AccompanyingPresences
                .Where(ap => ap.Date >= start && ap.Date <= end)
                .Select(ap => new { ap.StagiaireId, ap.ActivityName, ap.Date })
                .ToListAsync();

            var result = data
                .GroupBy(x => new { x.StagiaireId, x.ActivityName })
                .Select(g => new 
                {
                    StagiaireId = g.Key.StagiaireId,
                    ActivityKey = g.Key.ActivityName,
                    Dates = g.Select(x => x.Date.ToString("yyyy-MM-dd")).ToList()
                });

            return Ok(result);
        }

        // POST: api/accompanying/sync-dates
        [HttpPost("sync-dates")]
        public async Task<IActionResult> SyncDates([FromBody] SyncRequestDto request)
        {
            var existing = await _context.AccompanyingPresences
                .Where(ap => ap.StagiaireId == request.StagiaireId && ap.ActivityName == request.ActivityKey)
                .ToListAsync();

            var newDatesSet = request.Dates.Select(d => d.Date).ToHashSet(); // Ignore time

            // Supprimer les dates décochées
            var toDelete = existing.Where(e => !newDatesSet.Contains(e.Date)).ToList();
            if(toDelete.Any()) _context.AccompanyingPresences.RemoveRange(toDelete);

            // Ajouter les nouvelles dates
            var existingDatesSet = existing.Select(e => e.Date).ToHashSet();
            var toAdd = request.Dates
                .Where(d => !existingDatesSet.Contains(d.Date))
                .Select(d => new AccompanyingPresence {
                    StagiaireId = request.StagiaireId,
                    ActivityName = request.ActivityKey,
                    Date = d,
                    IsPresent = true,
                    Notes = ""
                }).ToList();

            if(toAdd.Any()) await _context.AccompanyingPresences.AddRangeAsync(toAdd);

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}