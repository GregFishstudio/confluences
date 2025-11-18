using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class HomeworkV2StudentExerciceAlone
    {
        [Key]
        public int HomeworkV2StudentId { get; set; }

        [Required]
        public DateTime HomeworkStudentDate { get; set; }

        [Required]
        public string HomeworkFile { get; set; } = string.Empty;
        public string? HomeworkFileName { get; set; }
        public string? HomeworkFileSize { get; set; }
        public string? HomeworkFileType { get; set; }

        public string? HomeworkFileTeacher { get; set; }
        public string? HomeworkCommentaryTeacher { get; set; }

        [Required]
        public int ExerciceId { get; set; }
        [ForeignKey(nameof(ExerciceId))]
        public virtual ExerciceAlone? ExerciceAlone { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;
        [ForeignKey(nameof(StudentId))]
        public virtual ApplicationUser? Student { get; set; }
    }
}
