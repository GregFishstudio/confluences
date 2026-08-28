using System.ComponentModel.DataAnnotations;

namespace Confluences.Domain.Entities
{
    public class MarrainageQuestion
    {
        [Key]
        public int MarrainageQuestionId { get; set; }
        public string Label { get; set; } = string.Empty;

        // "text", "textarea", "number", "choice"
        public string Type { get; set; } = "text";

        // Pour Type = "choice" : options séparées par "|" (ex: "Oui|Non|Autre")
        public string? Options { get; set; }

        public bool Required { get; set; }
        public int Order { get; set; }
        public bool Active { get; set; } = true;

        public virtual ICollection<MarrainageAnswer>? Answers { get; set; }
    }
}
