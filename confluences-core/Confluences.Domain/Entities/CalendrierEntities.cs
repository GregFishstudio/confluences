// ─────────────────────────────────────────────────────────────────────────────
// Fichier : Confluences.Domain/Entities/CalendrierEntities.cs
// ─────────────────────────────────────────────────────────────────────────────
using System;
using System.ComponentModel.DataAnnotations;

namespace Confluences.Domain.Entities
{
    // ── Session de formation ──────────────────────────────────────────────────
    /// <summary>
    /// Un trimestre de formation.
    ///
    /// Type (valeurs possibles) :
    ///   "fi_a1"     → Français &amp; Intégration niveau A1 (cible 60j : 20/40/60)
    ///   "fi_a2"     → Français &amp; Intégration niveau A2 (cible 60j : 20/40/60)
    ///   "telecours" → Français Télécours               (cible 32j : 10/20/32)
    ///
    /// IsTelevours() → true si type == "telecours"
    /// IsFi()        → true si type == "fi_a1" ou "fi_a2"
    /// </summary>
    public class SessionFormation
    {
        [Key]
        public int Id { get; set; }

        /// <summary>Nom lisible, ex: "S1-2025-FI_A1"</summary>
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>"fi_a1", "fi_a2" ou "telecours"</summary>
        [Required, MaxLength(20)]
        public string Type { get; set; } = "fi_a1";

        /// <summary>Premier jour de la session</summary>
        public DateTime StartDate { get; set; }

        /// <summary>Dernier jour de la session</summary>
        public DateTime EndDate { get; set; }

        /// <summary>Début des vacances inter-session (nullable)</summary>
        public DateTime? VacanceStart { get; set; }

        /// <summary>Fin des vacances inter-session (nullable)</summary>
        public DateTime? VacanceEnd { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ── Helpers ────────────────────────────────────────────────────────────
        public bool IsTelecours => Type?.ToLower() == "telecours";
        public bool IsFi        => Type?.ToLower() == "fi_a1" || Type?.ToLower() == "fi_a2";

        /// <summary>
        /// Retourne les jours cibles cumulés selon le mois dans la session (1/2/3).
        /// </summary>
        public int GetTargetDays(int monthInSession) =>
            (IsTelecours, Math.Max(1, Math.Min(3, monthInSession))) switch
            {
                (true,  1) => 10,
                (true,  2) => 20,
                (true,  _) => 32,
                (false, 1) => 20,
                (false, 2) => 40,
                (false, _) => 60,
            };
    }

    // ── Jour férié ────────────────────────────────────────────────────────────
    public class JourFerie
    {
        [Key]
        public int Id { get; set; }

        public DateTime Date { get; set; }

        [MaxLength(200)]
        public string? Label { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // ── Période de vacances générale ──────────────────────────────────────────
    public class PeriodeVacance
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(200)]
        public string Label { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate   { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}