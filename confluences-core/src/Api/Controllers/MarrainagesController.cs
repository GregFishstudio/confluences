using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Confluences.Persistence;
using Microsoft.AspNetCore.Authorization;
using Confluences.Domain.Entities;

namespace Api.Controllers
{
    [Authorize(Policy = "Teacher")]
    [Route("api/[controller]")]
    [ApiController]
    public class MarrainagesController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public MarrainagesController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/Marrainages?search=&participantId=&actif=
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Marrainage>>> GetMarrainages(
            [FromQuery] string? search,
            [FromQuery] string? participantId,
            [FromQuery] bool? actif)
        {
            await PasserNouveauxEnAttente();

            var query = _context.Marrainages
                .AsNoTracking()
                .Include(m => m.Participant)
                .Include(m => m.ParticipantLinks!)
                    .ThenInclude(pl => pl.Participant)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(participantId))
            {
                query = query.Where(m =>
                    m.ParticipantId == participantId ||
                    (m.ParticipantLinks != null && m.ParticipantLinks.Any(pl => pl.ParticipantId == participantId)));
            }

            if (actif.HasValue)
            {
                query = query.Where(m => m.Actif == actif.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(m =>
                    m.Nom.ToLower().Contains(s) ||
                    m.Prenom.ToLower().Contains(s) ||
                    (m.Commune != null && m.Commune.ToLower().Contains(s)) ||
                    (m.Participant != null && (m.Participant.Firstname.ToLower().Contains(s) || m.Participant.LastName.ToLower().Contains(s))));
            }

            var list = await query.OrderBy(m => m.Nom).ThenBy(m => m.Prenom).ToListAsync();

            // Date du dernier commentaire de chaque fiche (suivi "Prendre des nouvelles").
            var ids = list.Select(m => m.MarrainageId).ToList();
            var derniers = await _context.MarrainageComments.AsNoTracking()
                .Where(c => ids.Contains(c.MarrainageId))
                .GroupBy(c => c.MarrainageId)
                .Select(g => new { MarrainageId = g.Key, Date = g.Max(c => c.CreatedAt) })
                .ToDictionaryAsync(x => x.MarrainageId, x => x.Date);
            foreach (var m in list)
                if (derniers.TryGetValue(m.MarrainageId, out var d)) m.DernierCommentaire = d;

            return list;
        }

        // "Nouveau" reste un mois ; passé ce délai sans binôme constitué (aucune relation avec un·e
        // participant·e), la marraine/le parrain passe automatiquement "En attente de binôme".
        private async Task PasserNouveauxEnAttente()
        {
            var limite = System.DateTime.UtcNow.AddMonths(-1);
            var aPasser = await _context.Marrainages
                .Where(m => m.Statut == "nouveau" && m.CreatedAt != null && m.CreatedAt < limite
                    && m.ParticipantId == null
                    && !_context.MarrainageParticipants.Any(mp => mp.MarrainageId == m.MarrainageId))
                .ToListAsync();
            if (aPasser.Count == 0) return;
            foreach (var m in aPasser) m.Statut = "en_attente";
            await _context.SaveChangesAsync();
        }

