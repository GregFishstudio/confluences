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
    public class HomeworkV2studentsController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public HomeworkV2studentsController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/HomeworkV2students
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HomeworkV2Student>>> GetHomeworkV2students()
        {
            return await _context.HomeworkV2Students.ToListAsync();
        }

        // GET: api/HomeworkV2students/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HomeworkV2Student>> GetHomeworkV2students(int id)
        {
            var homeworkV2students = await _context.HomeworkV2Students.FindAsync(id);

            if (homeworkV2students == null)
            {
                return NotFound();
            }

            return homeworkV2students;
        }

        // PUT: api/HomeworkV2students/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHomeworkV2students(int id, HomeworkV2Student homeworkV2students)
        {
            if (id != homeworkV2students.HomeworkV2StudentId)
            {
                return BadRequest();
            }

            _context.Entry(homeworkV2students).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HomeworkV2studentsExists(id))
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

        // POST: api/HomeworkV2students
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<HomeworkV2Student>> PostHomeworkV2students(HomeworkV2Student homeworkV2students)
        {
            _context.HomeworkV2Students.Add(homeworkV2students);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetHomeworkV2students", new { id = homeworkV2students.HomeworkV2StudentId }, homeworkV2students);
        }

        // DELETE: api/HomeworkV2students/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<HomeworkV2Student>> DeleteHomeworkV2students(int id)
        {
            var homeworkV2students = await _context.HomeworkV2Students.FindAsync(id);
            if (homeworkV2students == null)
            {
                return NotFound();
            }

            _context.HomeworkV2Students.Remove(homeworkV2students);
            await _context.SaveChangesAsync();

            return homeworkV2students;
        }

        private bool HomeworkV2studentsExists(int id)
        {
            return _context.HomeworkV2Students.Any(e => e.HomeworkV2StudentId == id);
        }
    }
}
