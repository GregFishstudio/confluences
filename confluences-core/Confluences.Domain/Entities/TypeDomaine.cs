using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class TypeDomaine
    {
        [Key]
        public int TypeDomaineId { get; set; }

        [StringLength(60)]
        public string? Libelle { get; set; }

        [StringLength(300)]
        public string? OldNames { get; set; }

        [InverseProperty(nameof(EntrepriseDomaine.TypeDomaine))]
        public virtual ICollection<EntrepriseDomaine>? EntrepriseDomaines { get; set; }
    }
}
