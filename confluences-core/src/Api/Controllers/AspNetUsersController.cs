using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Confluences.Persistence;
using Microsoft.AspNetCore.Authorization;
using System.Runtime.CompilerServices;
using Confluences.Domain.Entities;

namespace Api.Controllers
{
    [Authorize(Policy = "Teacher")]
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationUsersController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public ApplicationUsersController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/ApplicationUsers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationUser>>> GetApplicationUsers()
        {
            var ApplicationUsers =  await _context.Users
                            .AsNoTracking()
                            .Include(a => a.SessionStudents)
                                .ThenInclude(a => a.Session)
                                    .ThenInclude(a => a.SchoolClassRoom)
                            .OrderBy(a => a.Firstname)
                            .ToListAsync();

            return ApplicationUsers;
        }

        // GET: api/ApplicationUsers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicationUser>> GetApplicationUser(string id)
        {
            var ApplicationUser = await _context.Users
                                        .Where(a => a.Id == id)
                                        .Include(a => a.Gender)
                                        .Include(a => a.SessionStudents)
                                            .ThenInclude(a => a.Session)
                                                .ThenInclude(a => a.SchoolClassRoom)
                                        .Include(a => a.ParticipantFiles)
                                        .Include(a => a.Marrainages)
                                        .SingleOrDefaultAsync();

            if (ApplicationUser == null)
            {
                return NotFound();
            }

            return ApplicationUser;
        }

