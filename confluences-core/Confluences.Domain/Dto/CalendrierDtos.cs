// ─────────────────────────────────────────────────────────────────────────────
// Fichier : Confluences.Domain/Dto/CalendrierDtos.cs
// ─────────────────────────────────────────────────────────────────────────────
using System;
using System.ComponentModel.DataAnnotations;

namespace Confluences.Domain.Dto
{
    // ── Session ───────────────────────────────────────────────────────────────
    public class SessionFormationDto
    {
        public int?     Id           { get; set; }   // null = nouvelle session

        [Required, MaxLength(100)]
        public string   Name         { get; set; } = string.Empty;

        /// <summary>"fi" ou "telecours"</summary>
        [Required, MaxLength(20)]
        public string   Type         { get; set; } = "fi";

        [Required]
        public DateTime StartDate    { get; set; }

        [Required]
        public DateTime EndDate      { get; set; }

        public DateTime? VacanceStart { get; set; }
        public DateTime? VacanceEnd   { get; set; }

        // ── Propriétés calculées (lecture seule, renvoyées au frontend) ────────
        /// <summary>Jours cumulés attendus pour chacun des 3 mois</summary>
        public int[] TargetDaysByMonth => Type?.ToLower()?.Contains("tele") == true
            ? new[] { 10, 20, 32 }
            : new[] { 20, 40, 60 };
    }

    // ── Jour férié ────────────────────────────────────────────────────────────
    public class JourFerieDto
    {
        public int?     Id    { get; set; }
        public DateTime Date  { get; set; }
        public string?  Label { get; set; }
    }

    // ── Période de vacances ───────────────────────────────────────────────────
    public class PeriodeVacanceDto
    {
        public int?     Id        { get; set; }

        [MaxLength(200)]
        public string   Label     { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate   { get; set; }
    }

    // ── Réponse agrégée (utilisée par PresenceView pour tout charger en 1 call) ──
    public class CalendrierCompletDto
    {
        public List<SessionFormationDto> Sessions  { get; set; } = new();
        public List<JourFerieDto>        Feries    { get; set; } = new();
        public List<PeriodeVacanceDto>   Vacances  { get; set; } = new();
    }
}