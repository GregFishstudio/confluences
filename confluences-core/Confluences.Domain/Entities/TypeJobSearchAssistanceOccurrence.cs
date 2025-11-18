using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class TypeJobSearchAssistanceOccurrence
    {
        [Key]
        public int TypeJobSearchAssistanceOccurrenceId { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [InverseProperty(nameof(JobSearchAssistance.TypeJobSearchAssistanceOccurrence))]
        public virtual ICollection<JobSearchAssistance>? JobSearchAssistances { get; set; }
    }
}
