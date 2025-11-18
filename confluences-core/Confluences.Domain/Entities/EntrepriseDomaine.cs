using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class EntrepriseDomaine
    {
        public int EntrepriseId { get; set; }
        [ForeignKey(nameof(EntrepriseId))]
        public virtual Entreprise? Entreprise { get; set; }

        public int TypeDomaineId { get; set; }
        [ForeignKey(nameof(TypeDomaineId))]
        public virtual TypeDomaine? TypeDomaine { get; set; }
    }
}
