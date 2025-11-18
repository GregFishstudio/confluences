using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class SessionTeacher
    {
        [DisplayName("Session")]
        // Is Key aswell go look in context
        public int SessionId { get; set; }
        [ForeignKey(nameof(SessionId))]
        public virtual Session? Session { get; set; }

        [DisplayName("Formateur-trice")]
        // Is Key aswell go look in context
        public string? TeacherId { get; set; }
        [ForeignKey(nameof(TeacherId))]
        public virtual ApplicationUser? Teacher { get; set; }

        public int Order { get; set; }
    }
}
