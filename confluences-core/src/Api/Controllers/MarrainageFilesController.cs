using Api.Utility;
using Confluences.Domain.Entities;
using Confluences.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Authorize(Policy = "Teacher")]
    [Route("api/[controller]")]
    [ApiController]
    public class MarrainageFilesController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public MarrainageFilesController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/MarrainageFiles?marrainageId=5&category=Convention
        // Métadonnées seulement (sans les octets) — utilisé pour lister les documents d'une
        // marraine ailleurs dans l'appli (ex. fiche participant) sans alourdir la réponse.
        [HttpGet]
        public async Task<ActionResult> GetMarrainageFiles([FromQuery] int? marrainageId, [FromQuery] string? category)
        {
            var query = _context.MarrainageFiles.AsNoTracking().AsQueryable();
            if (marrainageId.HasValue) query = query.Where(f => f.MarrainageId == marrainageId.Value);
            if (!string.IsNullOrWhiteSpace(category)) query = query.Where(f => f.Category == category);

            var files = await query
                .Select(f => new { f.MarrainageFileId, f.FileName, f.Category, f.MarrainageId })
                .ToListAsync();

            return Ok(files);
        }

        // GET: api/MarrainageFiles/5
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetMarrainageFile(int id)
        {
            var marrainageFile = await _context.MarrainageFiles.FindAsync(id);

            if (marrainageFile == null)
            {
                return NotFound();
            }

            return File(marrainageFile.Data, marrainageFile.FileType, marrainageFile.FileName);
        }

        // POST: api/MarrainageFiles
        [HttpPost]
        public async Task<ActionResult<MarrainageFile>> PostMarrainageFile(IFormFile file, [FromForm] int marrainageId, [FromForm] string? category)
        {
            if (!AllowedDocumentTypes.IsAllowed(file))
                return BadRequest($"Format de fichier non accepté. Formats autorisés : {AllowedDocumentTypes.AllowedExtensionsLabel}");

            var marrainageFile = new MarrainageFile()
            {
                FileName = file.FileName,
                FileType = file.ContentType,
                MarrainageId = marrainageId,
                Category = category
            };
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                marrainageFile.Data = ms.ToArray();
            }

            _context.MarrainageFiles.Add(marrainageFile);

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMarrainageFile", new { id = marrainageFile.MarrainageFileId }, marrainageFile);
        }

        // DELETE: api/MarrainageFiles/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<MarrainageFile>> DeleteMarrainageFile(int id)
        {
            var marrainageFile = await _context.MarrainageFiles.FindAsync(id);
            if (marrainageFile == null)
            {
                return NotFound();
            }

            _context.MarrainageFiles.Remove(marrainageFile);
            await _context.SaveChangesAsync();

            return marrainageFile;
        }
    }
}
