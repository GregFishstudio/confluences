using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
    public class CandidaturesController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public CandidaturesController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/Candidatures
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetCandidatures()
        {
            var list = await _context.Candidatures.AsNoTracking()
                .Include(c => c.AssistantSocial)
                .Include(c => c.Participant)
                .Include(c => c.Comments)
                .OrderByDescending(c => c.DateDemande)
                .ToListAsync();

            // Les remarques internes ("pour nous") ne sortent pas dans la liste : seulement dans
            // le formulaire d'une candidature (GET api/Candidatures/5).
            return Ok(list.Select(c => ToDto(c, includeInternal: false)));
        }

        // GET: api/Candidatures/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetCandidature(int id)
        {
            var c = await _context.Candidatures.AsNoTracking()
                .Include(x => x.AssistantSocial)
                .Include(x => x.Participant)
                .SingleOrDefaultAsync(x => x.CandidatureId == id);
            if (c == null) return NotFound();

            return Ok(ToDto(c, includeInternal: true));
        }

        // POST: api/Candidatures
        [HttpPost]
        public async Task<ActionResult<object>> PostCandidature(CandidatureDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nom)) return BadRequest("Le nom est requis.");
            if (!string.IsNullOrWhiteSpace(dto.EvaluationNiveau) && dto.DateEvaluation == null)
                return BadRequest("La date d'évaluation est obligatoire pour enregistrer une évaluation.");

            var c = new Candidature { CreatedAt = DateTime.UtcNow };
            Apply(c, dto);
            _context.Candidatures.Add(c);
            await _context.SaveChangesAsync();

            return Ok(new { c.CandidatureId });
        }

        // PUT: api/Candidatures/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCandidature(int id, CandidatureDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nom)) return BadRequest("Le nom est requis.");
            if (!string.IsNullOrWhiteSpace(dto.EvaluationNiveau) && dto.DateEvaluation == null)
                return BadRequest("La date d'évaluation est obligatoire pour enregistrer une évaluation.");

            var c = await _context.Candidatures.FindAsync(id);
            if (c == null) return NotFound();

            Apply(c, dto);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Candidatures/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCandidature(int id)
        {
            var c = await _context.Candidatures.FindAsync(id);
            if (c == null) return NotFound();

            _context.Candidatures.Remove(c);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Fils de remarques d'une candidature (cf. CandidatureComment.Kind) ; "as" ou vide =
        // fil historique "Remarques AS" (Kind null en base).
        public const string KindAsAContacter = "as_a_contacter";
        public const string KindTransmission = "transmission";
        private static string? NormalizeKind(string? kind) =>
            string.IsNullOrWhiteSpace(kind) || kind == "as" ? null : kind.Trim();

        // GET: api/Candidatures/5/comments?kind=transmission — le plus récent en premier.
        [HttpGet("{id}/comments")]
        public async Task<ActionResult<IEnumerable<object>>> GetComments(int id, [FromQuery] string? kind)
        {
            var k = NormalizeKind(kind);
            var comments = await _context.CandidatureComments.AsNoTracking()
                .Include(c => c.Author)
                .Where(c => c.CandidatureId == id && c.Kind == k)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return Ok(comments.Select(CommentDto));
        }

        // POST: api/Candidatures/5/comments
        [HttpPost("{id}/comments")]
        public async Task<ActionResult<object>> PostComment(int id, CandidatureCommentDto dto)
        {
            var text = (dto.Text ?? "").Trim();
            if (string.IsNullOrEmpty(text)) return BadRequest("La remarque est vide.");
            if (!await _context.Candidatures.AnyAsync(c => c.CandidatureId == id)) return NotFound();

            var comment = new CandidatureComment
            {
                CandidatureId = id,
                Text = text,
                Kind = NormalizeKind(dto.Kind),
                AuthorId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                CreatedAt = DateTime.UtcNow,
            };
            _context.CandidatureComments.Add(comment);
            await _context.SaveChangesAsync();
            await _context.Entry(comment).Reference(c => c.Author).LoadAsync();

            return Ok(CommentDto(comment));
        }

        // DELETE: api/Candidatures/comments/12 — seulement par son auteur·e (comme les marrainages).
        [HttpDelete("comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var comment = await _context.CandidatureComments.FindAsync(commentId);
            if (comment == null) return NotFound();
            if (comment.AuthorId != User.FindFirstValue(ClaimTypes.NameIdentifier)) return Forbid();

            _context.CandidatureComments.Remove(comment);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Auteur·e réduit·e à son nom (pas l'entité ApplicationUser complète dans le JSON).
        private static object CommentDto(CandidatureComment c) => new
        {
            c.CandidatureCommentId,
            c.Text,
            c.CreatedAt,
            c.AuthorId,
            c.Kind,
            author = c.Author == null ? null : new { c.Author.Firstname, c.Author.LastName, c.Author.UserName },
        };

        private static object ToDto(Candidature c, bool includeInternal) => new
        {
            c.CandidatureId,
            c.DateDemande,
            c.Statut,
            c.MesureVisee,
            c.AnneeVisee,
            c.SessionNumberVisee,
            c.Priorite,
            c.Civilite,
            c.Prenom,
            c.Nom,
            c.DateNaissance,
            c.Adresse,
            c.Telephone,
            c.Email,
            c.Origine,
            c.Mandant,
            c.AssistantSocialId,
            assistantSocial = c.AssistantSocial == null ? null : new
            {
                c.AssistantSocial.AssistantSocialId,
                c.AssistantSocial.Prenom,
                c.AssistantSocial.Nom,
                c.AssistantSocial.Email,
            },
            nbRemarquesAS = c.Comments?.Count(x => x.Kind == null) ?? 0,
            derniereRemarqueAS = c.Comments?.Where(x => x.Kind == null).OrderByDescending(x => x.CreatedAt).Select(x => x.Text).FirstOrDefault(),
            derniereRemarqueAsAContacter = LastComment(c, KindAsAContacter),
            derniereTransmission = LastComment(c, KindTransmission),
            nbTransmissions = c.Comments?.Count(x => x.Kind == KindTransmission) ?? 0,
            c.NotesRdv,
            c.DateEvaluation,
            c.EvaluationNiveau,
            remarquesPedagogiques = includeInternal ? c.RemarquesPedagogiques : null,
            c.RemarquesGenerales,
            c.InscriptionConfirmee,
            c.ContratMaori,
            c.TransmissionAS,
            c.ParticipantId,
            participantNom = c.Participant == null ? null : $"{c.Participant.Firstname} {c.Participant.LastName}",
            c.CreatedAt,
            c.UpdatedAt,
        };

        private static object? LastComment(Candidature c, string kind) => c.Comments?
            .Where(x => x.Kind == kind)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new { x.Text, x.CreatedAt })
            .FirstOrDefault();

        private static void Apply(Candidature c, CandidatureDto dto)
        {
            c.DateDemande = dto.DateDemande;
            c.Statut = dto.Statut;
            c.MesureVisee = dto.MesureVisee;
            c.AnneeVisee = dto.AnneeVisee;
            c.SessionNumberVisee = dto.SessionNumberVisee;
            c.Priorite = dto.Priorite;
            c.Civilite = dto.Civilite;
            c.Prenom = dto.Prenom?.Trim();
            c.Nom = (dto.Nom ?? "").Trim();
            c.DateNaissance = dto.DateNaissance;
            c.Adresse = dto.Adresse;
            c.Telephone = dto.Telephone?.Trim();
            c.Email = dto.Email?.Trim();
            c.Origine = dto.Origine?.Trim();
            c.Mandant = dto.Mandant;
            c.AssistantSocialId = dto.AssistantSocialId;
            c.NotesRdv = dto.NotesRdv;
            c.DateEvaluation = dto.DateEvaluation;
            c.EvaluationNiveau = dto.EvaluationNiveau;
            c.RemarquesPedagogiques = dto.RemarquesPedagogiques;
            c.RemarquesGenerales = dto.RemarquesGenerales;
            c.InscriptionConfirmee = dto.InscriptionConfirmee;
            c.ContratMaori = dto.ContratMaori;
            c.TransmissionAS = dto.TransmissionAS;
            c.ParticipantId = string.IsNullOrEmpty(dto.ParticipantId) ? null : dto.ParticipantId;
            c.UpdatedAt = DateTime.UtcNow;
        }
    }

    public class CandidatureDto
    {
        public DateTime? DateDemande { get; set; }
        public string? Statut { get; set; }
        public string? MesureVisee { get; set; }
        public int? AnneeVisee { get; set; }
        public int? SessionNumberVisee { get; set; }
        public int? Priorite { get; set; }
        public string? Civilite { get; set; }
        public string? Prenom { get; set; }
        public string? Nom { get; set; }
        public DateTime? DateNaissance { get; set; }
        public string? Adresse { get; set; }
        public string? Telephone { get; set; }
        public string? Email { get; set; }
        public string? Origine { get; set; }
        public string? Mandant { get; set; }
        public int? AssistantSocialId { get; set; }
        public string? NotesRdv { get; set; }
        public DateTime? DateEvaluation { get; set; }
        public string? EvaluationNiveau { get; set; }
        public string? RemarquesPedagogiques { get; set; }
        public string? RemarquesGenerales { get; set; }
        public bool InscriptionConfirmee { get; set; }
        public bool ContratMaori { get; set; }
        public string? TransmissionAS { get; set; }
        public string? ParticipantId { get; set; }
    }

    public class CandidatureCommentDto
    {
        public string? Text { get; set; }
        public string? Kind { get; set; }
    }
}
