//using Confluences.Domain.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class Presence
    {
        // Clé primaire de l'entrée de présence
        public int Id { get; set; }

        // Clé étrangère vers le stagiaire (string est la clé par défaut d'IdentityUser)
        // L'opérateur '!' est utilisé car EF Core garantit que cette FK ne sera pas null après la sauvegarde.
        public string StagiaireId { get; set; } = null!;

        // Date du jour concerné par l'enregistrement
        public DateTime Date { get; set; }

        // Statut de présence : 'p', 'a', 'e', 'r', 's', ou ''
        // L'opérateur '!' est utilisé car le statut sera toujours défini lors de la création/mise à jour.
        public string Statut { get; set; } = null!; 

        // Propriété de navigation vers le stagiaire
        // Peut être null avant chargement via Include.
        public virtual ApplicationUser Stagiaire { get; set; } = null!;

        // Propriété optionnelle: Si la présence est liée à un stage ou cours spécifique
        public int? StageId { get; set; }

        // Navigation property is nullable since StageId is nullable
        public virtual Stage? Stage { get; set; }

        // Optionnel : Ajouter des champs de suivi (création, modification) si non inclus dans BaseEntity
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedAt { get; set; }
    }
}