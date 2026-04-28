using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Important pour [Column]

namespace Confluences.Domain.Entities
{
    public class AccompanyingPresence 
    {
        [Key]
        public int AccompanyingPresenceId { get; set; }
        
        // On utilise UserId dans le code pour la clarté avec Identity
        // Mais on dit à la DB de pointer vers l'ancienne colonne StagiaireId
        [Column("StagiaireId")]
        public string UserId { get; set; }

        // Propriété de navigation vers l'utilisateur Identity
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public DateTime Date { get; set; }

        // "Admin", "Français", "Cuisine", etc.
        public string ActivityName { get; set; }
        
        public bool IsPresent { get; set; } 
        public string Notes { get; set; }
    }
}