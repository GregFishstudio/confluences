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

        // Fil du commentaire : null = commentaires généraux (fil historique),
        // "parrain" = ne concerne que le parrain / la marraine (pas la relation ni le binôme).
        [StringLength(20)]
        public string? Portee { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? AuthorId { get; set; }
        [ForeignKey(nameof(AuthorId))]
        public virtual ApplicationUser? Author { get; set; }
    }
}
