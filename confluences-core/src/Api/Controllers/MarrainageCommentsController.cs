using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
    public class MarrainageCommentsController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public MarrainageCommentsController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/MarrainageComments?marrainageId=5
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MarrainageComment>>> GetMarrainageComments([FromQuery] int marrainageId)
        {
            return await _context.MarrainageComments
                .AsNoTracking()
                .Include(c => c.Author)
                .Where(c => c.MarrainageId == marrainageId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        // POST: api/MarrainageComments
        [HttpPost]
        public async Task<ActionResult<MarrainageComment>> PostMarrainageComment(MarrainageCommentDto dto)
        {
            var authorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var comment = new MarrainageComment
            {
                MarrainageId = dto.MarrainageId,
                Text = dto.Text,
                // null = commentaires généraux ; "parrain" = ne concerne que le parrain / la marraine.
                Portee = dto.Portee == "parrain" ? "parrain" : null,
                AuthorId = authorId,
                CreatedAt = System.DateTime.UtcNow
            };

            _context.MarrainageComments.Add(comment);
            await _context.SaveChangesAsync();

            await _context.Entry(comment).Reference(c => c.Author).LoadAsync();

            return CreatedAtAction(nameof(GetMarrainageComments), new { marrainageId = comment.MarrainageId }, comment);
        }

        // DELETE: api/MarrainageComments/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<MarrainageComment>> DeleteMarrainageComment(int id)
        {
            var comment = await _context.MarrainageComments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (comment.AuthorId != currentUserId)
            {
                return Forbid();
            }

            _context.MarrainageComments.Remove(comment);
            await _context.SaveChangesAsync();

            return comment;
        }
    }

    public class MarrainageCommentDto
    {
        public int MarrainageId { get; set; }
        public string Text { get; set; } = string.Empty;
        public string? Portee { get; set; }
    }
}
