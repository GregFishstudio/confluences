using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    // Représente une relation de marrainage/parrainage entre une marraine/un parrain
    // (Marrainage) et un·e participant·e donné·e, avec ses propres dates et statut —
    // une même marraine peut avoir plusieurs relations simultanées, chacune suivie séparément.
    public class MarrainageParticipant
    {
        public int MarrainageId { get; set; }
        [ForeignKey(nameof(MarrainageId))]
        public virtual Marrainage? Marrainage { get; set; }

        public string ParticipantId { get; set; } = string.Empty;
        [ForeignKey(nameof(ParticipantId))]
        public virtual ApplicationUser? Participant { get; set; }

        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public bool Actif { get; set; } = true;
        public string? RaisonInactivite { get; set; }
        // Suivi libre de la relation : comment elle se passe, pourquoi/comment elle a été interrompue.
        public string? Commentaire { get; set; }
    }
}
