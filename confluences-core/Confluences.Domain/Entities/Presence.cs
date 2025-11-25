using Confluences.Domain.Common; // Assurez-vous d'utiliser votre base entity si vous en avez une (BaseEntity, IAuditable, etc.)
using System;

namespace Confluences.Domain.Entities
{
    public class Presence // Si vous utilisez BaseEntity, héritez-en ici
    {
        // Clé primaire de l'entrée de présence
        public int Id { get; set; }

        // Clé étrangère vers le stagiaire
        public Guid StagiaireId { get; set; }

        // Date du jour concerné par l'enregistrement
        public DateTime Date { get; set; }

        // Statut de présence : 'p', 'a', 'e', 'r', 's', ou ''
        // Utilisé par le Front-end
        public string Statut { get; set; }

        // Propriété de navigation vers le stagiaire
        public virtual ApplicationUser Stagiaire { get; set; }

        // Propriété optionnelle: Si la présence est liée à un stage ou cours spécifique
        public int? StageId { get; set; }
        public virtual Stage Stage { get; set; }

        // Optionnel : Ajouter des champs de suivi (création, modification) si non inclus dans BaseEntity
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedAt { get; set; }
    }
}