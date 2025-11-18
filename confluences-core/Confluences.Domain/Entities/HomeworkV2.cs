using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class HomeworkV2
    {
        [Key]
        public int HomeworkV2Id { get; set; }

        [Required]
        public DateTime HomeworkV2Date { get; set; }

        [Required]
        public string HomeworkV2Name { get; set; } = string.Empty;

        public bool IsFutur { get; set; }

        [Required]
        public int HomeworkTypeId { get; set; }
        [ForeignKey(nameof(HomeworkTypeId))]
        public virtual HomeworkType? HomeworkType { get; set; }

        [Required]
        public int SessionId { get; set; }
        [ForeignKey(nameof(SessionId))]
        public virtual Session? Session { get; set; }

        [Required]
        public string TeacherId { get; set; } = string.Empty;
        [ForeignKey(nameof(TeacherId))]
        public virtual ApplicationUser? Teacher { get; set; }

        [InverseProperty(nameof(ExerciceAlone.HomeworkV2))]
        public virtual ICollection<ExerciceAlone>? ExercicesAlones { get; set; }

        [InverseProperty(nameof(Theory.HomeworkV2))]
        public virtual ICollection<Theory>? Theories { get; set; }

    }
}
