using System.ComponentModel.DataAnnotations;

namespace Confluences.Domain.Entities
{
    // Carnet d'adresses des assistant·e·s sociaux·ales qui inscrivent des participant·e·s —
    // utilisé par les candidatures (FK) et proposé comme liste de choix pour
    // ApplicationUser.AssistantSocialEmail (qui reste un simple email, rapproché par valeur).
    public class AssistantSocial
    {
        [Key]
        public int AssistantSocialId { get; set; }

        [StringLength(100)]
        public string? Prenom { get; set; }

        [StringLength(100)]
        public string? Nom { get; set; }

        [Required]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Telephone { get; set; }

        // Service / organisme : CSIR, CSR, ARAS, EVAM...
        [StringLength(100)]
        public string? Organisme { get; set; }

        public string? Remarques { get; set; }

        public bool Actif { get; set; } = true;
    }
}
