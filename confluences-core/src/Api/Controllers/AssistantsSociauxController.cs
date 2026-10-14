using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Confluences.Persistence;
using Confluences.Domain.Entities;

namespace Api.Controllers
{
    [Authorize(Policy = "Teacher")]
    [Route("api/[controller]")]
    [ApiController]
    public class AssistantsSociauxController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public AssistantsSociauxController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/AssistantsSociaux
        // Avec le nombre de participant·e·s (par email) et de candidatures rattaché·e·s, pour
        // savoir d'un coup d'œil qui est encore actif·ve.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAssistantsSociaux()
        {
            var list = await _context.AssistantsSociaux.AsNoTracking()
                .OrderBy(a => a.Nom).ThenBy(a => a.Prenom)
                .ToListAsync();

            var participantCounts = await _context.Users
                .Where(u => u.AssistantSocialEmail != null && u.AssistantSocialEmail != "")
                .GroupBy(u => u.AssistantSocialEmail!.ToLower())
                .Select(g => new { Email = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Email, x => x.Count);

            var candidatureCounts = await _context.Candidatures
                .Where(c => c.AssistantSocialId != null)
                .GroupBy(c => c.AssistantSocialId!.Value)
                .Select(g => new { Id = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Id, x => x.Count);

            return Ok(list.Select(a => new
            {
                a.AssistantSocialId,
                a.Prenom,
                a.Nom,
                a.Email,
                a.Telephone,
                a.Organisme,
                a.Remarques,
                a.Actif,
                nbParticipants = participantCounts.GetValueOrDefault(a.Email.ToLower()),
                nbCandidatures = candidatureCounts.GetValueOrDefault(a.AssistantSocialId),
            }));
        }

        // POST: api/AssistantsSociaux
        [HttpPost]
        public async Task<ActionResult<AssistantSocial>> PostAssistantSocial(AssistantSocialDto dto)
        {
            var email = (dto.Email ?? "").Trim();
            if (string.IsNullOrEmpty(email)) return BadRequest("L'email est requis.");
            if (await EmailTaken(email, null)) return Conflict("Un·e AS avec cet email existe déjà.");

            var a = new AssistantSocial();
            Apply(a, dto);
            _context.AssistantsSociaux.Add(a);
            await _context.SaveChangesAsync();

            return Ok(a);
        }

        // PUT: api/AssistantsSociaux/5
        // Si l'email change, on le répercute sur les fiches participant qui utilisaient l'ancien,
        // pour que le lien (par valeur) ne se perde pas.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAssistantSocial(int id, AssistantSocialDto dto)
        {
            var a = await _context.AssistantsSociaux.FindAsync(id);
            if (a == null) return NotFound();

            var email = (dto.Email ?? "").Trim();
            if (string.IsNullOrEmpty(email)) return BadRequest("L'email est requis.");
            if (await EmailTaken(email, id)) return Conflict("Un·e AS avec cet email existe déjà.");

            var oldEmail = a.Email;
            Apply(a, dto);

            if (!string.Equals(oldEmail, a.Email, StringComparison.OrdinalIgnoreCase))
            {
                var oldLower = oldEmail.ToLower();
                var users = await _context.Users
                    .Where(u => u.AssistantSocialEmail != null && u.AssistantSocialEmail.ToLower() == oldLower)
                    .ToListAsync();
                foreach (var u in users) u.AssistantSocialEmail = a.Email;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/AssistantsSociaux/5
        // Les candidatures liées gardent leur historique (FK mise à null) ; les fiches participant
        // gardent l'email tel quel.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssistantSocial(int id)
        {
            var a = await _context.AssistantsSociaux.FindAsync(id);
            if (a == null) return NotFound();

            _context.AssistantsSociaux.Remove(a);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/AssistantsSociaux/import-participants
        // Crée une fiche AS pour chaque email saisi sur les fiches participant et pas encore
        // présent dans le carnet — prénom/nom devinés à partir de "prenom.nom@...".
        [HttpPost("import-participants")]
        public async Task<ActionResult<object>> ImportFromParticipants()
        {
            var existing = (await _context.AssistantsSociaux.Select(a => a.Email.ToLower()).ToListAsync()).ToHashSet();

            var emails = (await _context.Users
                    .Where(u => u.AssistantSocialEmail != null && u.AssistantSocialEmail != "")
                    .Select(u => u.AssistantSocialEmail!)
                    .ToListAsync())
                .Select(e => e.Trim())
                .Where(e => e.Contains('@'))
                .GroupBy(e => e.ToLower())
                .Select(g => g.First())
                .Where(e => !existing.Contains(e.ToLower()))
                .ToList();

            foreach (var email in emails)
            {
                var (prenom, nom) = GuessName(email);
                _context.AssistantsSociaux.Add(new AssistantSocial { Email = email, Prenom = prenom, Nom = nom, Actif = true });
            }
            await _context.SaveChangesAsync();

            return Ok(new { imported = emails.Count });
        }

        private Task<bool> EmailTaken(string email, int? exceptId)
        {
            var lower = email.ToLower();
            return _context.AssistantsSociaux.AnyAsync(a => a.Email.ToLower() == lower && a.AssistantSocialId != exceptId);
        }

        private static void Apply(AssistantSocial a, AssistantSocialDto dto)
        {
            a.Prenom = dto.Prenom?.Trim();
            a.Nom = dto.Nom?.Trim();
            a.Email = (dto.Email ?? "").Trim();
            a.Telephone = dto.Telephone?.Trim();
            a.Organisme = dto.Organisme?.Trim();
            a.Remarques = dto.Remarques;
            a.Actif = dto.Actif;
        }

        private static (string? Prenom, string? Nom) GuessName(string email)
        {
            var local = email.Split('@')[0];
            var parts = local.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) return (null, null);

            var ti = CultureInfo.GetCultureInfo("fr-CH").TextInfo;
            string Cap(string s) => ti.ToTitleCase(s.Replace('_', ' ').ToLower());
            return (Cap(parts[0]), Cap(string.Join(" ", parts.Skip(1))));
        }
    }

    public class AssistantSocialDto
    {
        public string? Prenom { get; set; }
        public string? Nom { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string? Organisme { get; set; }
        public string? Remarques { get; set; }
        public bool Actif { get; set; } = true;
    }
}
