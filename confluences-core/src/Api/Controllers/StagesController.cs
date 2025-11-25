/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Contrôleur permettant le CRUD sur la table Stages
 * Fichier : StagesController.cs
 **/

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Confluences.Persistence;
using Microsoft.AspNetCore.Authorization;
using System;
using Confluences.Domain.Entities;
using Syncfusion.XlsIO;
using System.IO;

namespace Api.Controllers
{
    [Authorize(Policy = "Teacher")]
    [Route("api/[controller]")]
    [ApiController]
    public class StagesController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public StagesController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // Modèle du filtre entreprise
        public class Filter
        {
            public string nom { get; set; }
            public int? typeMetierId { get; set; }
            public int? typeIntershipActivityId { get; set; }
            public int? entrepriseId { get; set; }
            public string stagiaireId { get; set; }
            public int? year { get; set; }
            public DateTime? dateDebut { get; set; }
            public DateTime? dateFin { get; set; }
            public int? typeStageId { get; set; }
            public int? typeAnnonceId { get; set; }
            public int? sessionId { get; set; }

        }

        // GET: api/Stages
        [HttpGet]
        public async Task<IEnumerable<Stage>> GetStages([FromQuery] Filter filter)
        {
            return await GetFilteredStagesAsync(filter);
        }

        // GET: api/Stages/export
        [HttpGet("export")]
        public async Task<FileStreamResult> GetExportedInternships([FromQuery] Filter filter)
        {
            var stages = (await GetFilteredStagesAsync(filter)).ToList();

            //New instance of ExcelEngine is created equivalent to launching Microsoft Excel with no workbooks open
            //Instantiate the spreadsheet creation engine
            using (ExcelEngine excelEngine = new ExcelEngine())
            {
                //Instantiate the Excel application object
                IApplication application = excelEngine.Excel;

                //Assigns default application version
                application.DefaultVersion = ExcelVersion.Excel2010;

                //A new workbook is created equivalent to creating a new workbook in Excel
                //Create a workbook with 1 worksheet
                IWorkbook workbook = application.Workbooks.Create(1);

                //Access first worksheet from the workbook
                IWorksheet worksheet = workbook.Worksheets[0];

                //Adding text to a cell
                worksheet.Range[1, 1].Text = "Id";
                worksheet.Range[1, 2].Text = "Type d'activité";
                worksheet.Range[1, 3].Text = "Métier";
                worksheet.Range[1, 4].Text = "Entreprise";
                worksheet.Range[1, 5].Text = "Personne";
                worksheet.Range[1, 6].Text = "Année";
                worksheet.Range[1, 7].Text = "Début";
                worksheet.Range[1, 8].Text = "Fin";
                worksheet.Range[1, 9].Text = "Session";
                worksheet.Range[1, 10].Text = "Type";
                worksheet.Range[1, 11].Text = "Annonce";

                for (int i = 0; i < stages.Count(); i++)
                {
                    worksheet.Range[i + 2, 1].Number = stages[i].StageId;
                    worksheet.Range[i + 2, 2].Text = stages[i].TypeIntershipActivity?.Nom;
                    worksheet.Range[i + 2, 3].Text = stages[i].TypeMetier?.Libelle;
                    worksheet.Range[i + 2, 4].Text = stages[i].Entreprise?.Nom;
                    worksheet.Range[i + 2, 5].Text = string.Concat(stages[i].Stagiaire?.Firstname, " ", stages[i].Stagiaire?.LastName);
                    worksheet.Range[i + 2, 6].Number = stages[i].Debut.Year;
                    worksheet.Range[i + 2, 7].DateTime = stages[i].Debut;
                    if (stages[i].Fin.HasValue)
                    {
                        worksheet.Range[i + 2, 8].DateTime = (DateTime)stages[i].Fin;
                    }
                    worksheet.Range[i + 2, 9].Text = stages[i].Session?.Description;
                    worksheet.Range[i + 2, 10].Text = stages[i].TypeStage?.Nom;
                    worksheet.Range[i + 2, 11].Text = stages[i].TypeAnnonce?.Libelle;
                }

                //Save the workbook to stream
                MemoryStream outputStream = new MemoryStream();
                workbook.SaveAs(outputStream);
                outputStream.Position = 0;

                return File(outputStream, "application/excel", "Output.xlsx");
            }


        }

