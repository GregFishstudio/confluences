using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class TypeAffiliation
    {
        [Key]
        public int TypeAffiliationId { get; set; }

        [StringLength(10)]
        public string? Code { get; set; }

        [StringLength(50)]
        public string? Libelle { get; set; }

        [InverseProperty(nameof(ApplicationUser.TypeAffiliation))]
        public virtual ICollection<ApplicationUser>? Users { get; set; }
    }
}
