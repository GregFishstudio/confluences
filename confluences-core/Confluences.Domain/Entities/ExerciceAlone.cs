using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class ExerciceAlone
    {
        [Key]
        public int ExerciceId { get; set; }

        [Required]
        public DateTime ExerciceDate { get; set; }
        public string? ExerciceName { get; set; }
        public string? ExerciceLink { get; set; }

        public string? AudioLink { get; set; }
        public string? AudioLinkSecond { get; set; }
        public string? AudioLinkThird { get; set; }
        public string? VideoLink { get; set; }

        public DateTime CorrectionDate { get; set; }
        public string? CorrectionLink { get; set; }

        [DisplayName("Est un exercice supplémentaire")]
        public bool IsHomeworkAdditionnal { get; set; }

        [Required]
        public string TeacherId { get; set; } = string.Empty;
        [ForeignKey(nameof(TeacherId))]
        public virtual ApplicationUser? Teacher { get; set; }

        [Required]
        public int HomeworkV2Id { get; set; }
        [ForeignKey(nameof(HomeworkV2Id))]
        public virtual HomeworkV2? HomeworkV2 { get; set; }

        [InverseProperty("ExerciceAlone")]
        public virtual ICollection<HomeworkV2StudentExerciceAlone>? HomeworkV2StudentExerciceAlones { get; set; }

    }
}
