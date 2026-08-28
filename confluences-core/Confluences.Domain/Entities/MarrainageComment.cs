using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class MarrainageComment
    {
        [Key]
        public int MarrainageCommentId { get; set; }

        public int MarrainageId { get; set; }
        [ForeignKey(nameof(MarrainageId))]
        public virtual Marrainage? Marrainage { get; set; }

        public string Text { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? AuthorId { get; set; }
        [ForeignKey(nameof(AuthorId))]
        public virtual ApplicationUser? Author { get; set; }
    }
}
