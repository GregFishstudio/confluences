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
    public class MarrainageFilesController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public MarrainageFilesController(ConfluencesDbContext context)
        {
            _context = context;
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
