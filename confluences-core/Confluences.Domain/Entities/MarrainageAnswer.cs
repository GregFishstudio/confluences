using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class MarrainageAnswer
    {
        [Key]
        public int MarrainageAnswerId { get; set; }

        public int MarrainageId { get; set; }
        [ForeignKey(nameof(MarrainageId))]
        public virtual Marrainage? Marrainage { get; set; }

        public int MarrainageQuestionId { get; set; }
        [ForeignKey(nameof(MarrainageQuestionId))]
        public virtual MarrainageQuestion? Question { get; set; }

        public string? Value { get; set; }
    }
}
