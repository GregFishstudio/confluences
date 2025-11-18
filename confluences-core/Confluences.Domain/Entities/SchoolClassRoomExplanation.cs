using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class SchoolClassRoomExplanation
    {
        [Key]
        public int SchoolClassRoomExplanationId { get; set; }

        [Required]
        public string LanguageName { get; set; } = string.Empty;

        [Required]
        public string AudioLink { get; set; } = string.Empty;

        [Required]
        public int SchoolClassRoomId { get; set; }
        [ForeignKey(nameof(SchoolClassRoomId))]
        public virtual SchoolClassRoom? SchoolClassRoom { get; set; }
    }
}
