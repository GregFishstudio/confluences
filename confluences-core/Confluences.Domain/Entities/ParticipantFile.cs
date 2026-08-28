using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class ParticipantFile
    {
        [Key]
        public int ParticipantFileId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string? Category { get; set; }
        public byte[] Data { get; set; } = new byte[0];

        public string StudentId { get; set; } = string.Empty;
        [ForeignKey(nameof(StudentId))]
        public virtual ApplicationUser? Student { get; set; }
    }
}
