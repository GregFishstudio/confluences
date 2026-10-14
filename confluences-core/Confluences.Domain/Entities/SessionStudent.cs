using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class SessionStudent
    {
        [DisplayName("Session")]
        public int SessionId { get; set; }
        [ForeignKey(nameof(SessionId))]
        public virtual Session? Session { get; set; }

        [DisplayName("Participant-e")]
        public string? StudentId { get; set; }
        [ForeignKey(nameof(StudentId))]
        public virtual ApplicationUser? Student { get; set; }

        // Taux de présence (%) mis en cache — recalculé à la demande (voir SessionStudentsController)
        // plutôt qu'à chaque affichage, pour éviter de ré-agréger toutes les présences en continu.
        public double? TauxParticipation { get; set; }
        public System.DateTime? TauxParticipationCalculeLe { get; set; }

        // --- Suivi des inscriptions (renouvellement / interruption / non-renouvellement) ---
        // Propre à cette session : les données durables (AS, mandant, Maori, niveaux) restent
        // sur la fiche participant (ApplicationUser).
        // "Nouveau-elle", "Renouvelé-e 1", "Renouvelé-e 2", ..., "De A1 - renouvelé-e 1", "Interrompu"
        public string? StatutBeneficiaire { get; set; }
        // "oui", "partiellement", "non"
        public string? ObjectifsAtteints { get; set; }
        public string? ObjectifsCommentaire { get; set; }
        // "Renouv. de la MIS", "MIS RLS", "Autre (précisez)", "Sans solution", ...
        public string? OrientationPreconisee { get; set; }
        public string? OrientationPrecisions { get; set; }
        public string? CommunicationAS { get; set; }
    }
}
