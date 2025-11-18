using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        public string AppointmentName { get; set; } = string.Empty;

        [Required]
        public DateTime DateStart { get; set; }

        [Required]
        public DateTime DateEnd { get; set; }

        public bool IsWeekly { get; set; }

        [Required]
        public string TeacherId { get; set; } = string.Empty;
        [ForeignKey(nameof(TeacherId))]
        public virtual ApplicationUser? Teacher { get; set; }

        [InverseProperty(nameof(AppointmentStudent.Appointment))]
        public virtual ICollection<AppointmentStudent>? AppointmentStudents { get; set; }
    }
}
