using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class JobSearchAssistance
    {
        [Key]
        public int JobSearchAssistanceId { get; set; }

        public string? Address { get; set; }
        public string? Town { get; set; }
        public string? ZipCode { get; set; }
        public DateTime? Date { get; set; }
        public string? Description { get; set; }
        public string? Website { get; set; }
        public string? KeyWords { get; set; }

        public int? TypeJobSearchAssistanceId { get; set; }
        [ForeignKey(nameof(TypeJobSearchAssistanceId))]
        public virtual TypeJobSearchAssistance? TypeJobSearchAssistance { get; set; }

        public int? TypeJobSearchAssistanceOccurrenceId { get; set; }
        [ForeignKey(nameof(TypeJobSearchAssistanceOccurrenceId))]
        public virtual TypeJobSearchAssistanceOccurrence? TypeJobSearchAssistanceOccurrence { get; set; }

    }
}
