using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Documents;
using Confluences.Persistence;
using System;
using System.Linq;

namespace Api.Controllers
{
    [Route("api/documents")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly ConfluencesDbContext _db;

        public DocumentsController(ConfluencesDbContext db)
        {
            _db = db;
        }

        [HttpGet("attestation/{id}")]
        public IActionResult Attestation(int id)
        {
            var stage = _db.Stages
                           .Include(s => s.Stagiaire)
                           .Where(s => s.StageId == id)
                           .FirstOrDefault();

            if (stage == null)
                return NotFound();

            var doc = new AttestationCoursDocument(stage);
            var pdf = doc.Generate();

            return File(pdf, "application/pdf", $"attestation-{id}.pdf");
        }

        [HttpGet("bilan/{id}")]
        public IActionResult Bilan(int id, [FromQuery] DateTime date)
        {
            var stage = _db.Stages
                           .Include(s => s.Stagiaire)
                           .Where(s => s.StageId == id)
                           .FirstOrDefault();

            if (stage == null)
                return NotFound();

            var start = date.AddMonths(-3);
            var end = date;

            var doc = new BilanTrimestrielDocument(stage, start, end);
            var pdf = doc.Generate();

            return File(pdf, "application/pdf", $"bilan-{id}.pdf");
        }
    }
}
