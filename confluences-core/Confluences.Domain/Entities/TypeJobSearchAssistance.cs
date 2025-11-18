using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class TypeJobSearchAssistance
    {
        [Key]
        public int TypeJobSearchAssistanceId { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [InverseProperty(nameof(JobSearchAssistance.TypeJobSearchAssistance))]
        public virtual ICollection<JobSearchAssistance>? JobSearchAssistances { get; set; }
    }
}
