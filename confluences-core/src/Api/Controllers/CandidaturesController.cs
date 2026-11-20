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
                .Include(c => c.StatutChanges!).ThenInclude(h => h.ChangedByUser)
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
                .Include(x => x.StatutChanges!).ThenInclude(h => h.ChangedByUser)
                .SingleOrDefaultAsync(x => x.CandidatureId == id);
            if (c == null) return NotFound();

            return Ok(ToDto(c, includeInternal: true));
        }

        // POST: api/Candidatures
        [HttpPost]
        public async Task<ActionResult<object>> PostCandidature(CandidatureDto dto)
        {
            var invalide = Valider(dto);
            if (invalide != null) return BadRequest(invalide);

            var c = new Candidature { CreatedAt = DateTime.UtcNow };
            Apply(c, dto);
            var erreurStatut = await EnregistrerStatut(c, null, dto, valider: true);
            if (erreurStatut != null) return BadRequest(erreurStatut);
            _context.Candidatures.Add(c);
            await SynchroniserContratMaori(c);
            await _context.SaveChangesAsync();

            return Ok(new { c.CandidatureId });
        }

        // PUT: api/Candidatures/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCandidature(int id, CandidatureDto dto)
        {
            var invalide = Valider(dto);
            if (invalide != null) return BadRequest(invalide);

            var c = await _context.Candidatures.FindAsync(id);
            if (c == null) return NotFound();

            var ancienStatut = c.Statut;
            Apply(c, dto);
            var erreurStatut = await EnregistrerStatut(c, ancienStatut, dto, valider: true);
            if (erreurStatut != null) return BadRequest(erreurStatut);
            await SynchroniserContratMaori(c);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/Candidatures/import
        // Import d'un tableau (CSV) déjà mis en correspondance côté frontend. Chaque ligne crée une
        // candidature, ou complète celle indiquée (CandidatureId) sans effacer ce qui est déjà saisi :
        // seules les valeurs présentes dans le fichier sont reprises. L'AS est retrouvé·e par e-mail
        // (créé·e dans le carnet si inconnu·e) ; la remarque AS devient une remarque datée du fil.
        // Tout ou rien : une erreur annule l'import entier.
        [HttpPost("import")]
        public async Task<ActionResult<object>> Import(List<CandidatureImportRow> rows)
        {
            if (rows == null || rows.Count == 0) return BadRequest("Aucune ligne à importer.");
            var bad = rows.FindIndex(r => r.CandidatureId == null && string.IsNullOrWhiteSpace(r.Data?.Nom));
            if (bad >= 0) return BadRequest($"Ligne {bad + 1} : le nom est requis.");

            var authorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var now = DateTime.UtcNow;
            var assistants = await _context.AssistantsSociaux.ToListAsync();
            int created = 0, updated = 0, asCreated = 0;

            await using var tx = await _context.Database.BeginTransactionAsync();
            foreach (var row in rows)
            {
                var dto = row.Data ?? new CandidatureDto();

                // Assistant·e social·e
                var asEmail = row.AsEmail?.Trim();
                if (!string.IsNullOrEmpty(asEmail))
                {
                    var a = assistants.FirstOrDefault(x => string.Equals(x.Email, asEmail, StringComparison.OrdinalIgnoreCase));
                    if (a == null)
                    {
                        a = new AssistantSocial
                        {
                            Email = asEmail, Prenom = row.AsPrenom?.Trim(), Nom = row.AsNom?.Trim(),
                            Telephone = row.AsTelephone?.Trim(), Organisme = dto.Mandant,
                        };
                        _context.AssistantsSociaux.Add(a);
                        await _context.SaveChangesAsync();
                        assistants.Add(a);
                        asCreated++;
                    }
                    dto.AssistantSocialId = a.AssistantSocialId;
                }

                Candidature c;
                if (row.CandidatureId.HasValue)
                {
                    c = await _context.Candidatures.FindAsync(row.CandidatureId.Value)
                        ?? throw new InvalidOperationException($"Candidature {row.CandidatureId} introuvable.");
                    var ancienStatut = c.Statut;
                    ApplyNonEmpty(c, dto);
                    // Import : statut daté et signé, sans exiger de motif (le fichier n'en a pas).
                    await EnregistrerStatut(c, ancienStatut, dto, valider: false);
                    updated++;
                }
                else
                {
                    c = new Candidature { CreatedAt = now };
                    Apply(c, dto);
                    await EnregistrerStatut(c, null, dto, valider: false);
                    _context.Candidatures.Add(c);
                    created++;
                }
                await SynchroniserContratMaori(c);
                c.UpdatedAt = now;
                await _context.SaveChangesAsync();

                var remarque = row.RemarqueAS?.Trim();
                if (!string.IsNullOrEmpty(remarque))
                {
                    _context.CandidatureComments.Add(new CandidatureComment
                    {
                        CandidatureId = c.CandidatureId, Text = remarque, AuthorId = authorId, CreatedAt = now,
                    });
                }
            }
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return Ok(new { created, updated, asCreated });
        }

        // Règles communes à la création et à la modification.
        private static string? Valider(CandidatureDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nom)) return "Le nom est requis.";
            if (!string.IsNullOrWhiteSpace(dto.EvaluationNiveau))
            {
                if (dto.DateEvaluation == null)
                    return "La date d'évaluation est obligatoire pour enregistrer une évaluation.";
                if (string.IsNullOrWhiteSpace(dto.Prenom))
                    return "Le prénom est obligatoire pour enregistrer une évaluation.";
            }
            return null;
        }

        // Date et auteur·e de chaque changement de statut. Pour « AS à contacter » et « Attente
        // du retour AS », le motif (et selon lui un commentaire) est obligatoire ; un nouveau motif
        // peut aussi être saisi sans changer de statut (nouveau contact avec l'AS).
        // Renvoie un message d'erreur, ou null.
        private async Task<string?> EnregistrerStatut(Candidature c, string? ancienStatut, CandidatureDto dto, bool valider)
        {
            var requiertMotif = MotifsContactAS.RequiertMotif(c.Statut);
            var change = (c.Statut ?? "") != (ancienStatut ?? "");
            var nouveauMotif = requiertMotif && !string.IsNullOrWhiteSpace(dto.MotifStatut);
            if (!change && !nouveauMotif) return null;

            var motif = requiertMotif ? dto.MotifStatut?.Trim() : null;
            if (string.IsNullOrEmpty(motif)) motif = null;
            var commentaire = string.IsNullOrWhiteSpace(dto.CommentaireStatut) ? null : dto.CommentaireStatut.Trim();

            if (valider && requiertMotif)
            {
                var precedents = c.CandidatureId == 0 ? 0 : await _context.CandidatureStatutChanges
                    .CountAsync(h => h.CandidatureId == c.CandidatureId && h.Motif != null && MotifsContactAS.Joignabilite.Contains(h.Motif));
                var erreur = MotifsContactAS.Verifier(motif, commentaire, precedents);
                if (erreur != null) return erreur;
            }

            _context.CandidatureStatutChanges.Add(new CandidatureStatutChange
            {
                Candidature = c,
                AncienStatut = ancienStatut,
                Statut = c.Statut,
                Motif = motif,
                Commentaire = commentaire,
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            });
            return null;
        }

        // Contrat Maori reçu pour la candidature : reporté sur la fiche du/de la participant·e liée,
        // aussi pour les personnes déjà inscrites (avant, seulement à l'inscription dans une session).
        private async Task SynchroniserContratMaori(Candidature c)
        {
            if (!c.ContratMaori || string.IsNullOrEmpty(c.ParticipantId)) return;
            var u = await _context.Users.FindAsync(c.ParticipantId);
            if (u != null && !u.ContratMaori) u.ContratMaori = true;
        }

        private static string? NomAuteur(ApplicationUser? u)
        {
            if (u == null) return null;
            var nom = $"{u.Firstname} {u.LastName}".Trim();
            return nom.Length > 0 ? nom : u.UserName;
        }

        // Dernière saisie du statut actuel (changement ou nouveau contact AS).
        private static CandidatureStatutChange? DernierChangement(Candidature c) => c.StatutChanges?
            .Where(h => h.Statut == c.Statut)
            .OrderByDescending(h => h.ChangedAt)
            .FirstOrDefault();

        // Sessions visées envoyées : la liste, ou à défaut l'ancien couple année + numéro.
        private static List<string> SessionsDuDto(CandidatureDto dto)
        {
            if (dto.SessionsVisees != null) return dto.SessionsVisees;
            return dto.AnneeVisee.HasValue && dto.SessionNumberVisee.HasValue
                ? new List<string> { $"{dto.AnneeVisee}-{dto.SessionNumberVisee}" }
                : new List<string>();
        }

        // Normalise ("2027-1"), dédoublonne et trie les sessions ; la première alimente
        // AnneeVisee/SessionNumberVisee.
        private static void SetSessionsVisees(Candidature c, IEnumerable<string> keys)
        {
            var sessions = keys
                .Select(k => (k ?? "").Trim().Split('-'))
                .Where(p => p.Length == 2 && int.TryParse(p[0], out _) && int.TryParse(p[1], out _))
                .Select(p => (Annee: int.Parse(p[0]), Numero: int.Parse(p[1])))
                .Distinct()
                .OrderBy(s => s.Annee).ThenBy(s => s.Numero)
                .ToList();
            c.SessionsVisees = sessions.Select(s => $"{s.Annee}-{s.Numero}").ToList();
            c.AnneeVisee = sessions.Count > 0 ? sessions[0].Annee : null;
            c.SessionNumberVisee = sessions.Count > 0 ? sessions[0].Numero : null;
        }

        // Complète une candidature existante avec les seules valeurs renseignées dans l'import.
        private static void ApplyNonEmpty(Candidature c, CandidatureDto dto)
        {
            static bool Has(string? s) => !string.IsNullOrWhiteSpace(s);
            if (dto.DateDemande.HasValue) c.DateDemande = dto.DateDemande;
            if (Has(dto.Statut)) c.Statut = dto.Statut;
            if (Has(dto.MesureVisee)) c.MesureVisee = dto.MesureVisee;
            var sessions = SessionsDuDto(dto);
            if (sessions.Count > 0) SetSessionsVisees(c, c.SessionsVisees.Concat(sessions));
            if (dto.Priorite.HasValue) c.Priorite = dto.Priorite;
            if (Has(dto.Civilite)) c.Civilite = dto.Civilite;
            if (Has(dto.Prenom)) c.Prenom = dto.Prenom!.Trim();
            if (Has(dto.Nom)) c.Nom = dto.Nom!.Trim();
            if (dto.DateNaissance.HasValue) c.DateNaissance = dto.DateNaissance;
            if (Has(dto.Adresse)) c.Adresse = dto.Adresse;
            if (Has(dto.Telephone)) c.Telephone = dto.Telephone!.Trim();
            if (Has(dto.Email)) c.Email = dto.Email!.Trim();
            if (Has(dto.Origine)) c.Origine = dto.Origine!.Trim();
            if (Has(dto.Mandant)) c.Mandant = dto.Mandant;
            if (dto.AssistantSocialId.HasValue) c.AssistantSocialId = dto.AssistantSocialId;
            if (Has(dto.NotesRdv)) c.NotesRdv = dto.NotesRdv;
            if (dto.DateEvaluation.HasValue) c.DateEvaluation = dto.DateEvaluation;
            if (Has(dto.EvaluationNiveau)) c.EvaluationNiveau = dto.EvaluationNiveau;
            if (Has(dto.RemarquesGenerales))
                c.RemarquesGenerales = Has(c.RemarquesGenerales) && !c.RemarquesGenerales!.Contains(dto.RemarquesGenerales!.Trim())
                    ? $"{c.RemarquesGenerales}\n{dto.RemarquesGenerales!.Trim()}" : dto.RemarquesGenerales;
            if (dto.InscriptionConfirmee) c.InscriptionConfirmee = true;
            if (dto.ContratMaori) c.ContratMaori = true;
            if (Has(dto.TransmissionAS)) c.TransmissionAS = dto.TransmissionAS;
            if (Has(dto.ParticipantId)) c.ParticipantId = dto.ParticipantId;
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
            sessionsVisees = c.SessionsVisees,
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
            // Date (et auteur·e) de la saisie du statut actuel : pour « Attente du retour AS »,
            // « En attente A1/A2 »… c'est la date du contact avec l'AS. Null pour les candidatures
            // dont le statut date d'avant l'historique des statuts.
            statutDepuis = DernierChangement(c)?.ChangedAt,
            statutPar = NomAuteur(DernierChangement(c)?.ChangedByUser),
            motifStatut = DernierChangement(c)?.Motif,
            commentaireStatut = DernierChangement(c)?.Commentaire,
            historiqueStatuts = !includeInternal ? null : c.StatutChanges?
                .OrderByDescending(h => h.ChangedAt)
                .Select(h => new
                {
                    h.CandidatureStatutChangeId,
                    h.AncienStatut,
                    h.Statut,
                    h.Motif,
                    h.Commentaire,
                    h.ChangedAt,
                    par = NomAuteur(h.ChangedByUser),
                }),
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
            SetSessionsVisees(c, SessionsDuDto(dto));
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
        public List<string>? SessionsVisees { get; set; }
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

        // Saisie du statut : non stockés sur la candidature, vont dans l'historique des statuts.
        public string? MotifStatut { get; set; }
        public string? CommentaireStatut { get; set; }
    }

    public class CandidatureImportRow
    {
        // Renseigné : complète cette candidature au lieu d'en créer une nouvelle.
        public int? CandidatureId { get; set; }
        public CandidatureDto? Data { get; set; }
        public string? RemarqueAS { get; set; }
        public string? AsEmail { get; set; }
        public string? AsPrenom { get; set; }
        public string? AsNom { get; set; }
        public string? AsTelephone { get; set; }
    }

    public class CandidatureCommentDto
    {
        public string? Text { get; set; }
        public string? Kind { get; set; }
    }
}
