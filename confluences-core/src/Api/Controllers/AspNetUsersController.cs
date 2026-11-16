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

        // GET: api/ApplicationUsers/localites
        // Couples NPA / localité déjà saisis sur les fiches : suggestions pour l'adresse structurée
        // (le NPA tapé complète la localité quand il est connu).
        [HttpGet("localites")]
        public async Task<ActionResult<IEnumerable<object>>> GetLocalites()
        {
            var pairs = await _context.Users.AsNoTracking()
                .Where(u => u.Npa != null && u.Localite != null)
                .Select(u => new { u.Npa, u.Localite })
                .ToListAsync();

            return pairs
                .GroupBy(p => (Npa: p.Npa!, Localite: p.Localite!.Trim()))
                .OrderBy(g => g.Key.Npa).ThenByDescending(g => g.Count())
                .Select(g => (object)new { npa = g.Key.Npa, localite = g.Key.Localite })
                .ToList();
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
                (nameof(ApplicationUser.BesoinsSouhaitsMarrainageCommentaire), applicationUser.BesoinsSouhaitsMarrainageCommentaire),
                (nameof(ApplicationUser.BesoinsSouhaitsEmploiCommentaire), applicationUser.BesoinsSouhaitsEmploiCommentaire),
                (nameof(ApplicationUser.BesoinsSouhaitsFormationCommentaire), applicationUser.BesoinsSouhaitsFormationCommentaire),
            };

            // --- Identité / contact de base ---
            if (!string.IsNullOrWhiteSpace(dto.Firstname)) applicationUser.Firstname = dto.Firstname;
            if (!string.IsNullOrWhiteSpace(dto.LastName))  applicationUser.LastName  = dto.LastName;
            applicationUser.PhoneNumber = dto.PhoneNumber;
            applicationUser.Nationality = dto.Nationality;
            applicationUser.Language    = dto.Language;
            if (dto.Birthday.HasValue)  applicationUser.Birthday = dto.Birthday.Value;
            if (dto.GenderId.HasValue)  applicationUser.GenderId = dto.GenderId.Value;

            // E-mail modifiable depuis la fiche (jusqu'ici seulement via la plateforme
            // d'enseignement). Le nom d'utilisateur (identifiant de connexion) n'est pas touché.
            var email = dto.Email?.Trim();
            if (!string.IsNullOrEmpty(email) && email != applicationUser.Email)
            {
                var normalizedEmail = email.ToUpperInvariant();
                var emailTaken = await _context.Users
                    .AnyAsync(u => u.Id != id && u.NormalizedEmail == normalizedEmail);
                if (emailTaken)
                {
                    return Conflict("Cette adresse e-mail est déjà utilisée par un autre compte.");
                }
                applicationUser.Email = email;
                applicationUser.NormalizedEmail = normalizedEmail;
            }

            // ContratMaori : se coche uniquement sous Inscriptions (Participant·e·s d'une session) ;
            // la fiche l'affiche en lecture seule, elle ne le modifie donc pas ici.
            applicationUser.EtatCivil = dto.EtatCivil;
            applicationUser.NombreEnfants = dto.NombreEnfants;
            // Adresse structurée : NPA à 4 chiffres et localité = un nom de lieu (pas d'indication
            // libre du type « dans les hauts de Lausanne »). L'adresse sur une ligne est recomposée
            // pour les écrans et documents qui l'utilisent ; tant que les champs structurés sont
            // vides, l'ancienne saisie libre est conservée.
            var rue = Clean(dto.Rue);
            var npa = Clean(dto.Npa);
            var localite = Clean(dto.Localite);
            if (npa != null && !AdresseRules.NpaValide(npa))
                return BadRequest("Le NPA doit comporter 4 chiffres (ex. 1004).");
            if (localite != null && !AdresseRules.LocaliteValide(localite))
                return BadRequest("La localité doit être un nom de lieu (ex. Lausanne), sans indication libre.");
            applicationUser.Rue = rue;
            applicationUser.Npa = npa;
            applicationUser.Localite = localite;
            applicationUser.Adresse = rue != null || npa != null || localite != null
                ? AdresseRules.Composer(rue, npa, localite)
                : Clean(dto.Adresse);
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
            applicationUser.RechercheLogement = TriState(dto.RechercheLogement);
            applicationUser.RechercheLogementCommentaire = dto.RechercheLogementCommentaire;
            applicationUser.BesoinsSouhaitsStages = TriState(dto.BesoinsSouhaitsStages);
            applicationUser.BesoinsSouhaitsStagesCommentaire = dto.BesoinsSouhaitsStagesCommentaire;
            applicationUser.BesoinsSouhaitsBenevolat = TriState(dto.BesoinsSouhaitsBenevolat);
            applicationUser.BesoinsSouhaitsBenevoletCommentaire = dto.BesoinsSouhaitsBenevoletCommentaire;
            applicationUser.BenevolatDomaines = Benevolat.Filtrer(dto.BenevolatDomaines, Benevolat.Domaines);
            applicationUser.BenevolatTypesActivite = Benevolat.Filtrer(dto.BenevolatTypesActivite, Benevolat.TypesActivite);
            var autre = Clean(dto.BenevolatDomaineAutre);
            applicationUser.BenevolatDomaineAutre = autre != null && autre.Length > 200 ? autre.Substring(0, 200) : autre;
            applicationUser.BesoinsSouhaitsMarrainage = TriState(dto.BesoinsSouhaitsMarrainage);
            applicationUser.BesoinsSouhaitsMarrainageCommentaire = dto.BesoinsSouhaitsMarrainageCommentaire;
            applicationUser.DateObtentionPermis = dto.DateObtentionPermis;
            applicationUser.AnneeObtentionPermis = dto.DateObtentionPermis?.Year ?? dto.AnneeObtentionPermis;
            applicationUser.ProjetProfessionnel = dto.ProjetProfessionnel;
            applicationUser.BesoinsSouhaitsSport = TriState(dto.BesoinsSouhaitsSport);
            applicationUser.Disponibilites = dto.Disponibilites;
            applicationUser.SouhaitGenreMarrainage = dto.SouhaitGenreMarrainage;
            applicationUser.SouhaitAgeMarrainage = dto.SouhaitAgeMarrainage;
            applicationUser.PrioriteMarrainage = dto.PrioriteMarrainage;
            applicationUser.DegreInteretStage = dto.DegreInteretStage;
            applicationUser.Region = string.IsNullOrWhiteSpace(dto.Region) ? null : dto.Region.Trim();
            applicationUser.TitresObtenus = dto.TitresObtenus;
            applicationUser.ActiviteProfOrigine = dto.ActiviteProfOrigine;
            applicationUser.BesoinsSouhaitsEmploi = TriState(dto.BesoinsSouhaitsEmploi);
            applicationUser.BesoinsSouhaitsEmploiCommentaire = dto.BesoinsSouhaitsEmploiCommentaire;
            applicationUser.BesoinsSouhaitsFormation = TriState(dto.BesoinsSouhaitsFormation);
            applicationUser.BesoinsSouhaitsFormationCommentaire = dto.BesoinsSouhaitsFormationCommentaire;
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
                (nameof(ApplicationUser.BesoinsSouhaitsMarrainageCommentaire), applicationUser.BesoinsSouhaitsMarrainageCommentaire),
                (nameof(ApplicationUser.BesoinsSouhaitsEmploiCommentaire), applicationUser.BesoinsSouhaitsEmploiCommentaire),
                (nameof(ApplicationUser.BesoinsSouhaitsFormationCommentaire), applicationUser.BesoinsSouhaitsFormationCommentaire),
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

        private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        // Besoins / souhaits : "oui", "non" ou "a_verifier" (toute autre valeur = non renseigné).
        private static string? TriState(string? value) =>
            value is "oui" or "non" or "a_verifier" ? value : null;

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
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Nationality { get; set; }
        public string? Language { get; set; }
        public DateTime? Birthday { get; set; }
        public short? GenderId { get; set; }

        public bool ContratMaori { get; set; }
        public string? EtatCivil { get; set; }
        public int? NombreEnfants { get; set; }
        public string? Adresse { get; set; }
        public string? Rue { get; set; }
        public string? Npa { get; set; }
        public string? Localite { get; set; }
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
        public string? RechercheLogement { get; set; }
        public string? RechercheLogementCommentaire { get; set; }
        public string? BesoinsSouhaitsStages { get; set; }
        public string? BesoinsSouhaitsStagesCommentaire { get; set; }
        public string? BesoinsSouhaitsBenevolat { get; set; }
        public string? BesoinsSouhaitsBenevoletCommentaire { get; set; }
        public List<string>? BenevolatDomaines { get; set; }
        public string? BenevolatDomaineAutre { get; set; }
        public List<string>? BenevolatTypesActivite { get; set; }
        public string? BesoinsSouhaitsMarrainage { get; set; }
        public string? BesoinsSouhaitsMarrainageCommentaire { get; set; }
        public DateTime? DateObtentionPermis { get; set; }
        public int? AnneeObtentionPermis { get; set; }
        public string? ProjetProfessionnel { get; set; }
        public string? BesoinsSouhaitsSport { get; set; }
        public string? Disponibilites { get; set; }
        public string? SouhaitGenreMarrainage { get; set; }
        public string? SouhaitAgeMarrainage { get; set; }
        public string? PrioriteMarrainage { get; set; }
        public string? DegreInteretStage { get; set; }
        public string? Region { get; set; }
        public string? TitresObtenus { get; set; }
        public bool? ActiviteProfOrigine { get; set; }
        public string? BesoinsSouhaitsEmploi { get; set; }
        public string? BesoinsSouhaitsEmploiCommentaire { get; set; }
        public string? BesoinsSouhaitsFormation { get; set; }
        public string? BesoinsSouhaitsFormationCommentaire { get; set; }
        public string? NiveauDebutContrat { get; set; }
        public string? NiveauFinContrat { get; set; }
    }

    // Règles de l'adresse structurée des participant·e·s.
    public static class AdresseRules
    {
        private static readonly System.Text.RegularExpressions.Regex NpaRegex = new(@"^\d{4}$");
        // Lettres (accents compris), espaces, tirets, apostrophes, points, parenthèses : « Lausanne »,
        // « La Tour-de-Peilz », « Yverdon-les-Bains », « St-Légier », « Château-d'Oex ».
        private static readonly System.Text.RegularExpressions.Regex LocaliteRegex = new(@"^\p{L}[\p{L}\s'’.\-()]*$");
        // Mots qui trahissent une indication libre plutôt qu'un nom de localité.
        private static readonly HashSet<string> MotsLibres = new(StringComparer.OrdinalIgnoreCase)
        {
            "dans", "près", "pres", "vers", "environ", "hauts", "haut", "quartier", "proche",
            "côté", "cote", "région", "region", "centre", "à", "au", "aux",
        };

        public static bool NpaValide(string npa) => NpaRegex.IsMatch(npa);

        public static bool LocaliteValide(string localite)
        {
            if (localite.Length > 60 || !LocaliteRegex.IsMatch(localite)) return false;
            var mots = localite.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return mots.Length <= 4 && !mots.Any(m => MotsLibres.Contains(m));
        }

        public static string Composer(string? rue, string? npa, string? localite)
        {
            var ville = string.Join(" ", new[] { npa, localite }.Where(x => !string.IsNullOrEmpty(x)));
            return string.Join(", ", new[] { rue, ville }.Where(x => !string.IsNullOrEmpty(x)));
        }
    }

    // Bénévolat – domaines d'intérêt et types d'activité proposés sur la fiche participant·e
    // (même liste côté application : src/utils/benevolat.js).
    public static class Benevolat
    {
        public static readonly string[] Domaines =
        {
            "Social et solidarité", "Personnes âgées", "Enfance et jeunesse", "Santé et handicap",
            "Cuisine et restauration", "Nature et environnement", "Animaux", "Recyclage et seconde main",
            "Art et culture", "Sport et loisirs", "Commerce et logistique", "Entretien et travaux manuels",
            "Événementiel", "Administration et informatique", "Autre (à préciser)",
        };

        public static readonly string[] TypesActivite =
        {
            "Activités manuelles ou pratiques", "Activités en contact avec des personnes",
            "Activités administratives ou informatiques", "Sans préférence",
        };

        // Ne garde que les valeurs de la liste, dans l'ordre de la liste, sans doublon.
        public static List<string> Filtrer(IEnumerable<string>? valeurs, string[] liste)
        {
            var choisies = new HashSet<string>(valeurs ?? Enumerable.Empty<string>());
            return liste.Where(choisies.Contains).ToList();
        }
    }
}
