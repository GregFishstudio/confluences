using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class TypeAnnonce
    {
        [Key]
        public int TypeAnnonceId { get; set; }

        [StringLength(30)]
        public string? Libelle { get; set; }

        [InverseProperty(nameof(Stage.TypeAnnonce))]
        public virtual ICollection<Stage>? Stages { get; set; }
    }
}
