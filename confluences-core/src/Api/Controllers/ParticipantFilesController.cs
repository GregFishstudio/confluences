using Api.Utility;
using Confluences.Domain.Entities;
using Confluences.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Authorize(Policy = "Teacher")]
    [Route("api/[controller]")]
    [ApiController]
    public class ParticipantFilesController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public ParticipantFilesController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/ParticipantFiles/5
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetParticipantFile(int id)
        {
            var participantFile = await _context.ParticipantFiles.FindAsync(id);

            if (participantFile == null)
            {
                return NotFound();
            }

            return File(participantFile.Data, participantFile.FileType, participantFile.FileName);
        }

        // POST: api/ParticipantFiles
        [HttpPost]
        public async Task<ActionResult<ParticipantFile>> PostParticipantFile(IFormFile file, [FromForm] string studentId, [FromForm] string? category)
        {
            if (!AllowedDocumentTypes.IsAllowed(file))
                return BadRequest($"Format de fichier non accepté. Formats autorisés : {AllowedDocumentTypes.AllowedExtensionsLabel}");

            var participantFile = new ParticipantFile()
            {
                FileName = file.FileName,
                FileType = file.ContentType,
                StudentId = studentId,
                Category = category
            };
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                participantFile.Data = ms.ToArray();
            }

            _context.ParticipantFiles.Add(participantFile);

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetParticipantFile", new { id = participantFile.ParticipantFileId }, participantFile);
        }

        // DELETE: api/ParticipantFiles/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ParticipantFile>> DeleteParticipantFile(int id)
        {
            var participantFile = await _context.ParticipantFiles.FindAsync(id);
            if (participantFile == null)
            {
                return NotFound();
            }

            _context.ParticipantFiles.Remove(participantFile);
            await _context.SaveChangesAsync();

            return participantFile;
        }
    }
}
