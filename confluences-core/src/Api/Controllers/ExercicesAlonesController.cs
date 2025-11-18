using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Confluences.Persistence;
using Microsoft.AspNetCore.Authorization;
using Confluences.Domain.Entities;

namespace Api.Controllers
{
    [Authorize(Policy = "Teacher")]
    [Route("api/[controller]")]
    [ApiController]
    public class ExercicesAlonesController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public ExercicesAlonesController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/ExercicesAlones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExerciceAlone>>> GetExercicesAlones()
        {
            return await _context.ExercicesAlone.ToListAsync();
        }

        [AllowAnonymous]
        // GET: api/ExercicesAlones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ExerciceAlone>> GetExercicesAlone(int id)
        {
            var exercicesAlone = await _context.ExercicesAlone
                                                .Include(e => e.HomeworkV2StudentExerciceAlones)
                                                    .ThenInclude(e => e.Student)
                                                .Where(e => e.ExerciceId == id)
                                                .SingleOrDefaultAsync();

            if (exercicesAlone == null)
            {
                return NotFound();
            }

            return exercicesAlone;
        }

        // PUT: api/ExercicesAlones/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutExercicesAlone(int id, ExerciceAlone exercicesAlone)
        {
            if (id != exercicesAlone.ExerciceId)
            {
                return BadRequest();
            }

            _context.Entry(exercicesAlone).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExercicesAloneExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ExercicesAlones
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<ExerciceAlone>> PostExercicesAlone(ExerciceAlone exercicesAlone)
        {
            _context.ExercicesAlone.Add(exercicesAlone);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetExercicesAlone", new { id = exercicesAlone.ExerciceId }, exercicesAlone);
        }

        // DELETE: api/ExercicesAlones/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ExerciceAlone>> DeleteExercicesAlone(int id)
        {
            var exercicesAlone = await _context.ExercicesAlone.FindAsync(id);
            if (exercicesAlone == null)
            {
                return NotFound();
            }

            _context.ExercicesAlone.Remove(exercicesAlone);
            await _context.SaveChangesAsync();

            return exercicesAlone;
        }

        private bool ExercicesAloneExists(int id)
        {
            return _context.ExercicesAlone.Any(e => e.ExerciceId == id);
        }
    }
}
