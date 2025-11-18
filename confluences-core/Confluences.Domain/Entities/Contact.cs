using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class Contact
    {
        [Key]
        public int ContactId { get; set; }

        [StringLength(50)]
        public string? Nom { get; set; }

        [StringLength(50)]
        public string? Prenom { get; set; }

        [StringLength(50)]
        public string? Fonction { get; set; }

        [StringLength(50)]
        public string? Email { get; set; }

        [StringLength(13)]
        public string? TelFix { get; set; }

        [StringLength(13)]
        public string? Natel { get; set; }

        public DateTime? DateCreation { get; set; }

        public DateTime? DateModification { get; set; }

        // FK
        public string? CreateurId { get; set; }
        [ForeignKey(nameof(CreateurId))]
        public virtual ApplicationUser? Createur { get; set; }

        public string? ModificateurId { get; set; }
        [ForeignKey(nameof(ModificateurId))]
        public virtual ApplicationUser? ContactModificateur { get; set; }

        public int? EntrepriseId { get; set; }
        [ForeignKey(nameof(EntrepriseId))]
        public virtual Entreprise? Entreprise { get; set; }
    }
}
