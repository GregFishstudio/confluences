using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities.Views
{
    [Table("ressources")]
    public class Ressource
    {
        public int? Id { get; set; }
        public string? SessionNumber { get; set; }
        public DateTime? Date { get; set; }
        public string? Name { get; set; }
        public string? HomeworkTypeName { get; set; }
        public string? Teacher { get; set; }
        public string? RessourceType { get; set; }
    }
}
