using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class TypeMoyen
    {
        [Key]
        public int TypeMoyenId { get; set; }

        [Required]
        [StringLength(20)]
        public string Libelle { get; set; } = string.Empty;

        [InverseProperty("TypeMoyen")]
        public virtual ICollection<Entreprise>? Entreprise { get; set; }
    }
}
