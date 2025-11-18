using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class AppointmentStudent
    {
        [Key]
        public int AppointmentStudentId { get; set; }

        [Required]
        public int AppointmentId { get; set; }
        [ForeignKey(nameof(AppointmentId))]
        public virtual Appointment? Appointment { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;
        [ForeignKey(nameof(StudentId))]
        public virtual ApplicationUser? Student { get; set; }
    }
}
