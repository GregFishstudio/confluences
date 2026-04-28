namespace Confluences.Domain.Entities
{
    public class MatrixColumn
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty; // ex: Activity_17123456
        public string Label { get; set; } = string.Empty; // ex: Cuisine
        public string Type { get; set; } = string.Empty; // "acco" ou "atelier"
        public int DisplayOrder { get; set; }
    }
}