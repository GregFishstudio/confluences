using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class TypeEntreprise
    {
        [Key]
        public int TypeEntrepriseId { get; set; }

        [Required]
        [StringLength(50)]
        public string Nom { get; set; } = string.Empty;

        [InverseProperty("TypeEntreprise")]
        public virtual ICollection<Entreprise>? Entreprise { get; set; }
    }
}
