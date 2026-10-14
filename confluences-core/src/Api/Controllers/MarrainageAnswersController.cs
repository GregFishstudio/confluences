using System;
using System.Collections.Generic;
using System.Security.Claims;
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
    public class MarrainageAnswersController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public MarrainageAnswersController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/MarrainageAnswers?marrainageId=5
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MarrainageAnswer>>> GetAnswers([FromQuery] int marrainageId)
        {
            return await _context.MarrainageAnswers
                .AsNoTracking()
                .Include(a => a.Question)
                .Where(a => a.MarrainageId == marrainageId)
                .ToListAsync();
        }

        // POST: api/MarrainageAnswers/bulk
        // Remplace toutes les réponses existantes du marrainage par la liste fournie.
        [HttpPost("bulk")]
        public async Task<ActionResult<IEnumerable<MarrainageAnswer>>> SaveAnswers(BulkAnswersDto dto)
        {
            var existing = _context.MarrainageAnswers.Where(a => a.MarrainageId == dto.MarrainageId);
            _context.MarrainageAnswers.RemoveRange(existing);

            if (!await _context.Marrainages.AnyAsync(m => m.MarrainageId == dto.MarrainageId)) return NotFound();

            // Une réponse à une question supprimée entre-temps faisait échouer tout l'enregistrement
            // (clé étrangère) : on l'ignore.
            var questionIds = (await _context.MarrainageQuestions.Select(q => q.MarrainageQuestionId).ToListAsync()).ToHashSet();

            var answers = dto.Answers
                .Where(a => !string.IsNullOrWhiteSpace(a.Value) && questionIds.Contains(a.QuestionId))
                .Select(a => new MarrainageAnswer
                {
                    MarrainageId = dto.MarrainageId,
                    MarrainageQuestionId = a.QuestionId,
                    Value = a.Value
                })
                .ToList();

            _context.MarrainageAnswers.AddRange(answers);

            // Trace qui a rempli le questionnaire, et quand (affiché à côté des réponses).
            var marrainage = await _context.Marrainages.FindAsync(dto.MarrainageId);
            if (marrainage != null)
            {
                var authorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var author = authorId != null ? await _context.Users.FindAsync(authorId) : null;
                marrainage.QuestionnaireSaisiPar = author != null ? $"{author.Firstname} {author.LastName}".Trim() : null;
                marrainage.QuestionnaireSaisiLe = DateTime.UtcNow;
                marrainage.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return answers;
        }
    }

    public class BulkAnswersDto
    {
        public int MarrainageId { get; set; }
        public List<AnswerDto> Answers { get; set; } = new();
    }

    public class AnswerDto
    {
        public int QuestionId { get; set; }
        public string? Value { get; set; }
    }
}
