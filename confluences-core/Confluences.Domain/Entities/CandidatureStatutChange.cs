using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    // Un changement de statut d'une candidature : quand, par qui, et pour « AS à contacter » /
    // « Attente du retour AS » le motif du contact et son commentaire. Sert aussi à afficher la
    // date du contact AS (date de la saisie) dans la liste.
    public class CandidatureStatutChange
    {
        [Key]
        public int CandidatureStatutChangeId { get; set; }

        public int CandidatureId { get; set; }
        [ForeignKey(nameof(CandidatureId))]
        public virtual Candidature? Candidature { get; set; }

        [StringLength(50)]
        public string? AncienStatut { get; set; }
        [StringLength(50)]
        public string? Statut { get; set; }

        [StringLength(80)]
        public string? Motif { get; set; }
        public string? Commentaire { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        public string? ChangedByUserId { get; set; }
        [ForeignKey(nameof(ChangedByUserId))]
        public virtual ApplicationUser? ChangedByUser { get; set; }
    }
}
