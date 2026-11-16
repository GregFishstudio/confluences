using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Confluences.Domain.Entities;
using Confluences.Persistence;

namespace Api.Controllers
{
    // Tableau "Stages et bénévolat" : participant·e·s qui cherchent un stage ou du bénévolat, avec
    // leurs métiers/domaines d'intérêt, et les entreprises qui proposent ces métiers/domaines.
    [Authorize(Policy = "Teacher")]
    [Route("api/[controller]")]
    [ApiController]
    public class StagesBenevolatController : ControllerBase
    {
        private static readonly string[] Recherche = { "oui", "a_verifier" };
        private readonly ConfluencesDbContext _context;

        public StagesBenevolatController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/StagesBenevolat/interets/abc-123
        [HttpGet("interets/{participantId}")]
        public async Task<ActionResult<InteretsDto>> GetInterets(string participantId)
        {
            return new InteretsDto
            {
                MetierIds = await _context.ParticipantMetiers.Where(pm => pm.ParticipantId == participantId)
                    .Select(pm => pm.TypeMetierId).ToListAsync(),
                DomaineIds = await _context.ParticipantDomaines.Where(pd => pd.ParticipantId == participantId)
                    .Select(pd => pd.TypeDomaineId).ToListAsync(),
            };
        }

        // PUT: api/StagesBenevolat/interets/abc-123 — remplace les métiers et domaines d'intérêt.
        [HttpPut("interets/{participantId}")]
        public async Task<IActionResult> PutInterets(string participantId, InteretsDto dto)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == participantId)) return NotFound();

            _context.ParticipantMetiers.RemoveRange(
                await _context.ParticipantMetiers.Where(pm => pm.ParticipantId == participantId).ToListAsync());
            _context.ParticipantDomaines.RemoveRange(
                await _context.ParticipantDomaines.Where(pd => pd.ParticipantId == participantId).ToListAsync());

            foreach (var id in (dto.MetierIds ?? new List<int>()).Distinct())
                _context.ParticipantMetiers.Add(new ParticipantMetier { ParticipantId = participantId, TypeMetierId = id });
            foreach (var id in (dto.DomaineIds ?? new List<int>()).Distinct())
                _context.ParticipantDomaines.Add(new ParticipantDomaine { ParticipantId = participantId, TypeDomaineId = id });

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // GET: api/StagesBenevolat/tableau
        [HttpGet("tableau")]
        public async Task<ActionResult<object>> GetTableau()
        {
            var today = DateTime.Today;
            int? AgeDe(DateTime naissance)
            {
                if (naissance.Year < 1900) return null;
                var a = today.Year - naissance.Year;
                if (naissance.Date > today.AddYears(-a)) a--;
                return a;
            }

            var users = await _context.Users.AsNoTracking()
                .Where(u => Recherche.Contains(u.BesoinsSouhaitsStages!) || Recherche.Contains(u.BesoinsSouhaitsBenevolat!))
                .Select(u => new
                {
                    u.Id, u.Firstname, u.LastName, Genre = u.Gender!.GenderName, u.Birthday, u.Adresse, u.Localite, u.Region,
                    u.BenevolatDomaines, u.BenevolatDomaineAutre, u.BenevolatTypesActivite,
                    u.BesoinsSouhaitsStages, u.BesoinsSouhaitsBenevolat, u.DegreInteretStage, u.Disponibilites,
                    Session = u.SessionStudents!
                        .OrderByDescending(ss => ss.Session!.DateStart)
                        .Select(ss => new { Year = ss.Session!.DateStart.Year, Number = ss.Session.SessionNumberId, Classe = ss.Session.SchoolClassRoom!.SchoolClassRoomName })
                        .FirstOrDefault(),
                })
                .ToListAsync();
            var ids = users.Select(u => u.Id).ToList();

            var metiers = (await _context.ParticipantMetiers.AsNoTracking()
                    .Where(pm => ids.Contains(pm.ParticipantId))
                    .Select(pm => new { pm.ParticipantId, Id = pm.TypeMetierId, pm.TypeMetier!.Libelle })
                    .ToListAsync())
                .ToLookup(x => x.ParticipantId);
            var domaines = (await _context.ParticipantDomaines.AsNoTracking()
                    .Where(pd => ids.Contains(pd.ParticipantId))
                    .Select(pd => new { pd.ParticipantId, Id = pd.TypeDomaineId, pd.TypeDomaine!.Libelle })
                    .ToListAsync())
                .ToLookup(x => x.ParticipantId);

            // Dernier commentaire des rubriques Stage et Bénévolat.
            var comments = (await _context.ParticipantComments.AsNoTracking()
                    .Where(c => ids.Contains(c.StudentId) && (c.Rubrique == "stages" || c.Rubrique == "benevolat"))
                    .Select(c => new { c.StudentId, c.Rubrique, c.Text, c.CreatedAt, c.AuthorName })
                    .ToListAsync())
                .GroupBy(c => c.StudentId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(c => c.CreatedAt).First());

            // Stages / bénévolats en cours.
            var activites = (await _context.Stages.AsNoTracking()
                    .Where(s => ids.Contains(s.StagiaireId) && s.Debut <= today && (s.Fin == null || s.Fin >= today))
                    .Select(s => new
                    {
                        s.StagiaireId, s.StageId, Type = s.TypeIntershipActivity!.Nom, Metier = s.TypeMetier!.Libelle,
                        Entreprise = s.Entreprise!.Nom, s.Debut, s.Fin,
                    })
                    .ToListAsync())
                .ToLookup(s => s.StagiaireId);

            var participants = users.Select(u => new
            {
                id = u.Id, firstname = u.Firstname, lastName = u.LastName, genre = u.Genre, age = AgeDe(u.Birthday),
                adresse = u.Adresse, localite = u.Localite, region = u.Region,
                benevolatDomaines = u.BenevolatDomaines, benevolatDomaineAutre = u.BenevolatDomaineAutre,
                benevolatTypesActivite = u.BenevolatTypesActivite, besoinStage = u.BesoinsSouhaitsStages,
                besoinBenevolat = u.BesoinsSouhaitsBenevolat, degre = u.DegreInteretStage, disponibilites = u.Disponibilites,
                session = u.Session,
                metiers = metiers[u.Id].Select(m => new { m.Id, m.Libelle }),
                domaines = domaines[u.Id].Select(d => new { d.Id, d.Libelle }),
                dernierCommentaire = comments.TryGetValue(u.Id, out var c)
                    ? new { c.Rubrique, c.Text, c.CreatedAt, c.AuthorName } : null,
                activites = activites[u.Id].Select(a => new { a.StageId, a.Type, a.Metier, a.Entreprise, a.Debut, a.Fin }),
            });

            // Entreprises qui ont au moins un métier ou un domaine renseigné (pour les rapprochements).
            var entreprises = await _context.Entreprises.AsNoTracking()
                .Where(e => e.EntrepriseMetiers!.Any() || e.EntrepriseDomaines!.Any())
                .Select(e => new
                {
                    id = e.EntrepriseId, nom = e.Nom, ville = e.Ville,
                    metierIds = e.EntrepriseMetiers!.Select(m => m.TypeMetierId),
                    domaineIds = e.EntrepriseDomaines!.Select(d => d.TypeDomaineId),
                })
                .ToListAsync();

            return Ok(new { participants, entreprises });
        }
    }

    public class InteretsDto
    {
        public List<int>? MetierIds { get; set; }
        public List<int>? DomaineIds { get; set; }
    }
}