        // GET: api/Marrainages/filleuls-en-attente
        // Participant·e·s qui souhaitent un marrainage ("oui" ou "à vérifier" sur leur fiche) et
        // n'ont aucune relation de marrainage active — pour le tableau "à trouver" par région.
        [HttpGet("filleuls-en-attente")]
        public async Task<ActionResult<IEnumerable<object>>> GetFilleulsEnAttente()
        {
            var users = await _context.Users.AsNoTracking()
                .Where(u => u.BesoinsSouhaitsMarrainage == "oui" || u.BesoinsSouhaitsMarrainage == "a_verifier")
                .Where(u => !_context.MarrainageParticipants.Any(mp => mp.ParticipantId == u.Id && mp.Actif))
                .Select(u => new
                {
                    u.Id,
                    u.Firstname,
                    u.LastName,
                    u.Adresse,
                    besoin = u.BesoinsSouhaitsMarrainage,
                    u.SouhaitGenreMarrainage,
                    u.SouhaitAgeMarrainage,
                    u.Disponibilites,
                    // Classe de la session la plus récente (télécours / F&I A1 / A2).
                    classe = u.SessionStudents!
                        .OrderByDescending(ss => ss.Session!.DateStart)
                        .Select(ss => ss.Session!.SchoolClassRoom!.SchoolClassRoomName)
                        .FirstOrDefault(),
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/Marrainages/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Marrainage>> GetMarrainage(int id)
        {
            var marrainage = await _context.Marrainages
                .Include(m => m.Participant)
                .Include(m => m.ParticipantLinks!)
                    .ThenInclude(pl => pl.Participant)
                .Include(m => m.Comments!.OrderByDescending(c => c.CreatedAt))
                    .ThenInclude(c => c.Author)
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.MarrainageId == id);

            if (marrainage == null)
            {
                return NotFound();
            }

            // Métadonnées des fichiers seulement : charger les octets (CV, extraits de casier…)
            // alourdissait fortement la réponse et ralentissait, voire bloquait, l'ouverture de la
            // fiche. Le contenu se télécharge à la demande via api/MarrainageFiles/{id}.
            marrainage.Files = await _context.MarrainageFiles
                .AsNoTracking()
                .Where(f => f.MarrainageId == id)
                .Select(f => new MarrainageFile
                {
                    MarrainageFileId = f.MarrainageFileId,
                    FileName = f.FileName,
                    FileType = f.FileType,
                    Category = f.Category,
                    MarrainageId = f.MarrainageId
                })
                .ToListAsync();

            return marrainage;
        }

        // POST: api/Marrainages
        [HttpPost]
        public async Task<ActionResult<Marrainage>> PostMarrainage(Marrainage marrainage)
        {
            // Toute nouvelle inscription de marraine/parrain apparaît "Nouveau" dans la liste
            // tant que les formatrices ne lui ont pas attribué un autre statut.
            if (string.IsNullOrWhiteSpace(marrainage.Statut)) marrainage.Statut = "nouveau";
            marrainage.CreatedAt = System.DateTime.UtcNow;
            marrainage.UpdatedAt = marrainage.CreatedAt;

            _context.Marrainages.Add(marrainage);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMarrainage", new { id = marrainage.MarrainageId }, marrainage);
        }

        // POST: api/Marrainages/import
        // Met à jour les marrainages déjà importés (retrouvés par nom du parrain/marraine)
        // et ajoute les nouveaux, sans écraser un champ existant avec une valeur vide du CSV.
        [HttpPost("import")]
        public async Task<ActionResult<IEnumerable<Marrainage>>> ImportMarrainages(List<Marrainage> marrainages)
        {
            var existingByNom = (await _context.Marrainages.ToListAsync())
                .GroupBy(m => NormalizeName(m.Nom))
                .Where(g => !string.IsNullOrEmpty(g.Key))
                .ToDictionary(g => g.Key, g => g.First());

            var result = new List<Marrainage>();

            foreach (var incoming in marrainages)
            {
                var key = NormalizeName(incoming.Nom);
                if (!string.IsNullOrEmpty(key) && existingByNom.TryGetValue(key, out var match))
                {
                    ApplyIfNotEmpty(v => match.Prenom = v, incoming.Prenom);
                    if (incoming.Date.HasValue) match.Date = incoming.Date;
                    ApplyIfNotEmpty(v => match.Numero = v, incoming.Numero);
                    ApplyIfNotEmpty(v => match.RemarqueParrain = v, incoming.RemarqueParrain);
                    ApplyIfNotEmpty(v => match.Commune = v, incoming.Commune);
                    ApplyIfNotEmpty(v => match.Email = v, incoming.Email);
                    ApplyIfNotEmpty(v => match.EtatCivil = v, incoming.EtatCivil);
                    ApplyIfNotEmpty(v => match.Profession = v, incoming.Profession);
                    if (incoming.DateNaissance.HasValue) match.DateNaissance = incoming.DateNaissance;
                    if (incoming.Age.HasValue) match.Age = incoming.Age;
                    ApplyIfNotEmpty(v => match.PeriodeDemande = v, incoming.PeriodeDemande);
                    ApplyIfNotEmpty(v => match.FilleulNom = v, incoming.FilleulNom);
                    ApplyIfNotEmpty(v => match.TelephoneFilleul = v, incoming.TelephoneFilleul);
                    ApplyIfNotEmpty(v => match.EmailFilleul = v, incoming.EmailFilleul);
                    ApplyIfNotEmpty(v => match.Suivi = v, incoming.Suivi);
                    ApplyIfNotEmpty(v => match.ExtraitCasier = v, incoming.ExtraitCasier);
                    ApplyIfNotEmpty(v => match.RelanceNotes = v, incoming.RelanceNotes);
                    if (incoming.CharteSignee) match.CharteSignee = true;

                    result.Add(match);
                }
                else
                {
                    incoming.MarrainageId = 0;
                    incoming.CreatedAt = System.DateTime.UtcNow;
                    incoming.UpdatedAt = incoming.CreatedAt;
                    _context.Marrainages.Add(incoming);
                    result.Add(incoming);
                    if (!string.IsNullOrEmpty(key)) existingByNom[key] = incoming;
                }
            }

            await _context.SaveChangesAsync();

            return result;
        }

        // POST: api/Marrainages/5/participants
        [HttpPost("{id}/participants")]
        public async Task<IActionResult> AddParticipant(int id, [FromBody] AddParticipantDto dto)
        {
            var marrainage = await _context.Marrainages.FindAsync(id);
            if (marrainage == null) return NotFound();

            var exists = await _context.MarrainageParticipants
                .AnyAsync(mp => mp.MarrainageId == id && mp.ParticipantId == dto.ParticipantId);

            if (!exists)
            {
                _context.MarrainageParticipants.Add(new MarrainageParticipant
                {
                    MarrainageId = id,
                    ParticipantId = dto.ParticipantId,
                    DateDebut = dto.DateDebut ?? System.DateTime.Today,
                    Actif = true
                });
                marrainage.UpdatedAt = System.DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        // DELETE: api/Marrainages/5/participants/abc-123
        [HttpDelete("{id}/participants/{participantId}")]
        public async Task<IActionResult> RemoveParticipant(int id, string participantId)
        {
            var link = await _context.MarrainageParticipants
                .SingleOrDefaultAsync(mp => mp.MarrainageId == id && mp.ParticipantId == participantId);

            if (link == null) return NotFound();

            _context.MarrainageParticipants.Remove(link);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/Marrainages/5/participants/abc-123
        // Met à jour les dates et le statut d'une relation de marrainage précise
        // (une même marraine peut avoir plusieurs participant·e·s, chacun·e avec son propre suivi).
        [HttpPatch("{id}/participants/{participantId}")]
        public async Task<IActionResult> PatchParticipantLink(int id, string participantId, [FromBody] ParticipantLinkDto dto)
        {
            var link = await _context.MarrainageParticipants
                .SingleOrDefaultAsync(mp => mp.MarrainageId == id && mp.ParticipantId == participantId);

            if (link == null) return NotFound();

            link.DateDebut = dto.DateDebut;
            link.DateFin = dto.DateFin;
            link.Actif = dto.Actif;
            link.RaisonInactivite = dto.Actif ? null : dto.RaisonInactivite;
            var parent = await _context.Marrainages.FindAsync(id);
            if (parent != null) parent.UpdatedAt = System.DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/Marrainages/5/statut
        [HttpPatch("{id}/statut")]
        public async Task<IActionResult> PatchStatut(int id, [FromBody] StatutDto dto)
        {
            var marrainage = await _context.Marrainages.FindAsync(id);
            if (marrainage == null) return NotFound();

            marrainage.Actif = dto.Actif;
            marrainage.RaisonInactivite = dto.Actif ? null : dto.RaisonInactivite;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Marrainages/5/merge  { duplicateId: 12 }
        // Fusionne "duplicateId" dans "id" (celui conservé) : rapatrie commentaires, fichiers,
        // participant·e·s lié·e·s et réponses au questionnaire, complète les champs vides du
        // conservé avec ceux du doublon, puis supprime le doublon.
        [HttpPost("{id}/merge")]
        public async Task<ActionResult<Marrainage>> MergeMarrainage(int id, [FromBody] MergeDto dto)
        {
            if (id == dto.DuplicateId) return BadRequest("Impossible de fusionner un marrainage avec lui-même.");

            var keep = await _context.Marrainages.FindAsync(id);
            var duplicate = await _context.Marrainages.FindAsync(dto.DuplicateId);
            if (keep == null || duplicate == null) return NotFound();

            // Complète les champs vides du conservé avec les valeurs du doublon.
            ApplyIfNotEmpty(v => keep.Numero = v, duplicate.Numero);
            ApplyIfNotEmpty(v => keep.Commune = v, duplicate.Commune);
            ApplyIfNotEmpty(v => keep.Email = v, duplicate.Email);
            ApplyIfNotEmpty(v => keep.EtatCivil = v, duplicate.EtatCivil);
            ApplyIfNotEmpty(v => keep.Profession = v, duplicate.Profession);
            if (keep.Age == null) keep.Age = duplicate.Age;
            if (keep.DateNaissance == null) keep.DateNaissance = duplicate.DateNaissance;
            ApplyIfNotEmpty(v => keep.PeriodeDemande = v, duplicate.PeriodeDemande);
            ApplyIfNotEmpty(v => keep.RemarqueParrain = v, duplicate.RemarqueParrain);
            ApplyIfNotEmpty(v => keep.FilleulNom = v, duplicate.FilleulNom);
            ApplyIfNotEmpty(v => keep.TelephoneFilleul = v, duplicate.TelephoneFilleul);
            ApplyIfNotEmpty(v => keep.EmailFilleul = v, duplicate.EmailFilleul);
            ApplyIfNotEmpty(v => keep.Suivi = v, duplicate.Suivi);
            ApplyIfNotEmpty(v => keep.ExtraitCasier = v, duplicate.ExtraitCasier);
            ApplyIfNotEmpty(v => keep.RelanceNotes = v, duplicate.RelanceNotes);
            if (duplicate.CharteSignee) keep.CharteSignee = true;
            if (keep.Date == null) keep.Date = duplicate.Date;
            if (keep.ParticipantId == null) keep.ParticipantId = duplicate.ParticipantId;

            // Commentaires et fichiers : simple ré-affectation au conservé.
            await _context.MarrainageComments
                .Where(c => c.MarrainageId == dto.DuplicateId)
                .ForEachAsync(c => c.MarrainageId = id);
            await _context.MarrainageFiles
                .Where(f => f.MarrainageId == dto.DuplicateId)
                .ForEachAsync(f => f.MarrainageId = id);

            // Participant·e·s lié·e·s : ré-affecte sauf si déjà lié·e au conservé (sinon conflit de clé).
            var keepParticipantIds = await _context.MarrainageParticipants
                .Where(mp => mp.MarrainageId == id)
                .Select(mp => mp.ParticipantId)
                .ToListAsync();
            var duplicateLinks = await _context.MarrainageParticipants
                .Where(mp => mp.MarrainageId == dto.DuplicateId)
                .ToListAsync();
            foreach (var link in duplicateLinks)
            {
                if (keepParticipantIds.Contains(link.ParticipantId))
                    _context.MarrainageParticipants.Remove(link);
                else
                    link.MarrainageId = id;
            }

            // Réponses au questionnaire : celles du doublon l'emportent en cas de question
            // déjà répondue sur les deux fiches (le doublon vient souvent d'un import plus récent).
            var keepAnswers = await _context.MarrainageAnswers
                .Where(a => a.MarrainageId == id)
                .ToListAsync();
            var duplicateAnswers = await _context.MarrainageAnswers
                .Where(a => a.MarrainageId == dto.DuplicateId)
                .ToListAsync();
            foreach (var da in duplicateAnswers)
            {
                var existing = keepAnswers.FirstOrDefault(a => a.MarrainageQuestionId == da.MarrainageQuestionId);
                if (existing != null)
                {
                    existing.Value = da.Value;
                    _context.MarrainageAnswers.Remove(da);
                }
                else
                {
                    da.MarrainageId = id;
                }
            }

            _context.Marrainages.Remove(duplicate);
            await _context.SaveChangesAsync();

            return await GetMarrainage(id);
        }

        // DELETE: api/Marrainages/all
        [HttpDelete("all")]
        public async Task<IActionResult> DeleteAllMarrainages()
        {
            _context.Marrainages.RemoveRange(_context.Marrainages);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static string NormalizeName(string? s) => (s ?? string.Empty).Trim().ToLowerInvariant();

        private static void ApplyIfNotEmpty(System.Action<string> setter, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value)) setter(value);
        }

        // PUT: api/Marrainages/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMarrainage(int id, Marrainage marrainage)
        {
            if (id != marrainage.MarrainageId)
            {
                return BadRequest();
            }

            _context.Entry(marrainage).State = EntityState.Modified;
            // Champs gérés par le serveur : jamais écrasés par le formulaire.
            _context.Entry(marrainage).Property(m => m.CreatedAt).IsModified = false;
            _context.Entry(marrainage).Property(m => m.QuestionnaireSaisiPar).IsModified = false;
            _context.Entry(marrainage).Property(m => m.QuestionnaireSaisiLe).IsModified = false;
            marrainage.UpdatedAt = System.DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MarrainageExists(id))
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

        // DELETE: api/Marrainages/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Marrainage>> DeleteMarrainage(int id)
        {
            var marrainage = await _context.Marrainages.FindAsync(id);
            if (marrainage == null)
            {
                return NotFound();
            }

            _context.Marrainages.Remove(marrainage);
            await _context.SaveChangesAsync();

            return marrainage;
        }

        private bool MarrainageExists(int id)
        {
            return _context.Marrainages.Any(e => e.MarrainageId == id);
        }
    }

    public class AddParticipantDto
    {
        public string ParticipantId { get; set; } = string.Empty;
        public System.DateTime? DateDebut { get; set; }
    }

    public class ParticipantLinkDto
    {
        public System.DateTime? DateDebut { get; set; }
        public System.DateTime? DateFin { get; set; }
        public bool Actif { get; set; } = true;
        public string? RaisonInactivite { get; set; }
    }

    public class StatutDto
    {
        public bool Actif { get; set; }
        public string? RaisonInactivite { get; set; }
    }

    public class MergeDto
    {
        public int DuplicateId { get; set; }
    }
}