        // GET: api/Stages/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Stage>> GetStage(int id)
        {
            var stage = await _context.Stages
                .Include(s => s.TypeMetier)
                .Include(s => s.StageFiles)
                .Include(s => s.TypeIntershipActivity)
                .Include(s => s.Entreprise)
                .Include(s => s.Stagiaire)
                .Include(s => s.TypeAnnonce)
                .Include(s => s.TypeStage)
                .Include(s => s.Session)
                    .ThenInclude(x => x.SchoolClassRoom)
                .FirstOrDefaultAsync(s=> s.StageId == id);

            if (stage == null)
            {
                return NotFound();
            }

            return stage;
        }

        // PUT: api/Stages/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStage(int id, Stage stage)
        {
            if (id != stage.StageId)
            {
                return BadRequest();
            }

            _context.Entry(stage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StageExists(id))
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

        // POST: api/Stages
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Stage>> PostStage(Stage stage)
        {
            _context.Stages.Add(stage);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStage", new { id = stage.StageId }, stage);
        }

        // GET: api/Stages/ByStagiaire/{stagiaireId}
        [HttpGet("ByStagiaire/{stagiaireId}")]
public async Task<IActionResult> GetStagesByStagiaire(string stagiaireId)
{
    var stages = await _context.Stages
        .Include(s => s.TypeMetier)
        .Include(s => s.Entreprise)
        .Include(s => s.Stagiaire)
        .Where(s => s.StagiaireId == stagiaireId)
        .OrderByDescending(s => s.Debut)
        .ToListAsync();

    return Ok(stages);
}


        // DELETE: api/Stages/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Stage>> DeleteStage(int id)
        {
            var stage = await _context.Stages.FindAsync(id);
            if (stage == null)
            {
                return NotFound();
            }

            _context.Stages.Remove(stage);
            await _context.SaveChangesAsync();

            return stage;
        }

        private bool StageExists(int id)
        {
            return _context.Stages.Any(e => e.StageId == id);
        }

        private async Task<IEnumerable<Stage>> GetFilteredStagesAsync(Filter filter)
        {
            // Start with all stages and their includes
            var stages = await _context.Stages
                                        .Include(s => s.TypeMetier)
                                        .Include(s => s.TypeIntershipActivity)
                                        .Include(s => s.Entreprise)
                                        .Include(s => s.Stagiaire)
                                        .Include(s => s.TypeAnnonce)
                                        .Include(s => s.TypeStage)
                                        .Include(s => s.Session)
                                            .ThenInclude(x => x.SchoolClassRoom)
                                        .AsNoTracking()
                                        .OrderByDescending(s => s.Debut)
                                        .ToListAsync();

            // Apply filters using ToList() on the materialized collection
            if (filter.nom != null)
            {
                stages = stages.Where(e => e.Nom.ToUpper().Contains(filter.nom.ToUpper())).ToList();
            }

            if (filter.typeIntershipActivityId != null)
            {
                stages = stages.Where(e => e.TypeIntershipActivityId == filter.typeIntershipActivityId).ToList();
            }

            if (filter.typeMetierId != null)
            {
                stages = stages.Where(e => e.TypeMetierId == filter.typeMetierId).ToList();
            }

            if (filter.entrepriseId != null)
            {
                stages = stages.Where(e => e.EntrepriseId == filter.entrepriseId).ToList();
            }

           if (!string.IsNullOrEmpty(filter.stagiaireId))
{
    stages = stages.Where(e => e.StagiaireId == filter.stagiaireId).ToList();
}


            if (filter.year != null)
            {
                stages = stages.Where(e => e.Debut.Year == filter.year).ToList();
            }

            if (filter.dateDebut != null)
            {
                stages = stages.Where(e => e.Debut >= filter.dateDebut).ToList();
            }

            if (filter.dateFin != null)
            {
                stages = stages.Where(e => e.Fin <= filter.dateFin).ToList();
            }

            if (filter.typeStageId != null)
            {
                stages = stages.Where(e => e.TypeStageId == filter.typeStageId).ToList();
            }

            if (filter.typeAnnonceId != null)
            {
                // FIX: Comparing TypeAnnonceId (int?) with filter.typeAnnonceId (int?)
                stages = stages.Where(e => e.TypeAnnonceId == filter.typeAnnonceId).ToList();
            }

            if (filter.sessionId != null)
            {
                stages = stages.Where(e => e.SessionId == filter.sessionId).ToList();
            }

            return stages;
        }
    }
}