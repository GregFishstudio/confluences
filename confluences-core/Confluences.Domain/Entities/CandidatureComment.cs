using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    // Remarque de/sur l'AS d'une candidature, horodatée et signée — fil de discussion qui
    // remplace la colonne "Remarques AS" de l'Excel (même principe que MarrainageComment).
    public class CandidatureComment
    {
        [Key]
        public int CandidatureCommentId { get; set; }

        public int CandidatureId { get; set; }
        [ForeignKey(nameof(CandidatureId))]
        public virtual Candidature? Candidature { get; set; }

        public string Text { get; set; } = string.Empty;

        // Fil auquel appartient la remarque : null = "Remarques AS" (fil historique),
        // "as_a_contacter" = remarques liées au statut "AS à contacter",
        // "transmission" = "Transmission AS / ML". Chaque remarque est datée et conservée.
        [StringLength(30)]
        public string? Kind { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? AuthorId { get; set; }
        [ForeignKey(nameof(AuthorId))]
        public virtual ApplicationUser? Author { get; set; }
    }
}
