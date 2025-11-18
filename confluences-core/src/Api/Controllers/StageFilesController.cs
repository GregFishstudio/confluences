using Confluences.Domain.Entities;
using Confluences.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Authorize(Policy = "Teacher")]
    [Route("api/[controller]")]
    [ApiController]
    public class StageFilesController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public StageFilesController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/StageFiles/5
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetStageFile(int id)
        {
            var stageFile = await _context.StageFiles.FindAsync(id);

            if (stageFile == null)
            {
                return NotFound();
            }

            return File(stageFile.Data, stageFile.FileType, stageFile.FileName);
        }

        // POST: api/StageFiles
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<StageFile>> PostStageFile(IFormFile file, [FromForm]int stageId)
        {
            var stageFile = new StageFile()
            {
                FileName = file.FileName,
                FileType = file.ContentType,
                StageId = stageId
            };
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                stageFile.Data = ms.ToArray();
            }

            _context.StageFiles.Add(stageFile);

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStageFile", new { id = stageFile.StageId }, stageFile);
        }

        // DELETE: api/StageFiles/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<StageFile>> DeleteStageFile(int id)
        {
            var stageFile = await _context.StageFiles.FindAsync(id);
            if (stageFile == null)
            {
                return NotFound();
            }

            _context.StageFiles.Remove(stageFile);
            await _context.SaveChangesAsync();

            return stageFile;
        }
    }
}
