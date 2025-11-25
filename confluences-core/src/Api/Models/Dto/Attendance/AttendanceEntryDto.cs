// File: Api/Models/Dto/Attendance/AttendanceEntryDto.cs

using System;
using System.ComponentModel.DataAnnotations;

namespace Api.Models.Dto.Attendance
{
    public class AttendanceEntryDto
    {
        [Required]
        public Guid StagiaireId { get; set; } // Identifiant du stagiaire

        [Required]
        public DateTime Date { get; set; } // Jour de l'enregistrement

        [Required]
        [MaxLength(2)] // 'p', 'a', 'e', 'r', 's', ou vide
        public string Statut { get; set; }
    }
}