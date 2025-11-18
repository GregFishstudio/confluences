using Confluences.Domain.Entities;
using Confluences.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Api.Controllers
{
    [Authorize(Policy = "Teacher")]
    [Route("api/[controller]")]
    [ApiController]
    public class TypeJobSearchAssistancesController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public TypeJobSearchAssistancesController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/TypeJobSearchAssistances
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TypeJobSearchAssistance>>> GetTypeJobSearchAssistances()
        {
            return await _context.TypeJobSearchAssistances.AsNoTracking().OrderBy(s => s.Description).ToListAsync();
        }

        // GET: api/TypeJobSearchAssistances/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TypeJobSearchAssistance>> GetTypeJobSearchAssistance(int id)
        {
            var TypeJobSearchAssistance = await _context.TypeJobSearchAssistances
                .Include(x => x.JobSearchAssistances)
                .Where(t => t.TypeJobSearchAssistanceId == id)
                .SingleOrDefaultAsync();

            if (TypeJobSearchAssistance == null)
            {
                return NotFound();
            }

            return TypeJobSearchAssistance;
        }

        // PUT: api/TypeJobSearchAssistances/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTypeJobSearchAssistance(int id, TypeJobSearchAssistance TypeJobSearchAssistance)
        {
            if (id != TypeJobSearchAssistance.TypeJobSearchAssistanceId)
            {
                return BadRequest();
            }

            _context.Entry(TypeJobSearchAssistance).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TypeJobSearchAssistanceExists(id))
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

        // POST: api/TypeJobSearchAssistances
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<TypeJobSearchAssistance>> PostTypeJobSearchAssistance(TypeJobSearchAssistance TypeJobSearchAssistance)
        {
            _context.TypeJobSearchAssistances.Add(TypeJobSearchAssistance);

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTypeJobSearchAssistance", new { id = TypeJobSearchAssistance.TypeJobSearchAssistanceId }, TypeJobSearchAssistance);
        }

        // DELETE: api/TypeJobSearchAssistances/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<TypeJobSearchAssistance>> DeleteTypeJobSearchAssistance(int id)
        {
            var TypeJobSearchAssistance = await _context.TypeJobSearchAssistances.FindAsync(id);
            if (TypeJobSearchAssistance == null)
            {
                return NotFound();
            }

            _context.TypeJobSearchAssistances.Remove(TypeJobSearchAssistance);
            await _context.SaveChangesAsync();

            return TypeJobSearchAssistance;
        }

        private bool TypeJobSearchAssistanceExists(int id)
        {
            return _context.TypeJobSearchAssistances.Any(e => e.TypeJobSearchAssistanceId == id);
        }
    }
}
