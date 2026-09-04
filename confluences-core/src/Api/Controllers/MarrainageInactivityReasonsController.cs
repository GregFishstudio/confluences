using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Confluences.Persistence;
using Confluences.Domain.Entities;

namespace Api.Controllers
{
    [Authorize(Policy = "Teacher")]
    [Route("api/[controller]")]
    [ApiController]
    public class MarrainageInactivityReasonsController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public MarrainageInactivityReasonsController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/MarrainageInactivityReasons
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MarrainageInactivityReason>>> GetReasons()
        {
            return await _context.MarrainageInactivityReasons.OrderBy(r => r.Label).ToListAsync();
        }

        // POST: api/MarrainageInactivityReasons
        [HttpPost]
        public async Task<ActionResult<MarrainageInactivityReason>> PostReason(MarrainageInactivityReasonDto dto)
        {
            var label = (dto.Label ?? "").Trim();
            if (string.IsNullOrEmpty(label)) return BadRequest("Le libellé est requis.");

            var existing = await _context.MarrainageInactivityReasons
                .FirstOrDefaultAsync(r => r.Label.ToLower() == label.ToLower());
            if (existing != null) return Ok(existing);

            var reason = new MarrainageInactivityReason { Label = label };
            _context.MarrainageInactivityReasons.Add(reason);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReasons), new { }, reason);
        }

        // DELETE: api/MarrainageInactivityReasons/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReason(int id)
        {
            var reason = await _context.MarrainageInactivityReasons.FindAsync(id);
            if (reason == null) return NotFound();

            _context.MarrainageInactivityReasons.Remove(reason);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class MarrainageInactivityReasonDto
    {
        public string? Label { get; set; }
    }
}
