using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class Theory
    {
        [Key]
        public int TheoryId { get; set; }

        [Required]
        public DateTime TheoryDate { get; set; }
        public string? TheoryName { get; set; }
        public string? TheoryLink { get; set; }

        public string? AudioLink { get; set; }
        public string? AudioLinkSecond { get; set; }
        public string? AudioLinkThird { get; set; }
        public string? VideoLink { get; set; }

        [Required]
        public string TeacherId { get; set; } = string.Empty;
        [ForeignKey(nameof(TeacherId))]
        public virtual ApplicationUser? Teacher { get; set; }

        [Required]
        public int HomeworkV2Id { get; set; }
        [ForeignKey(nameof(HomeworkV2Id))]
        public virtual HomeworkV2? HomeworkV2 { get; set; }

        [InverseProperty(nameof(Exercice.Theory))]
        public virtual ICollection<Exercice>? Exercices { get; set; }
    }
}
