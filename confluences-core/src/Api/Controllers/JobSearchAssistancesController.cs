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
    public class JobSearchAssistancesController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public JobSearchAssistancesController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/JobSearchAssistances
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobSearchAssistance>>> GetJobSearchAssistances()
        {
            return await _context.JobSearchAssistances
                .Include(x => x.TypeJobSearchAssistance)
                .Include(x => x.TypeJobSearchAssistanceOccurrence)
                .AsNoTracking().OrderBy(s => s.Description).ToListAsync();
        }

        // GET: api/JobSearchAssistances/5
        [HttpGet("{id}")]
        public async Task<ActionResult<JobSearchAssistance>> GetJobSearchAssistance(int id)
        {
            var JobSearchAssistance = await _context.JobSearchAssistances
                .Where(t => t.JobSearchAssistanceId == id)
                .SingleOrDefaultAsync();

            if (JobSearchAssistance == null)
            {
                return NotFound();
            }

            return JobSearchAssistance;
        }

        // PUT: api/JobSearchAssistances/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutJobSearchAssistance(int id, JobSearchAssistance JobSearchAssistance)
        {
            if (id != JobSearchAssistance.JobSearchAssistanceId)
            {
                return BadRequest();
            }

            _context.Entry(JobSearchAssistance).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JobSearchAssistanceExists(id))
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

        // POST: api/JobSearchAssistances
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<JobSearchAssistance>> PostJobSearchAssistance(JobSearchAssistance JobSearchAssistance)
        {
            _context.JobSearchAssistances.Add(JobSearchAssistance);

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetJobSearchAssistance", new { id = JobSearchAssistance.JobSearchAssistanceId }, JobSearchAssistance);
        }

        // DELETE: api/JobSearchAssistances/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<JobSearchAssistance>> DeleteJobSearchAssistance(int id)
        {
            var JobSearchAssistance = await _context.JobSearchAssistances.FindAsync(id);
            if (JobSearchAssistance == null)
            {
                return NotFound();
            }

            _context.JobSearchAssistances.Remove(JobSearchAssistance);
            await _context.SaveChangesAsync();

            return JobSearchAssistance;
        }

        private bool JobSearchAssistanceExists(int id)
        {
            return _context.JobSearchAssistances.Any(e => e.JobSearchAssistanceId == id);
        }
    }
}
