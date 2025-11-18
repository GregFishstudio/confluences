using Confluences.Domain.Entities;
using Confluences.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Authorize(Policy = "Teacher")]
    [Route("api/[controller]")]
    [ApiController]
    public class TypeJobSearchAssistanceOccurrencesController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public TypeJobSearchAssistanceOccurrencesController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/TypeJobSearchAssistanceOccurrences
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TypeJobSearchAssistanceOccurrence>>> GetTypeJobSearchAssistanceOccurrences()
        {
            return await _context.TypeJobSearchAssistanceOccurrences.AsNoTracking().OrderBy(s => s.Description).ToListAsync();
        }

        // GET: api/TypeJobSearchAssistanceOccurrences/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TypeJobSearchAssistanceOccurrence>> GetTypeJobSearchAssistanceOccurrence(int id)
        {
            var TypeJobSearchAssistanceOccurrence = await _context.TypeJobSearchAssistanceOccurrences
                .Include(x => x.JobSearchAssistances)
                .Where(t => t.TypeJobSearchAssistanceOccurrenceId == id)
                .SingleOrDefaultAsync();

            if (TypeJobSearchAssistanceOccurrence == null)
            {
                return NotFound();
            }

            return TypeJobSearchAssistanceOccurrence;
        }

        // PUT: api/TypeJobSearchAssistanceOccurrences/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTypeJobSearchAssistanceOccurrence(int id, TypeJobSearchAssistanceOccurrence TypeJobSearchAssistanceOccurrence)
        {
            if (id != TypeJobSearchAssistanceOccurrence.TypeJobSearchAssistanceOccurrenceId)
            {
                return BadRequest();
            }

            _context.Entry(TypeJobSearchAssistanceOccurrence).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TypeJobSearchAssistanceOccurrenceExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return NoContent();
        }

        // POST: api/TypeJobSearchAssistanceOccurrences
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<TypeJobSearchAssistanceOccurrence>> PostTypeJobSearchAssistanceOccurrence(TypeJobSearchAssistanceOccurrence TypeJobSearchAssistanceOccurrence)
        {
            _context.TypeJobSearchAssistanceOccurrences.Add(TypeJobSearchAssistanceOccurrence);

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTypeJobSearchAssistanceOccurrence", new { id = TypeJobSearchAssistanceOccurrence.TypeJobSearchAssistanceOccurrenceId }, TypeJobSearchAssistanceOccurrence);
        }

        // DELETE: api/TypeJobSearchAssistanceOccurrences/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<TypeJobSearchAssistanceOccurrence>> DeleteTypeJobSearchAssistanceOccurrence(int id)
        {
            var TypeJobSearchAssistanceOccurrence = await _context.TypeJobSearchAssistanceOccurrences.FindAsync(id);
            if (TypeJobSearchAssistanceOccurrence == null)
            {
                return NotFound();
            }

            _context.TypeJobSearchAssistanceOccurrences.Remove(TypeJobSearchAssistanceOccurrence);
            await _context.SaveChangesAsync();

            return TypeJobSearchAssistanceOccurrence;
        }

        private bool TypeJobSearchAssistanceOccurrenceExists(int id)
        {
            return _context.TypeJobSearchAssistanceOccurrences.Any(e => e.TypeJobSearchAssistanceOccurrenceId == id);
        }

    }
}
