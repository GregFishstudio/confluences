using System.ComponentModel.DataAnnotations;

namespace Confluences.Domain.Entities
{
    // Liste des raisons d'inactivité proposées pour un marrainage/parrainage ou une relation de
    // marrainage — éditable par les formatrices elles-mêmes (au lieu d'une liste figée dans le code).
    public class MarrainageInactivityReason
    {
        [Key]
        public int MarrainageInactivityReasonId { get; set; }

        [Required]
        [StringLength(200)]
        public string Label { get; set; } = string.Empty;
    }
}
