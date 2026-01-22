using System;
using System.ComponentModel.DataAnnotations;

namespace Confluences.Domain.Entities
{
    public class AccompanyingPresence 
    {
        [Key]
        public int AccompanyingPresenceId { get; set; }
        
        public string StagiaireId { get; set; }
        public ApplicationUser Stagiaire { get; set; }

        public DateTime Date { get; set; }

        // "Admin", "Français", "Cuisine", etc.
        public string ActivityName { get; set; }
        
        public bool IsPresent { get; set; } 
        public string Notes { get; set; }
    }
}