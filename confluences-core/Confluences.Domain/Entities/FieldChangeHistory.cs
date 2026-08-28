using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    // Historique ciblé de modifications de champ — volontairement générique (EntityType +
    // EntityId + FieldName) pour couvrir plusieurs contextes (assistant·e social·e, commentaires
    // du suivi formatrices, ...) sans une table dédiée par champ, mais n'est écrit que pour les
    // quelques champs explicitement suivis (pas un audit générique de tout le profil).
    public class FieldChangeHistory
    {
        [Key]
        public int FieldChangeHistoryId { get; set; }

        [Required]
        [StringLength(60)]
        public string EntityType { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string EntityId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FieldName { get; set; } = string.Empty;

        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        public string? ChangedByUserId { get; set; }
        [ForeignKey(nameof(ChangedByUserId))]
        public virtual ApplicationUser? ChangedByUser { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
