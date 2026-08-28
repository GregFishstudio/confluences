using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Confluences.Persistence;

namespace Api.Controllers
{
    [Authorize(Policy = "Teacher")]
    [Route("api/[controller]")]
    [ApiController]
    public class StagiaireNiveauxController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public StagiaireNiveauxController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/StagiaireNiveaux
        // Historique complet des niveaux oral/écrit par stagiaire et par période
        // (année + trimestre), utilisé par la fiche participant pour reconstituer
        // l'historique de session.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetStagiaireNiveaux()
        {
            var niveaux = await _context.StagiaireNiveaux
                .AsNoTracking()
                .Select(n => new
                {
                    stagiaireId  = n.StagiaireId,
                    annee        = n.Annee,
                    trimestre    = n.Trimestre,
                    oralLevel    = n.OralLevel,
                    writtenLevel = n.WrittenLevel,
                })
                .ToListAsync();

            return Ok(niveaux);
        }
    }
}
