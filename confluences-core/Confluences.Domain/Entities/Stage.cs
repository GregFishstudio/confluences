using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class Stage
    {
        [Key]
        public int StageId { get; set; }

        [Required]
        [StringLength(30)]
        public string Nom { get; set; } = string.Empty;

        [NotMapped]
        public int Year => Debut.Year;
        public DateTime Debut { get; set; }

        public DateTime? Fin { get; set; }

        public string? Bilan { get; set; }

        public string? ActionSuivi { get; set; }
      
        public string? Remarque { get; set; }

        public bool? Rapport { get; set; }
        
        public bool? Attestation { get; set; }

        public bool? Repas { get; set; }

        public bool? Trajets { get; set; }

        public bool? HasContract { get; set; }

        // FK
        [Required]
        public string StagiaireId { get; set; } = string.Empty;
        [ForeignKey(nameof(StagiaireId))]
        public virtual ApplicationUser? Stagiaire { get; set; }

        public int? EntrepriseId { get; set; }
        [ForeignKey(nameof(EntrepriseId))]
        public virtual Entreprise? Entreprise { get; set; }

        public string? CreateurId { get; set; }
        [ForeignKey(nameof(CreateurId))]
        public virtual ApplicationUser? Createur { get; set; }

        [Required]
        public int TypeMetierId { get; set; }
        [ForeignKey(nameof(TypeMetierId))]
        public virtual TypeMetier? TypeMetier { get; set; }

        public int? TypeStageId { get; set; }
        [ForeignKey(nameof(TypeStageId))]
        public virtual TypeStage? TypeStage { get; set; }

        public int TypeIntershipActivityId { get; set; }
        [ForeignKey(nameof(TypeIntershipActivityId))]
        public virtual TypeIntershipActivity? TypeIntershipActivity { get; set; }

        [StringLength(11)]
        public string? HoraireMatin { get; set; }

        [StringLength(11)]
        public string? HoraireApresMidi { get; set; }

        [StringLength(30)]
        public string? HoraireSamedi { get; set; }

        public int? TypeAnnonceId { get; set; }
        [ForeignKey(nameof(TypeAnnonceId))]
        public virtual TypeAnnonce? TypeAnnonce { get; set; }

        public int? SessionId { get; set; }
        [ForeignKey(nameof(SessionId))]
        public virtual Session? Session { get; set; }

        [InverseProperty(nameof(StageFile.Stage))]
        public virtual ICollection<StageFile>? StageFiles { get; set; }
    }
}
