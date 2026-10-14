using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    // Commentaire daté et signé d'une rubrique du "Suivi formatrices" de la fiche participant·e
    // (parcours en Suisse, formation, besoins : français, logement, sport...). Chaque ajout est
    // une nouvelle entrée : l'historique est conservé, une entrée erronée peut être supprimée.
    public class ParticipantComment
    {
        [Key]
        public int ParticipantCommentId { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;
        [ForeignKey(nameof(StudentId))]
        public virtual ApplicationUser? Student { get; set; }

        // Rubrique : "parcours_suisse", "formation_origine", "activite_origine", "activite_suisse",
        // "francais", "logement", "stages", "benevolat", "emploi", "formation", "sport",
        // "marrainage", "autre".
        [Required]
        [StringLength(40)]
        public string Rubrique { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Auteur·e : id + nom figé à l'écriture (les commentaires repris des anciens champs
        // n'ont que le nom de la dernière personne ayant modifié la fiche, voire rien).
        public string? AuthorId { get; set; }
        [StringLength(200)]
        public string? AuthorName { get; set; }
    }
}
