using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class MarrainageFile
    {
        [Key]
        public int MarrainageFileId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string? Category { get; set; }
        public byte[] Data { get; set; } = new byte[0];

        public int MarrainageId { get; set; }
        [ForeignKey(nameof(MarrainageId))]
        public virtual Marrainage? Marrainage { get; set; }
    }
}