        // PUT: api/ApplicationUsers/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutApplicationUser(string id, ApplicationUser ApplicationUser)
        {
            if (id != ApplicationUser.Id)
            {
                return BadRequest();
            }

            _context.Entry(ApplicationUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApplicationUserExists(id))
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

        // DELETE: api/ApplicationUsers/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApplicationUser>> DeleteApplicationUser(string id)
        {
            var ApplicationUser = await _context.Users.FindAsync(id);
            if (ApplicationUser == null)
            {
                return NotFound();
            }

            _context.Users.Remove(ApplicationUser);
            await _context.SaveChangesAsync();

            return ApplicationUser;
        }

        // PATCH: api/ApplicationUsers/5/profile
        [HttpPatch("{id}/profile")]
        public async Task<IActionResult> PatchApplicationUserProfile(string id, ParticipantProfileDto dto)
        {
            var applicationUser = await _context.Users.FindAsync(id);
            if (applicationUser == null)
            {
                return NotFound();
            }

            // Capture des valeurs avant modification, pour les quelques champs dont on trace
            // l'historique (assistant·e social·e + commentaires du suivi formatrices) — pas un
            // audit générique de tout le profil.
            var trackedBefore = new (string FieldName, string? OldValue)[]
            {
                (nameof(ApplicationUser.AssistantSocialEmail), applicationUser.AssistantSocialEmail),
                (nameof(ApplicationUser.PlusHautesEtudesCommentaire), applicationUser.PlusHautesEtudesCommentaire),
                (nameof(ApplicationUser.ActiviteProfSuisseCommentaire), applicationUser.ActiviteProfSuisseCommentaire),
                (nameof(ApplicationUser.RechercheLogementCommentaire), applicationUser.RechercheLogementCommentaire),
                (nameof(ApplicationUser.BesoinsSouhaitsStagesCommentaire), applicationUser.BesoinsSouhaitsStagesCommentaire),
                (nameof(ApplicationUser.BesoinsSouhaitsBenevoletCommentaire), applicationUser.BesoinsSouhaitsBenevoletCommentaire),
            };

            // --- Identité / contact de base ---
            if (!string.IsNullOrWhiteSpace(dto.Firstname)) applicationUser.Firstname = dto.Firstname;
            if (!string.IsNullOrWhiteSpace(dto.LastName))  applicationUser.LastName  = dto.LastName;
            applicationUser.PhoneNumber = dto.PhoneNumber;
            applicationUser.Nationality = dto.Nationality;
            applicationUser.Language    = dto.Language;
            if (dto.Birthday.HasValue)  applicationUser.Birthday = dto.Birthday.Value;
            if (dto.GenderId.HasValue)  applicationUser.GenderId = dto.GenderId.Value;

            applicationUser.ContratMaori = dto.ContratMaori;
            applicationUser.EtatCivil = dto.EtatCivil;
            applicationUser.NombreEnfants = dto.NombreEnfants;
            applicationUser.Adresse = dto.Adresse;
            applicationUser.BeneficiaireDe = dto.BeneficiaireDe;
            applicationUser.AssistantSocialEmail = dto.AssistantSocialEmail;
            applicationUser.AnneeArriveeSuisse = dto.AnneeArriveeSuisse;
            applicationUser.AutresLangues = dto.AutresLangues;
            applicationUser.ParcoursDepuisArriveeSuisse = dto.ParcoursDepuisArriveeSuisse;
            applicationUser.ParcoursFormationSuisse = dto.ParcoursFormationSuisse;
            applicationUser.DomaineEtudeTitreObtenu = dto.DomaineEtudeTitreObtenu;
            applicationUser.ParcoursProfessionnel = dto.ParcoursProfessionnel;
            applicationUser.BesoinsSouhaitsFrancais = dto.BesoinsSouhaitsFrancais;
            applicationUser.Autre = dto.Autre;
            applicationUser.PlusHautesEtudes = dto.PlusHautesEtudes;
            applicationUser.PlusHautesEtudesCommentaire = dto.PlusHautesEtudesCommentaire;
            applicationUser.ActiviteProfSuisse = dto.ActiviteProfSuisse;
            applicationUser.ActiviteProfSuisseCommentaire = dto.ActiviteProfSuisseCommentaire;
            applicationUser.RechercheLogement = dto.RechercheLogement;
            applicationUser.RechercheLogementCommentaire = dto.RechercheLogementCommentaire;
            applicationUser.BesoinsSouhaitsStages = dto.BesoinsSouhaitsStages;
            applicationUser.BesoinsSouhaitsStagesCommentaire = dto.BesoinsSouhaitsStagesCommentaire;
            applicationUser.BesoinsSouhaitsBenevolat = dto.BesoinsSouhaitsBenevolat;
            applicationUser.BesoinsSouhaitsBenevoletCommentaire = dto.BesoinsSouhaitsBenevoletCommentaire;
            applicationUser.NiveauDebutContrat = dto.NiveauDebutContrat;
            applicationUser.NiveauFinContrat = dto.NiveauFinContrat;
            applicationUser.ProfilUpdatedAt = DateTime.UtcNow;

            var trackedAfter = new (string FieldName, string? NewValue)[]
            {
                (nameof(ApplicationUser.AssistantSocialEmail), applicationUser.AssistantSocialEmail),
                (nameof(ApplicationUser.PlusHautesEtudesCommentaire), applicationUser.PlusHautesEtudesCommentaire),
                (nameof(ApplicationUser.ActiviteProfSuisseCommentaire), applicationUser.ActiviteProfSuisseCommentaire),
                (nameof(ApplicationUser.RechercheLogementCommentaire), applicationUser.RechercheLogementCommentaire),
                (nameof(ApplicationUser.BesoinsSouhaitsStagesCommentaire), applicationUser.BesoinsSouhaitsStagesCommentaire),
                (nameof(ApplicationUser.BesoinsSouhaitsBenevoletCommentaire), applicationUser.BesoinsSouhaitsBenevoletCommentaire),
            };

            var changedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            for (int i = 0; i < trackedBefore.Length; i++)
            {
                if (trackedBefore[i].OldValue != trackedAfter[i].NewValue)
                {
                    _context.FieldChangeHistories.Add(new FieldChangeHistory
                    {
                        EntityType = nameof(ApplicationUser),
                        EntityId = applicationUser.Id,
                        FieldName = trackedBefore[i].FieldName,
                        OldValue = trackedBefore[i].OldValue,
                        NewValue = trackedAfter[i].NewValue,
                        ChangedByUserId = changedByUserId,
                        ChangedAt = DateTime.UtcNow,
                    });
                }
            }

            var currentUser = changedByUserId != null ? await _context.Users.FindAsync(changedByUserId) : null;
            applicationUser.ProfilUpdatedByName = currentUser != null
                ? $"{currentUser.Firstname} {currentUser.LastName}"
                : null;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ApplicationUserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }

    public class ParticipantProfileDto
    {
        // --- Identité / contact de base ---
        public string? Firstname { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Nationality { get; set; }
        public string? Language { get; set; }
        public DateTime? Birthday { get; set; }
        public short? GenderId { get; set; }

        public bool ContratMaori { get; set; }
        public string? EtatCivil { get; set; }
        public int? NombreEnfants { get; set; }
        public string? Adresse { get; set; }
        public string? BeneficiaireDe { get; set; }
        public string? AssistantSocialEmail { get; set; }
        public int? AnneeArriveeSuisse { get; set; }
        public string? AutresLangues { get; set; }
        public string? ParcoursDepuisArriveeSuisse { get; set; }
        public string? ParcoursFormationSuisse { get; set; }
        public string? DomaineEtudeTitreObtenu { get; set; }
        public string? ParcoursProfessionnel { get; set; }
        public string? BesoinsSouhaitsFrancais { get; set; }
        public string? Autre { get; set; }
        public string? PlusHautesEtudes { get; set; }
        public string? PlusHautesEtudesCommentaire { get; set; }
        public bool? ActiviteProfSuisse { get; set; }
        public string? ActiviteProfSuisseCommentaire { get; set; }
        public bool? RechercheLogement { get; set; }
        public string? RechercheLogementCommentaire { get; set; }
        public bool? BesoinsSouhaitsStages { get; set; }
        public string? BesoinsSouhaitsStagesCommentaire { get; set; }
        public bool? BesoinsSouhaitsBenevolat { get; set; }
        public string? BesoinsSouhaitsBenevoletCommentaire { get; set; }
        public string? NiveauDebutContrat { get; set; }
        public string? NiveauFinContrat { get; set; }
    }
}
