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
    // Suivi des inscriptions d'une session (renouvellements, interruptions, non-renouvellements,
    // nouveaux·elles candidat·e·s) — remplace le 2e tableau Excel.
    //
    // Une session = (année, numéro), comme dans les autres écrans : le numéro est celui de
    // Session.SessionNumberId, l'année celle de Session.DateStart ; les vraies dates viennent du
    // calendrier (SessionFormation), côté frontend. Le groupe (télécours / F&I A1 / F&I A2) est
    // déduit du nom de la classe, avec la même règle que FollowUpPage.vue (matchesGroup).
    [Authorize(Policy = "Teacher")]
    [Route("api/[controller]")]
    [ApiController]
    public class InscriptionsController : ControllerBase
    {
        private const string Retire = "Retiré-e";
        private const string PassageA2 = "Passage en A2";
        private const string Interrompu = "Interrompu";

        private readonly ConfluencesDbContext _context;

        public InscriptionsController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/Inscriptions/session?year=2026&number=3
        // Toutes classes confondues (le frontend répartit par groupe) :
        //  - inscrits       : participant·e·s rattaché·e·s à la session ;
        //  - renouvellements: participant·e·s de la session précédente orienté·e·s vers un
        //                     renouvellement (ou un passage en A2) et pas encore inscrit·e·s ici ;
        //  - candidats      : candidatures visant cette session, pas retirées, dont la personne
        //                     n'est pas déjà inscrite ici.
        [HttpGet("session")]
        public async Task<ActionResult<object>> GetSession([FromQuery] int year, [FromQuery] int number)
        {
            var keys = await _context.Sessions.AsNoTracking()
                .Select(s => new { Year = s.DateStart.Year, Number = s.SessionNumberId })
                .Distinct()
                .ToListAsync();
            var previous = keys
                .Where(k => k.Year < year || (k.Year == year && k.Number < number))
                .OrderByDescending(k => k.Year).ThenByDescending(k => k.Number)
                .FirstOrDefault();
            // Valeurs "à plat" pour la requête EF (pas de membre d'un objet potentiellement null).
            var prevYear = previous?.Year ?? -1;
            var prevNumber = previous?.Number ?? -1;

            var links = await _context.SessionStudents.AsNoTracking()
                .Include(ss => ss.Session).ThenInclude(s => s!.SchoolClassRoom)
                .Include(ss => ss.Student)
                .Where(ss => ss.Session != null && ss.Student != null
                          && ((ss.Session.SessionNumberId == number && ss.Session.DateStart.Year == year)
                              || (ss.Session.SessionNumberId == prevNumber && ss.Session.DateStart.Year == prevYear)))
                .ToListAsync();

            var inscrits = links.Where(l => l.Session!.SessionNumberId == number && l.Session.DateStart.Year == year).ToList();
            var inscritIds = inscrits.Select(l => l.StudentId!).ToHashSet();

            // Session en préparation (aucun·e inscrit·e dans un groupe) : les participant·e·s
            // actif·ve·s de la session précédente de ce groupe y sont reconduit·e·s par défaut
            // (renouvelé·e·s), sauf si leur orientation dit autre chose (MIS RLS, sans solution...).
            // Dès qu'un groupe a des inscrit·e·s, ce sont eux/elles qui font foi : personne n'est
            // ajouté (sinon les personnes parties après la session précédente gonfleraient le total).
            var groupesAvecInscrits = inscrits.Select(Groupe).ToHashSet();
            var precedents = previous == null ? new List<SessionStudent>() : links
                .Where(l => l.Session!.SessionNumberId == prevNumber && l.Session.DateStart.Year == prevYear)
                .Where(l => !groupesAvecInscrits.Contains(GroupeDestination(l)))
                .Where(l => !inscritIds.Contains(l.StudentId!))
                .Where(l => l.Student!.DateFinParticipation == null && l.StatutBeneficiaire != Interrompu)
                .GroupBy(l => l.StudentId).Select(g => g.First())
                .ToList();
            var renouvellements = precedents
                .Where(l => string.IsNullOrWhiteSpace(l.OrientationPreconisee) || IsRenouvellement(l.OrientationPreconisee))
                .ToList();

            // Nombre de sessions suivies avant la session affichée — aide à remplir "Renouvelé-e N".
            var ids = inscrits.Concat(precedents).Select(l => l.StudentId!).Distinct().ToList();
            var history = await _context.SessionStudents.AsNoTracking()
                .Where(ss => ids.Contains(ss.StudentId!))
                .Select(ss => new { ss.StudentId, Year = ss.Session!.DateStart.Year, Number = ss.Session.SessionNumberId })
                .ToListAsync();
            var previousCounts = history
                .Where(h => h.Year < year || (h.Year == year && h.Number < number))
                .GroupBy(h => h.StudentId!)
                .ToDictionary(g => g.Key, g => g.Select(h => (h.Year, h.Number)).Distinct().Count());

            // Niveaux de contrat d'après Bilans & attestations (un niveau oral/écrit par session,
            // celui atteint en fin de session) : début = niveau noté à la session suivie juste
            // avant celle-ci (vide pour un·e nouveau·elle), fin = niveau noté pour cette session.
            var bilans = (await _context.StagiaireNiveaux.AsNoTracking()
                    .Where(n => ids.Contains(n.StagiaireId))
                    .ToListAsync())
                .GroupBy(n => (n.StagiaireId, n.Annee, n.Trimestre))
                .ToDictionary(g => g.Key, g => FormatNiveau(g.Last().OralLevel, g.Last().WrittenLevel));
            var sessionPrecedente = history
                .Where(h => h.Year < year || (h.Year == year && h.Number < number))
                .GroupBy(h => h.StudentId!)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(h => h.Year).ThenByDescending(h => h.Number).First());
            NiveauxContrat niveaux(string studentId) => new(
                sessionPrecedente.TryGetValue(studentId, out var p) ? bilans.GetValueOrDefault((studentId, p.Year, p.Number)) : null,
                bilans.GetValueOrDefault((studentId, year, number)));

            // Candidatures qui visent cette session (parmi d'autres éventuellement).
            var sessionKey = $"{year}-{number}";
            var candidatures = await _context.Candidatures.AsNoTracking()
                .Include(c => c.AssistantSocial)
                .Include(c => c.StatutChanges)
                .Where(c => c.SessionsVisees.Contains(sessionKey) && c.Statut != Retire)
                .ToListAsync();
            var candidats = candidatures.Where(c => c.ParticipantId == null || !inscritIds.Contains(c.ParticipantId));

            return Ok(new
            {
                previous = previous == null ? null : new { year = previous.Year, number = previous.Number },
                inscrits = inscrits.Select(l => ToRow(l, Groupe(l), previousCounts, niveaux(l.StudentId!))),
                renouvellements = renouvellements.Select(l => ToRow(l, GroupeDestination(l), previousCounts, niveaux(l.StudentId!))),
                candidats = candidats.Select(c => new
                {
                    c.CandidatureId,
                    groupe = GroupeCandidature(c.MesureVisee, c.Statut),
                    c.MesureVisee,
                    c.ParticipantId,
                    c.Civilite,
                    c.Prenom,
                    c.Nom,
                    c.Origine,
                    c.Mandant,
                    assistantSocialEmail = c.AssistantSocial?.Email,
                    c.Statut,
                    // Date de la saisie du statut actuel (date du contact AS pour les listes d'attente).
                    statutDepuis = c.StatutChanges?.Where(h => h.Statut == c.Statut).Max(h => (DateTime?)h.ChangedAt),
                    c.EvaluationNiveau,
                    c.InscriptionConfirmee,
                    c.ContratMaori,
                }),
            });
        }

        // POST: api/Inscriptions/inscrire
        // Inscrit réellement des personnes dans une session : crée la session de la classe si elle
        // n'existe pas encore (même convention que les sessions existantes : début le 1er du mois
        // N de l'année, fin un mois plus tard — seuls l'année, le numéro et la classe comptent pour
        // la fiche de présence, Accomp. & Ateliers et Bilans), puis le lien participant·e/session.
        // Les personnes apparaissent ensuite dans ces pages pour la session.
        [HttpPost("inscrire")]
        public async Task<ActionResult<object>> Inscrire(InscriptionBatchDto dto)
        {
            if (dto.Number < 1 || dto.Number > 4) return BadRequest("Numéro de session invalide.");
            if (dto.Items == null || dto.Items.Count == 0) return BadRequest("Personne à inscrire.");

            var sessionsCreees = 0;
            var inscrits = 0;
            foreach (var item in dto.Items)
            {
                if (string.IsNullOrEmpty(item.StudentId)) continue;
                var user = await _context.Users.FindAsync(item.StudentId);
                if (user == null) return BadRequest("Participant·e introuvable.");
                if (!await _context.SchoolClassRooms.AnyAsync(c => c.SchoolClassRoomId == item.SchoolClassRoomId))
                    return BadRequest("Classe introuvable.");

                // Déjà inscrit·e dans cette session (quelle que soit la classe) : on ne double pas.
                var dejaInscrit = await _context.SessionStudents.AnyAsync(ss => ss.StudentId == item.StudentId
                    && ss.Session!.SessionNumberId == dto.Number && ss.Session.DateStart.Year == dto.Year);
                if (dejaInscrit) continue;

                var session = await _context.Sessions.FirstOrDefaultAsync(s =>
                    s.SessionNumberId == dto.Number && s.DateStart.Year == dto.Year && s.SchoolClassRoomId == item.SchoolClassRoomId);
                if (session == null)
                {
                    var start = new DateTime(dto.Year, dto.Number, 1);
                    session = new Session
                    {
                        DateStart = start,
                        DateEnd = start.AddMonths(1),
                        SessionNumberId = dto.Number,
                        SchoolClassRoomId = item.SchoolClassRoomId,
                    };
                    _context.Sessions.Add(session);
                    await _context.SaveChangesAsync();
                    sessionsCreees++;
                }

                // Statut de départ : d'après le nombre de sessions déjà suivies.
                var dejaSuivies = (await _context.SessionStudents
                    .Where(ss => ss.StudentId == item.StudentId)
                    .Select(ss => new { ss.Session!.DateStart.Year, ss.Session.SessionNumberId })
                    .ToListAsync())
                    .Where(x => x.Year < dto.Year || (x.Year == dto.Year && x.SessionNumberId < dto.Number))
                    .Distinct()
                    .Count();

                _context.SessionStudents.Add(new SessionStudent
                {
                    SessionId = session.SessionId,
                    StudentId = item.StudentId,
                    StatutBeneficiaire = dejaSuivies > 0 ? $"Renouvelé-e {dejaSuivies}" : "Nouveau-elle",
                });

                // Session courante affichée sur la fiche participant (si plus récente).
                if (user.CurrentSessionYear == null || dto.Year > user.CurrentSessionYear
                    || (dto.Year == user.CurrentSessionYear && dto.Number > (user.CurrentSessionNumber ?? 0)))
                {
                    user.CurrentSessionYear = dto.Year;
                    user.CurrentSessionNumber = dto.Number;
                }

                if (item.CandidatureId.HasValue)
                {
                    var candidature = await _context.Candidatures.FindAsync(item.CandidatureId.Value);
                    if (candidature != null)
                    {
                        if (candidature.Statut != "Inscrit-e")
                        {
                            // Changement de statut daté et signé, comme depuis le formulaire.
                            _context.CandidatureStatutChanges.Add(new CandidatureStatutChange
                            {
                                CandidatureId = candidature.CandidatureId,
                                AncienStatut = candidature.Statut,
                                Statut = "Inscrit-e",
                                ChangedAt = DateTime.UtcNow,
                                ChangedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                            });
                        }
                        candidature.Statut = "Inscrit-e";
                        candidature.InscriptionConfirmee = true;
                        if (candidature.ContratMaori) user.ContratMaori = true;
                        candidature.ParticipantId = item.StudentId;
                        candidature.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
                inscrits++;
            }

            return Ok(new { inscrits, sessionsCreees });
        }

        // DELETE: api/Inscriptions/participants/12/abc-123
        // Retire une inscription faite par erreur (les présences déjà saisies ne sont pas touchées).
        [HttpDelete("participants/{sessionId}/{studentId}")]
        public async Task<IActionResult> Desinscrire(int sessionId, string studentId)
        {
            var ss = await _context.SessionStudents.SingleOrDefaultAsync(x => x.SessionId == sessionId && x.StudentId == studentId);
            if (ss == null) return NotFound();

            _context.SessionStudents.Remove(ss);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static bool IsRenouvellement(string? orientation) =>
            orientation != null && (orientation.StartsWith("Renouv", StringComparison.OrdinalIgnoreCase) || orientation == PassageA2);

        private static string Groupe(SessionStudent l)
        {
            var name = (l.Session?.SchoolClassRoom?.SchoolClassRoomName ?? "").ToLowerInvariant();
            if (name.Contains("tele") || name.Contains("télé")) return "telecours";
            if (name.Contains("a2")) return "fi_a2";
            if (name.Contains("a1")) return "fi_a1";
            return "autre";
        }

        // Groupe dans lequel une personne reconduite arrive : A2 si "Passage en A2", sinon le même.
        private static string GroupeDestination(SessionStudent l) =>
            l.OrientationPreconisee == PassageA2 ? "fi_a2" : Groupe(l);

        // Groupe d'une candidature : Télécours, ou F&I A1 / A2 d'après le niveau du statut
        // ("En attente A2 F&I" → F&I A2 ; F&I sans niveau précisé → F&I A1).
        private static string GroupeCandidature(string? mesure, string? statut)
        {
            if (mesure == "Télécours") return "telecours";
            if (mesure == "F&I" || mesure == "A1" || mesure == "A2")
                return mesure == "A2" || System.Text.RegularExpressions.Regex.IsMatch(statut ?? "", @"A2") ? "fi_a2" : "fi_a1";
            return "autre";
        }

        private record NiveauxContrat(string? Debut, string? Fin);

        // "A2.1" si oral = écrit (ou un seul renseigné), sinon "Oral A2.1 / Écrit A1.2".
        private static string? FormatNiveau(string? oral, string? ecrit)
        {
            oral = string.IsNullOrWhiteSpace(oral) ? null : oral.Trim();
            ecrit = string.IsNullOrWhiteSpace(ecrit) ? null : ecrit.Trim();
            if (oral == null || ecrit == null || oral == ecrit) return oral ?? ecrit;
            return $"Oral {oral} / Écrit {ecrit}";
        }

        private static object ToRow(SessionStudent r, string groupe, Dictionary<string, int> previousCounts, NiveauxContrat niveaux) => new
        {
            sessionId = r.SessionId,
            studentId = r.StudentId,
            groupe,
            classe = r.Session!.SchoolClassRoom?.SchoolClassRoomName,
            prenom = r.Student!.Firstname,
            nom = r.Student.LastName,
            origine = r.Student.Nationality,
            mandant = r.Student.BeneficiaireDe,
            assistantSocialEmail = r.Student.AssistantSocialEmail,
            contratMaori = r.Student.ContratMaori,
            niveauDebutContrat = niveaux.Debut,
            niveauFinContrat = niveaux.Fin,
            dateFinParticipation = r.Student.DateFinParticipation,
            raisonInterruptionMesure = r.Student.RaisonInterruptionMesure,
            sessionsPrecedentes = previousCounts.GetValueOrDefault(r.StudentId!),
            r.StatutBeneficiaire,
            r.ObjectifsAtteints,
            r.ObjectifsCommentaire,
            r.OrientationPreconisee,
            r.OrientationPrecisions,
            r.CommunicationAS,
        };

        // PUT: api/Inscriptions/participants/12/abc-123
        // Met à jour la ligne de suivi de la session ET les champs durables de la fiche
        // participant (pas de double saisie entre les deux écrans).
        // PATCH: api/Inscriptions/participants/abc-123/contrat-maori  { contratMaori: true }
        // Case "Contrat Maori" du tableau des participant·e·s : seul endroit où il se coche
        // (la fiche participant·e l'affiche en lecture seule).
        [HttpPatch("participants/{studentId}/contrat-maori")]
        public async Task<IActionResult> PatchContratMaori(string studentId, [FromBody] ContratMaoriDto dto)
        {
            var u = await _context.Users.FindAsync(studentId);
            if (u == null) return NotFound();
            if (u.ContratMaori != dto.ContratMaori)
            {
                // Coché depuis le tableau d'une session ou depuis la fiche : daté et signé.
                _context.FieldChangeHistories.Add(new FieldChangeHistory
                {
                    EntityType = nameof(ApplicationUser),
                    EntityId = u.Id,
                    FieldName = nameof(ApplicationUser.ContratMaori),
                    OldValue = u.ContratMaori ? "reçu" : "non reçu",
                    NewValue = dto.ContratMaori ? "reçu" : "non reçu",
                    ChangedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    ChangedAt = DateTime.UtcNow,
                });
            }
            u.ContratMaori = dto.ContratMaori;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        public class ContratMaoriDto
        {
            public bool ContratMaori { get; set; }
        }

        [HttpPut("participants/{sessionId}/{studentId}")]
        public async Task<IActionResult> PutParticipant(int sessionId, string studentId, InscriptionParticipantDto dto)
        {
            var ss = await _context.SessionStudents
                .Include(x => x.Student)
                .SingleOrDefaultAsync(x => x.SessionId == sessionId && x.StudentId == studentId);
            if (ss == null || ss.Student == null) return NotFound();

            // Une interruption a toujours une date et une raison ; une date d'interruption
            // saisie fait passer le statut de la session à "Interrompu".
            var interrompu = dto.StatutBeneficiaire == Interrompu || dto.DateFinParticipation.HasValue;
            if (interrompu && (!dto.DateFinParticipation.HasValue || string.IsNullOrWhiteSpace(dto.RaisonInterruptionMesure)))
                return BadRequest("Pour une interruption, la date et la raison sont obligatoires.");
            if (dto.DateFinParticipation.HasValue) dto.StatutBeneficiaire = Interrompu;

            ss.StatutBeneficiaire = dto.StatutBeneficiaire;
            ss.ObjectifsAtteints = dto.ObjectifsAtteints;
            ss.ObjectifsCommentaire = dto.ObjectifsCommentaire;
            ss.OrientationPreconisee = dto.OrientationPreconisee;
            ss.OrientationPrecisions = dto.OrientationPrecisions;
            ss.CommunicationAS = dto.CommunicationAS;

            var u = ss.Student;
            var newAsEmail = string.IsNullOrWhiteSpace(dto.AssistantSocialEmail) ? null : dto.AssistantSocialEmail.Trim();
            if (u.AssistantSocialEmail != newAsEmail)
            {
                // Même historique que depuis la fiche participant (AspNetUsersController).
                _context.FieldChangeHistories.Add(new FieldChangeHistory
                {
                    EntityType = nameof(ApplicationUser),
                    EntityId = u.Id,
                    FieldName = nameof(ApplicationUser.AssistantSocialEmail),
                    OldValue = u.AssistantSocialEmail,
                    NewValue = newAsEmail,
                    ChangedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    ChangedAt = DateTime.UtcNow,
                });
                u.AssistantSocialEmail = newAsEmail;
            }

            u.Nationality = dto.Origine;
            u.BeneficiaireDe = dto.Mandant;
            u.ContratMaori = dto.ContratMaori;
            u.DateFinParticipation = dto.DateFinParticipation;
            u.RaisonInterruptionMesure = dto.RaisonInterruptionMesure;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class InscriptionParticipantDto
    {
        public string? StatutBeneficiaire { get; set; }
        public string? ObjectifsAtteints { get; set; }
        public string? ObjectifsCommentaire { get; set; }
        public string? OrientationPreconisee { get; set; }
        public string? OrientationPrecisions { get; set; }
        public string? CommunicationAS { get; set; }

        public string? Origine { get; set; }
        public string? Mandant { get; set; }
        public string? AssistantSocialEmail { get; set; }
        public bool ContratMaori { get; set; }
        public DateTime? DateFinParticipation { get; set; }
        public string? RaisonInterruptionMesure { get; set; }
    }

    public class InscriptionBatchDto
    {
        public int Year { get; set; }
        public int Number { get; set; }
        public List<InscriptionItemDto>? Items { get; set; }
    }

    public class InscriptionItemDto
    {
        public string StudentId { get; set; } = string.Empty;
        public int SchoolClassRoomId { get; set; }
        // Candidature d'origine : passe à "Inscrit-e" une fois la personne inscrite.
        public int? CandidatureId { get; set; }
    }
}
