using System.ComponentModel.DataAnnotations;

namespace Confluences.Domain.Entities
{
    public class SessionNumber
    {
        [Key]
        public int SessionNumberId { get; set; }

    }
}
