using System.ComponentModel.DataAnnotations;

namespace Confluences.Domain.Entities
{
    public class HomeworkType
    {
        [Key]
        public int HomeworkTypeId { get; set; }

        [Required]
        public string HomeworkTypeName { get; set; } = string.Empty;

        public int HomeworkOrder { get; set; }
    }
}
