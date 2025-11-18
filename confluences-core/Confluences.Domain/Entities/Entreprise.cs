using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class Entreprise
    {
        [Key]
        public int EntrepriseId { get; set; }

        [Required]
        [StringLength(50)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Ville { get; set; } = string.Empty;

        [StringLength(13)]
        public string? TelNatel { get; set; }

        [StringLength(13)]
        public string? TelFix { get; set; }

        [StringLength(50)]
        public string? Adr1 { get; set; }

        [StringLength(50)]
        public string? Adr2 { get; set; }

        [StringLength(4)]
        public string? CodePostal { get; set; }

        [StringLength(50)]
        public string? Email { get; set; }
     
        [StringLength(10000)]
        public string? Remarque { get; set; }

        public DateTime? DateCreation { get; set; }

        public int? TypeEntrepriseId { get; set; }
        [ForeignKey(nameof(TypeEntrepriseId))]
        public virtual TypeEntreprise? TypeEntreprise { get; set; }

        public int? TypeMoyenId { get; set; }
        [ForeignKey(nameof(TypeMoyenId))]
        public virtual TypeMoyen? TypeMoyen { get; set; }

        public DateTime? DateDernierContact { get; set; }
        public DateTime? DateLastRecall { get; set; }

        // OK
        public string? CreateurId { get; set; }
        [ForeignKey(nameof(CreateurId))]
        public virtual ApplicationUser? Createur { get; set; }

        // Ok
        public string? FormateurIdDernierContact { get; set; }
        public virtual ApplicationUser? FormateurDernierContact { get; set; }

        // ok
        public string? StagiaireIdDernierContact { get; set; }
        public virtual ApplicationUser? StagiaireDernier { get; set; }

        [InverseProperty(nameof(Contact.Entreprise))]
        public virtual ICollection<Contact>? Contacts { get; set; }

        [InverseProperty(nameof(EntrepriseDomaine.Entreprise))]
        public virtual ICollection<EntrepriseDomaine>? EntrepriseDomaines { get; set; }

        [InverseProperty(nameof(EntrepriseMetier.Entreprise))]
        public virtual ICollection<EntrepriseMetier>? EntrepriseMetiers { get; set; }

        [InverseProperty(nameof(EntrepriseOffre.Entreprise))]
        public virtual ICollection<EntrepriseOffre>? EntrepriseOffres { get; set; }

        [InverseProperty(nameof(Stage.Entreprise))]
        public virtual ICollection<Stage>? Stages { get; set; }

        [InverseProperty(nameof(LastContact.Entreprise))]
        public virtual ICollection<LastContact>? LastContacts { get; set; }


    }
}