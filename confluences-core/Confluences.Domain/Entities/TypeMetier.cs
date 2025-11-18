using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class TypeMetier
    {
        [Key]
        public int TypeMetierId { get; set; }

        public string? Libelle { get; set; }

        public string? OldNames { get; set; }

        [InverseProperty(nameof(EntrepriseMetier.TypeMetier))]
        public virtual ICollection<EntrepriseMetier>? EntrepriseMetiers { get; set; }
        [InverseProperty(nameof(Stage.TypeMetier))]
        public virtual ICollection<Stage>? Stages { get; set; }
    }
}
