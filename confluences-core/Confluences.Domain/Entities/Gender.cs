using System.ComponentModel.DataAnnotations;

namespace Confluences.Domain.Entities
{
    public class Gender
    {

        [Key]
        public short GenderId { get; set; }
        [Required]
        [StringLength(10)]
        public string GenderName { get; set; } = string.Empty;

    }
}
