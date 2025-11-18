using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class TypeIntershipActivity
    {
        [Key]
        public int TypeIntershipActivityId { get; set; }
        [Required]
        [StringLength(50)]
        public string Nom { get; set; } = string.Empty;

        [InverseProperty(nameof(Stage.TypeIntershipActivity))]
        public virtual ICollection<Stage>? Stages { get; set; }
    }
}
