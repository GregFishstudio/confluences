using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class Session
    {
        [Key]
        public int SessionId { get; set; }

        [DisplayName("Date de début")]
        [Required]
        public DateTime DateStart { get; set; }

        [DisplayName("Date de fin")]
        [Required]
        public DateTime DateEnd { get; set; }

        [DisplayName("Classe")]
        [Required]
        public int SchoolClassRoomId { get; set; }
        [ForeignKey(nameof(SchoolClassRoomId))]
        public virtual SchoolClassRoom? SchoolClassRoom { get; set; }

        [DisplayName("Numéro de la session")]
        [Required]
        public int SessionNumberId { get; set; }
        [ForeignKey(nameof(SessionNumberId))]
        public virtual SessionNumber? SessionNumber { get; set; }

        [NotMapped]
        public string Description => string.Join(" ", DateStart.Year, SessionNumberId, SchoolClassRoom?.SchoolClassRoomName);

        [InverseProperty("Session")]
        public virtual ICollection<HomeworkV2>? HomeworkV2s { get; set; }
        [InverseProperty(nameof(SessionStudent.Session))]
        public virtual ICollection<SessionStudent>? SessionStudents { get; set; }
        [InverseProperty(nameof(SessionTeacher.Session))]
        public virtual ICollection<SessionTeacher>? SessionTeachers { get; set; }

        [InverseProperty(nameof(Stage.Session))]
        public virtual ICollection<Stage>? Stages { get; set; }

    }
}
