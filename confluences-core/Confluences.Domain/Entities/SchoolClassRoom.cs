using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class SchoolClassRoom
    {
        [Key]
        public int SchoolClassRoomId { get; set; }

        [DisplayName("Nom")]
        [Required]
        public string SchoolClassRoomName { get; set; } = string.Empty;

        [DisplayName("Lien du tuto")]
        public string? ExplanationVideoLink { get; set; }

        public bool IsArchived { get; set; }

        [InverseProperty(nameof(SchoolClassRoomExplanation.SchoolClassRoom))]
        public virtual ICollection<SchoolClassRoomExplanation>? SchoolClassRoomExplanations { get; set; }
        [InverseProperty(nameof(Session.SchoolClassRoom))]
        public virtual ICollection<Session>? Sessions { get; set; }


    }
}
