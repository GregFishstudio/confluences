using System.ComponentModel.DataAnnotations;

namespace Confluences.Domain.Entities
{
    public class AnnouncePage
    {
        [Key]
        public int AnnouncePageId { get; set; }

        [Required]
        public string AnnouncePageName { get; set; } = string.Empty;

        public bool IsActivated { get; set; }
    }
}
