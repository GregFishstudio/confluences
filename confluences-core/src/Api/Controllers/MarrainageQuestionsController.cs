using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class MarrainageQuestionsController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public MarrainageQuestionsController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/MarrainageQuestions?activeOnly=true
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MarrainageQuestion>>> GetQuestions([FromQuery] bool activeOnly = false)
        {
            var query = _context.MarrainageQuestions.AsNoTracking().AsQueryable();
            if (activeOnly) query = query.Where(q => q.Active);

            return await query.OrderBy(q => q.Order).ToListAsync();
        }

        // POST: api/MarrainageQuestions
        [HttpPost]
        public async Task<ActionResult<MarrainageQuestion>> PostQuestion(MarrainageQuestion question)
        {
            if (question.Order == 0)
            {
                var maxOrder = await _context.MarrainageQuestions.Select(q => (int?)q.Order).MaxAsync() ?? 0;
                question.Order = maxOrder + 1;
            }

            _context.MarrainageQuestions.Add(question);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetQuestions), new { }, question);
        }

        // PUT: api/MarrainageQuestions/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuestion(int id, MarrainageQuestion question)
        {
            if (id != question.MarrainageQuestionId) return BadRequest();

            _context.Entry(question).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/MarrainageQuestions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var question = await _context.MarrainageQuestions.FindAsync(id);
            if (question == null) return NotFound();

            _context.MarrainageQuestions.Remove(question);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
