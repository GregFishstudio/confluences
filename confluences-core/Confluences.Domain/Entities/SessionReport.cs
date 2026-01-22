// Fichier : Confluences.Domain/Entities/SessionReport.cs

using System;
using Confluences.Domain.Entities; // Assurez-vous que le chemin vers ApplicationUser est correct

namespace Confluences.Domain.Entities
{
    public class SessionReport
{
    public int SessionReportId { get; set; }

    public string StagiaireId { get; set; }
    public ApplicationUser Stagiaire { get; set; }

    public int Year { get; set; }
    public string Quarter { get; set; }

    public string EvaluationText { get; set; }
    public string FollowUpActions { get; set; }
    public double GlobalRate { get; set; }

    public string? WorkshopsJson { get; set; }

    // ============================
    // ✅ NOUVEAUX CHAMPS BILAN
    // ============================
    public string? SocialInsertionText { get; set; }
public string? DigitalSkillsText { get; set; }
public string? IndividualAccompanyingText { get; set; }
public string? HomeVisitsText { get; set; }


    // ============================
    // ✅ AUDIT
    // ============================
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

}