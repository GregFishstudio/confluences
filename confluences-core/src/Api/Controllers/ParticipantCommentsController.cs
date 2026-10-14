using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Confluences.Domain.Entities;
using Confluences.Persistence;

namespace Api.Controllers
{
    // Commentaires datés et signés des rubriques du "Suivi formatrices" (fiche participant·e).
    [Authorize(Policy = "Teacher")]
    [Route("api/[controller]")]
    [ApiController]
    public class ParticipantCommentsController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public ParticipantCommentsController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/ParticipantComments?studentId=abc — toutes rubriques, le plus récent en premier.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ParticipantComment>>> GetComments([FromQuery] string studentId)
        {
            return await _context.ParticipantComments.AsNoTracking()
                .Where(c => c.StudentId == studentId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        // POST: api/ParticipantComments  { studentId, rubrique, text }
        [HttpPost]
        public async Task<ActionResult<ParticipantComment>> PostComment(ParticipantCommentDto dto)
        {
            var text = (dto.Text ?? "").Trim();
            if (string.IsNullOrEmpty(text)) return BadRequest("Le commentaire est vide.");
            if (string.IsNullOrWhiteSpace(dto.Rubrique)) return BadRequest("Rubrique manquante.");
            if (!await _context.Users.AnyAsync(u => u.Id == dto.StudentId)) return NotFound();

            var authorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var author = authorId != null ? await _context.Users.FindAsync(authorId) : null;

            var comment = new ParticipantComment
            {
                StudentId = dto.StudentId,
                Rubrique = dto.Rubrique.Trim(),
                Text = text,
                CreatedAt = DateTime.UtcNow,
                AuthorId = authorId,
                AuthorName = author != null ? $"{author.Firstname} {author.LastName}".Trim() : null,
            };
            _context.ParticipantComments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        // DELETE: api/ParticipantComments/5 — suppression d'une entrée erronée.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.ParticipantComments.FindAsync(id);
            if (comment == null) return NotFound();

            _context.ParticipantComments.Remove(comment);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class ParticipantCommentDto
    {
        public string StudentId { get; set; } = string.Empty;
        public string Rubrique { get; set; } = string.Empty;
        public string? Text { get; set; }
    }
}
