using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class StagiaireNiveau
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string StagiaireId { get; set; } = string.Empty;

        public int Annee { get; set; }
        public int Trimestre { get; set; }

        public string? OralLevel { get; set; }
        public string? WrittenLevel { get; set; }

        [ForeignKey("StagiaireId")]
        public virtual ApplicationUser? Stagiaire { get; set; }
    }
}