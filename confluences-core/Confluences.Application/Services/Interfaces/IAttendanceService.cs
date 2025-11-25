using Api.Models.Dto.Attendance; 
using Confluences.Domain.Entities; // Nécessaire si vous utilisez des entités du domaine
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Assurez-vous que ce namespace correspond à la structure de votre projet Application
namespace Confluences.Application.Services.Interfaces
{
    public interface IAttendanceService
    {
        /// <summary>
        /// Sauvegarde ou met à jour les entrées de présence.
        /// Utilise le DTO de l'API pour recevoir les données.
        /// </summary>
        Task SaveAttendanceEntriesAsync(List<AttendanceEntryDto> entries);

        /// <summary>
        /// Récupère toutes les entrées de présence pour une période donnée.
        /// </summary>
        /// <returns>Une liste des entités de présence (incluant les données des stagiaires).</returns>
        Task<List<Presence>> GetPresencesByPeriodAsync(DateTime startDate, DateTime endDate);
    }
}