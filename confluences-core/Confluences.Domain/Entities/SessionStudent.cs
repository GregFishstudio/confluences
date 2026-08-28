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
    }
}
