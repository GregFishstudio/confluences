// Fichier : Confluences.Domain/Entities/SessionReport.cs

using System;
using Confluences.Domain.Entities; // Assurez-vous que le chemin vers ApplicationUser est correct

namespace Confluences.Domain.Entities
{
    public class SessionReport
    {
        public int Id { get; set; }
        
        // Clé pour lier au stagiaire
        public string StagiaireId { get; set; } 
        // Relation EF Core : nécessite le using Microsoft.AspNetCore.Identity; dans le DbContext
        public ApplicationUser Stagiaire { get; set; }

        // Clé pour identifier la période
        public int Year { get; set; }
        public string Quarter { get; set; } // "1", "2", "3", "4"
        
        // Données sauvegardées
        public string EvaluationText { get; set; } // Texte du bilan
        public string FollowUpActions { get; set; } // Actions de suivi
        public double GlobalRate { get; set; } // Taux global
        public string WorkshopsJson { get; set; } // Liste des ateliers sérialisée (JSON)
    }
}