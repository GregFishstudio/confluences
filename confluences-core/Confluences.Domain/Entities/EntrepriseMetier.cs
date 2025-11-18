using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class EntrepriseMetier
    {
        public int EntrepriseId { get; set; }
        [ForeignKey(nameof(EntrepriseId))]
        public virtual Entreprise? Entreprise { get; set; }

        public int TypeMetierId { get; set; }
        [ForeignKey(nameof(TypeMetierId))]
        public virtual TypeMetier? TypeMetier { get; set; }
    }
}
