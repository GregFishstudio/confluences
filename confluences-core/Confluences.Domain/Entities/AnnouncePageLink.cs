using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class AnnouncePageLink
    {
        [Key]
        public int AnnouncePageLinkId { get; set; }

        [Required]
        public string AnnouncePageLinkName { get; set; } = string.Empty;

        [Required]
        public string AnnouncePageLinkLanguage { get; set; } = string.Empty;

        public int AnnouncePageId { get; set; }
        [ForeignKey(nameof(AnnouncePageId))]
        public virtual AnnouncePage? AnnouncePage { get; set; }
    }
}
