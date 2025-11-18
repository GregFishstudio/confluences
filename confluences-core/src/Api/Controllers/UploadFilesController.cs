using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Confluences.Domain.Entities;
using Confluences.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadFilesController : ControllerBase
    {
        public static IWebHostEnvironment _environnement;
        private readonly ConfluencesDbContext _context;
        private readonly ILogger<UploadFilesController> _logger;

        public UploadFilesController(IWebHostEnvironment environnement, ConfluencesDbContext context, ILogger<UploadFilesController> logger)
        {
            _environnement = environnement;
            _context = context;
            _logger = logger;
        }

        [Authorize(Policy = "teacher")]
        [Route("SaveTheory")]
        [HttpPost]
        public ActionResult<string> PostSaveTheory(List<IFormFile> files)
        {
            try
            {
                string path = "";
                string folder = "Cours";
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        if (!Directory.Exists(Path.Combine(_environnement.WebRootPath, folder)))
                        {
                            Directory.CreateDirectory(Path.Combine(_environnement.WebRootPath, folder));
                        }

                        string filename = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss_") + Path.GetRandomFileName() + file.FileName;
                        using (FileStream fileStream = System.IO.File.Create(Path.Combine(_environnement.WebRootPath, folder, filename)))
                        {
                            file.CopyTo(fileStream);
                            fileStream.Flush();
                        }
                        path = folder + "/" + filename;
                    }
                }
                return Content(path);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Upload file in SaveTheory failed : {error}", e.Message);
                return BadRequest();
            }
        }

        [Authorize(Policy = "teacher")]
        [Route("SaveTheoryVideo")]
        [HttpPost]
        public ActionResult<string> PostSaveTheoryVideo(List<IFormFile> files)
        {
            try
            {
                string path = "";
                string folder = "Cours";
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        if (!Directory.Exists(Path.Combine(_environnement.WebRootPath, folder)))
                        {
                            Directory.CreateDirectory(Path.Combine(_environnement.WebRootPath, folder));
                        }

                        string filename = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss_") + Path.GetRandomFileName() + file.FileName;
                        using (FileStream fileStream = System.IO.File.Create(Path.Combine(_environnement.WebRootPath, folder, filename)))
                        {
                            file.CopyTo(fileStream);
                            fileStream.Flush();
                        }
                        path = folder + "/" + filename;
                    }
                }
                return Content(path);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [Authorize(Policy = "teacher")]
        [Route("SaveExercice")]
        [HttpPost]
        public ActionResult<string> PostSaveExercice(List<IFormFile> files)
        {
            try
            {
                string path = "";
                string folder = "Exercice";
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        if (!Directory.Exists(Path.Combine(_environnement.WebRootPath, folder)))
                        {
                            Directory.CreateDirectory(Path.Combine(_environnement.WebRootPath, folder));
                        }

                        string filename = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss_") + Path.GetRandomFileName() + file.FileName;
                        using (FileStream fileStream = System.IO.File.Create(Path.Combine(_environnement.WebRootPath, folder, filename)))
                        {
                            file.CopyTo(fileStream);
                            fileStream.Flush();
                        }
                        path = folder + "/" + filename;
                    }
                }
                return Content(path);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Upload file in SaveExercice failed : {error}", e.Message);
                return BadRequest();
            }
        }

        [Authorize]
        [Route("SaveHomeworkStudent")]
        [HttpPost]
        public async Task<ActionResult<string>> PostSaveHomeworkStudentAsync(List<IFormFile> files, int id)
        {
            try
            {
                string path = "";
                string folder = "Devoirs_Participants";
                string userId = User.Claims.Where(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").SingleOrDefault().Value;
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        if (!Directory.Exists(Path.Combine(_environnement.WebRootPath, folder)))
                        {
                            Directory.CreateDirectory(Path.Combine(_environnement.WebRootPath, folder));
                        }

                        string filename = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss_") + Path.GetRandomFileName() + file.FileName;
                        using (FileStream fileStream = System.IO.File.Create(Path.Combine(_environnement.WebRootPath, folder, filename)))
                        {
                            file.CopyTo(fileStream);
                            fileStream.Flush();
                        }
                        path = folder + "/" + filename;
                        HomeworkV2Student homework = new HomeworkV2Student
                        {
                            ExerciceId = id,
                            HomeworkFile = path,
                            HomeworkFileName = file.FileName,
                            HomeworkFileSize = file.Length.ToString(),
                            HomeworkFileType = file.ContentType,
                            HomeworkStudentDate = DateTime.Now,
                            StudentId = userId
                        };

                        await _context.HomeworkV2Students.AddAsync(homework);
                        await _context.SaveChangesAsync();
                    }

                }
                return Content(path);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Upload file in SaveHomeworkStudent failed : {error}", e.Message);
                return BadRequest();
            }
        }

        [Authorize(Policy = "teacher")]
        [Route("EditHomeworkStudent")]
        [HttpPost]
        public async Task<ActionResult<string>> PostEditHomeworkStudentAsync(List<IFormFile> files, int id)
        {
            try
            {
                string path = "";
                string folder = "Devoirs_Participants";
                string userId = User.Claims.Where(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").SingleOrDefault().Value;
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        if (!Directory.Exists(Path.Combine(_environnement.WebRootPath, folder)))
                        {
                            Directory.CreateDirectory(Path.Combine(_environnement.WebRootPath, folder));
                        }

                        string filename = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss_") + Path.GetRandomFileName() + file.FileName;
                        using (FileStream fileStream = System.IO.File.Create(Path.Combine(_environnement.WebRootPath, folder, filename)))
                        {
                            file.CopyTo(fileStream);
                            fileStream.Flush();
                        }
                        path = folder + "/" + filename;

                        var homework = await _context.HomeworkV2Students.FindAsync(id);

                        homework.HomeworkFile = path;

                        _context.HomeworkV2Students.Update(homework);

                        await _context.SaveChangesAsync();
                    }

                }
                return Content(path);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Upload file in EditHomeworkStudent failed : {error}", e.Message);
                return BadRequest();
            }
        }

        [Authorize]
        [Route("EraseHomeworkStudent")]
        [HttpDelete]
        public async Task<ActionResult<HomeworkV2Student>> PostEraseHomeworkStudentAsync(int id)
        {
            var homework = await _context.HomeworkV2Students.FindAsync(id);
            if (homework == null)
            {
                return NotFound();
            }

            _context.HomeworkV2Students.Remove(homework);
            await _context.SaveChangesAsync();

            return homework;

        }

        [Authorize]
        [Route("SaveHomeworkStudentAlone")]
        [HttpPost]
        public async Task<ActionResult<string>> PostSaveHomeworkStudentAloneAsync(List<IFormFile> files, int id)
        {
            try
            {
                string path = "";
                string folder = "Devoirs_Participants";
                string userId = User.Claims.Where(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").SingleOrDefault().Value;
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        if (!Directory.Exists(Path.Combine(_environnement.WebRootPath, folder)))
                        {
                            Directory.CreateDirectory(Path.Combine(_environnement.WebRootPath, folder));
                        }

                        string filename = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss_") + Path.GetRandomFileName() + file.FileName;
                        using (FileStream fileStream = System.IO.File.Create(Path.Combine(_environnement.WebRootPath, folder, filename)))
                        {
                            file.CopyTo(fileStream);
                            fileStream.Flush();
                        }
                        path = folder + "/" + filename;
                        HomeworkV2StudentExerciceAlone homework = new HomeworkV2StudentExerciceAlone
                        {
                            ExerciceId = id,
                            HomeworkFile = path,
                            HomeworkFileName = file.FileName,
                            HomeworkFileSize = file.Length.ToString(),
                            HomeworkFileType = file.ContentType,
                            HomeworkStudentDate = DateTime.Now,
                            StudentId = userId
                        };

                        await _context.HomeworkV2StudentExerciceAlones.AddAsync(homework);
                        await _context.SaveChangesAsync();
                    }

                }
                return Content(path);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Upload file in SaveHomeworkStudentAlone failed : {error}", e.Message);
                return BadRequest();
            }
        }

        [Authorize(Policy = "teacher")]
        [Route("EditHomeworkStudentAlone")]
        [HttpPost]
        public async Task<ActionResult<string>> PostEditHomeworkStudentAloneAsync(List<IFormFile> files, int id)
        {
            try
            {
                string path = "";
                string folder = "Devoirs_Participants";
                string userId = User.Claims.Where(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").SingleOrDefault().Value;
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        if (!Directory.Exists(Path.Combine(_environnement.WebRootPath, folder)))
                        {
                            Directory.CreateDirectory(Path.Combine(_environnement.WebRootPath, folder));
                        }

                        string filename = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss_") + Path.GetRandomFileName() + file.FileName;
                        using (FileStream fileStream = System.IO.File.Create(Path.Combine(_environnement.WebRootPath, folder, filename)))
                        {
                            file.CopyTo(fileStream);
                            fileStream.Flush();
                        }
                        path = folder + "/" + filename;

                        var homework = await _context.HomeworkV2StudentExerciceAlones.FindAsync(id);

                        homework.HomeworkFile = path;

                        _context.HomeworkV2StudentExerciceAlones.Update(homework);

                        await _context.SaveChangesAsync();
                    }

                }
                return Content(path);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Upload file in EditHomeworkStudentAlone failed : {error}", e.Message);
                return BadRequest();
            }
        }

        [Authorize]
        [Route("EraseHomeworkStudentAlone")]
        [HttpDelete]
        public async Task<ActionResult<HomeworkV2StudentExerciceAlone>> PostEraseHomeworkStudentAloneAsync(int id)
        {
            var homework = await _context.HomeworkV2StudentExerciceAlones.FindAsync(id);
            if (homework == null)
            {
                return NotFound();
            }

            _context.HomeworkV2StudentExerciceAlones.Remove(homework);
            await _context.SaveChangesAsync();

            return homework;

        }

        //[Authorize(Policy = "teacher")]
        [Route("UploadHomeworkStudentLive")]
        [HttpPost]
        public async Task<ActionResult<string>> UploadHomeworkStudentLiveAsync(IFormFile file, [FromForm] int id)
        {
            var homework = await _context.HomeworkV2Students.FindAsync(id);
            if (homework == null)
            {
                return NotFound();
            }

            string folder = "Devoirs_Participants";

            // The trick here is to bypass the cache by changing erasing old file and giving a new name to the file
            string oldFile = homework.HomeworkFile.Replace(string.Concat(folder, "/"), "");
            System.IO.File.Delete(Path.Combine(_environnement.WebRootPath, folder, oldFile));

            string filename = DateTime.Now.ToString("yyyy-MM-dd_HH-m-ss_") + Path.GetRandomFileName() + file.FileName;

            if (file.Length > 0)
            {
                if (!Directory.Exists(Path.Combine(_environnement.WebRootPath, folder)))
                {
                    Directory.CreateDirectory(Path.Combine(_environnement.WebRootPath, folder));
                }

                using (FileStream fileStream = System.IO.File.Create(Path.Combine(_environnement.WebRootPath, folder, filename)))
                {
                    file.CopyTo(fileStream);
                    fileStream.Flush();
                }
            }

            homework = await _context.HomeworkV2Students.FindAsync(id);
            var path = folder + "/" + filename;
            homework.HomeworkFile = path;

            _context.HomeworkV2Students.Update(homework);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [Route("UploadHomeworkStudentExerciceAloneLive")]
        [HttpPost]
        public async Task<ActionResult<string>> UploadHomeworkStudentExerciceAloneLiveAsync(IFormFile file, [FromForm] int id)
        {
            var homework = await _context.HomeworkV2StudentExerciceAlones.FindAsync(id);
            if (homework == null)
            {
                return NotFound();
            }

            string folder = "Devoirs_Participants";

            // The trick here is to bypass the cache by changing erasing old file and giving a new name to the file
            string oldFile = homework.HomeworkFile.Replace(string.Concat(folder, "/"), "");
            System.IO.File.Delete(Path.Combine(_environnement.WebRootPath, folder, oldFile));

            string filename = DateTime.Now.ToString("yyyy-MM-dd_HH-m-ss_") + Path.GetRandomFileName() + file.FileName;

            if (file.Length > 0)
            {
                if (!Directory.Exists(Path.Combine(_environnement.WebRootPath, folder)))
                {
                    Directory.CreateDirectory(Path.Combine(_environnement.WebRootPath, folder));
                }

                using (FileStream fileStream = System.IO.File.Create(Path.Combine(_environnement.WebRootPath, folder, filename)))
                {
                    file.CopyTo(fileStream);
                    fileStream.Flush();
                }
            }

            homework = await _context.HomeworkV2StudentExerciceAlones.FindAsync(id);
            var path = folder + "/" + filename;
            homework.HomeworkFile = path;

            _context.HomeworkV2StudentExerciceAlones.Update(homework);

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
