using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class TypeStage
    {
        [Key]
        public int TypeStageId { get; set; }
        [Required]
        [StringLength(50)]
        public string Nom { get; set; } = string.Empty;

        [InverseProperty(nameof(Stage.TypeStage))]
        public virtual ICollection<Stage>? Stages { get; set; }
    }
}
