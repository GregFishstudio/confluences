using System.ComponentModel.DataAnnotations;

namespace Confluences.Domain.Entities
{
    // Région de marrainage (Lausanne, Riviera, Moudon…) — liste éditable par les formatrices.
    // Marraines/parrains (Marrainage.Region) et participant·e·s (ApplicationUser.Region) stockent le
    // nom de la région : renommer une région met ces fiches à jour (RegionsController).
    public class Region
    {
        [Key]
        public int RegionId { get; set; }

        [Required]
        [StringLength(50)]
        public string Nom { get; set; } = string.Empty;

        // Couleur d'affichage (#RRGGBB).
        [StringLength(9)]
        public string? Couleur { get; set; }

        public int Ordre { get; set; }

        // Où et comment chercher des parrains/marraines dans cette région (associations, paroisses…).
        public string? PistesRecherche { get; set; }
    }
}
