// File: Confluences.Domain/Dto/Attendance/AttendanceEntryDto.cs

using System;
using System.ComponentModel.DataAnnotations;

namespace Confluences.Domain.Dto
{
    public class AttendanceEntryDto
    {
        [Required]
        public string StagiaireId { get; set; }

        [Required]
        public DateTime Date { get; set; } // Jour de l'enregistrement

        [Required]
        [MaxLength(2)] // 'p', 'a', 'e', 'r', 's', ou vide
        public string Statut { get; set; }
    }
}