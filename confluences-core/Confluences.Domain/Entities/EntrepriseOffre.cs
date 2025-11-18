using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class EntrepriseOffre
    {
        public int EntrepriseId { get; set; }
        [ForeignKey(nameof(EntrepriseId))]
        public virtual Entreprise? Entreprise { get; set; }

        public int TypeOffreId { get; set; }
        [ForeignKey(nameof(TypeOffreId))]
        public virtual TypeOffre? TypeOffre { get; set; }
    }
}
