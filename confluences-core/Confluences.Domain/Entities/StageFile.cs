using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class StageFile
    {
        [Key]
        public int StageFileId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public byte[] Data { get; set; } = new byte[0];

        public int StageId { get; set; }
        [ForeignKey(nameof(StageId))]
        public virtual Stage? Stage { get; set; }
    }
}
