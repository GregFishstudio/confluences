using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class TypeOffre
    {
        [Key]
        public int TypeOffreId { get; set; }

        [StringLength(30)]
        public string? Libelle { get; set; }

        [InverseProperty(nameof(EntrepriseOffre.TypeOffre))]
        public virtual ICollection<EntrepriseOffre>? EntrepriseOffres { get; set; }
    }
}
