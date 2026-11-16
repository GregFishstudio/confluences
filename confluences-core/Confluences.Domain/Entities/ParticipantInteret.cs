using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    // Métiers visés par un·e participant·e pour un stage ou du bénévolat — mêmes listes que les
    // métiers des entreprises (TypeMetier), pour proposer les entreprises correspondantes.
    public class ParticipantMetier
    {
        public string ParticipantId { get; set; } = string.Empty;
        [ForeignKey(nameof(ParticipantId))]
        public virtual ApplicationUser? Participant { get; set; }

        public int TypeMetierId { get; set; }
        [ForeignKey(nameof(TypeMetierId))]
        public virtual TypeMetier? TypeMetier { get; set; }
    }

    // Domaines d'intérêt (TypeDomaine : logistique, santé, social…) d'un·e participant·e.
    public class ParticipantDomaine
    {
        public string ParticipantId { get; set; } = string.Empty;
        [ForeignKey(nameof(ParticipantId))]
        public virtual ApplicationUser? Participant { get; set; }

        public int TypeDomaineId { get; set; }
        [ForeignKey(nameof(TypeDomaineId))]
        public virtual TypeDomaine? TypeDomaine { get; set; }
    }
}
