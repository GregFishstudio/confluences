using Confluences.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using Api.Controllers;        // ActivitySummaryItem
using System;
using System.Collections.Generic;
using System.Linq;

namespace Api.Documents
{
    public class BilanSessionDocument : IDocument
    {
        // ── Données ────────────────────────────────────────────────────────────
        private readonly ApplicationUser           _stagiaire;
        private readonly string                    _evaluationText;
        private readonly string                    _followUpActions;
        private readonly string                    _socialInsertionText;
        private readonly string                    _digitalSkillsText;
        private readonly string                    _individualAccompanyingText;
        private readonly string                    _homeVisitsText;
        private readonly string                    _objectivesText;
        private readonly double                    _globalRate;
        private readonly int                       _year;
        private readonly string                    _quarter;
        private readonly byte[]                    _logo;
        private readonly string                    _courseName;
        private readonly DateTime                  _startDate;
        private readonly DateTime                  _endDate;
        private readonly List<ActivitySummaryItem> _activities;
        private readonly string                    _oralLevel;
        private readonly string                    _writtenLevel;
        private readonly string                    _mesureLevel;

        private bool IsTelecours =>
            _courseName.ToLower().Contains("télécours") ||
            _courseName.ToLower().Contains("telecours");

        // ── Palette ────────────────────────────────────────────────────────────
        private static readonly Color PrimaryColor = Colors.Black;
        private static readonly Color AccentColor  = Color.FromHex("#4A90E2");

        // ── Constructeur ───────────────────────────────────────────────────────
        public BilanSessionDocument(
            ApplicationUser           stagiaire,
            string                    evaluationText,
            string                    followUpActions,
            string                    oralLevel,
            string                    writtenLevel,
            double                    globalRate,
            int                       year,
            string                    quarter,
            byte[]                    logoBytes,
            string                    courseName,
            DateTime                  startDate,
            DateTime                  endDate,
            string                    socialInsertionText        = "",
            string                    digitalSkillsText          = "",
            string                    individualAccompanyingText = "",
            string                    homeVisitsText             = "",
            List<ActivitySummaryItem> activities                 = null,
            string                    mesureLevel                = null,
            string                    objectivesText             = "")
        {
            _mesureLevel = mesureLevel;
            _stagiaire                  = stagiaire;
            _evaluationText             = evaluationText;
            _followUpActions            = followUpActions;
            _oralLevel                  = oralLevel;
            _writtenLevel               = writtenLevel;
            _globalRate                 = globalRate;
            _year                       = year;
            _quarter                    = quarter;
            _logo                       = logoBytes ?? Array.Empty<byte>();
            _courseName                 = courseName ?? "Français & Intégration";
            _startDate                  = startDate;
            _endDate                    = endDate;
            _socialInsertionText        = socialInsertionText;
            _digitalSkillsText          = digitalSkillsText;
            _individualAccompanyingText = individualAccompanyingText;
            _homeVisitsText             = homeVisitsText;
            _objectivesText             = objectivesText;
            _activities                 = activities ?? new List<ActivitySummaryItem>();
        }

        // ── Composition ────────────────────────────────────────────────────────
        public void Compose(IDocumentContainer document)
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0.75f, Unit.Inch);
                page.DefaultTextStyle(x =>
                    x.FontFamily("Helvetica").FontSize(11).FontColor(PrimaryColor));

                page.Header().Element(RenderHeader);
                page.Content().Element(RenderContent);
            });
        }

        // ── En-tête ────────────────────────────────────────────────────────────
        private void RenderHeader(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        if (_logo.Length > 0)
                            c.Item().Height(60).Image(_logo).FitHeight();
                        else
                            c.Item().Text("CONFLUENCES").FontSize(20).ExtraBold().FontColor(AccentColor);
                    });

                    row.RelativeItem().AlignRight().Column(c =>
                    {
                        c.Item().Text("www.confluences.ch").FontSize(9);
                        c.Item().Text("confluences.vaud@gmail.com").FontSize(9);
                        c.Item().Text("Chemin de Renens 52e").FontSize(9);
                        c.Item().Text("1004 Lausanne").FontSize(9);
                        c.Item().Text("077 / 520 02 84").FontSize(9);
                    });
                });

                col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
            });
        }

        // ── Taille du corps selon densité du contenu ──────────────────────────
        // < 500 chars → 11.5 pt   < 1100 chars → 11 pt   sinon → 10 pt
        private float ComputeBodySize()
        {
            int total = (_evaluationText?.Length             ?? 0)
                      + (_followUpActions?.Length            ?? 0)
                      + (_socialInsertionText?.Length        ?? 0)
                      + (_digitalSkillsText?.Length          ?? 0)
                      + (_individualAccompanyingText?.Length ?? 0)
                      + (_homeVisitsText?.Length             ?? 0)
                      + (_objectivesText?.Length             ?? 0)
                      + _activities.Count * 60;

            if (total < 500)  return 11.5f;
            if (total < 1100) return 11f;
            return 10f;
        }

        // ── Contenu ────────────────────────────────────────────────────────────
        private void RenderContent(IContainer container)
        {
            float bodySize = ComputeBodySize();

            container.PaddingTop(20).Column(col =>
            {
                col.Spacing(14);

                // Titre — ligne 1 : formation + niveau, ligne 2 : dates
                {
                    // Le niveau affiché dans le titre est celui de la MESURE suivie (ex. Télécours
                    // A2), pas le niveau personnel atteint par le/la participant·e — quelqu'un peut
                    // très bien dépasser le niveau de sa mesure (ex. B1.1 en cours de Télécours A2)
                    // sans que ça change l'intitulé du bilan.
                    string baseName   = _courseName;
                    string levelLabel = "";
                    int pIdx = _courseName.IndexOf(" (Niveau", StringComparison.OrdinalIgnoreCase);
                    if (pIdx > 0) baseName = _courseName.Substring(0, pIdx);

                    if (!string.IsNullOrWhiteSpace(_mesureLevel))
                    {
                        var lm = System.Text.RegularExpressions.Regex.Match(_mesureLevel, @"[A-Za-z]\d");
                        if (lm.Success) levelLabel = $" - Niveau {lm.Value.ToUpper()}";
                    }
                    else if (pIdx > 0)
                    {
                        var lm = System.Text.RegularExpressions.Regex.Match(
                            _courseName.Substring(pIdx), @"[A-Za-z]\d");
                        if (lm.Success) levelLabel = $" - Niveau {lm.Value.ToUpper()}";
                    }

                    col.Item().AlignCenter().Text(t =>
                    {
                        t.AlignCenter();
                        t.Span("Bilan de mesure ").FontSize(14).FontColor(AccentColor);
                        t.Span(baseName + levelLabel).FontSize(14).Bold().FontColor(AccentColor);
                    });
                    col.Item().AlignCenter()
                        .Text($"{_startDate:dd.MM} – {_endDate:dd.MM.yyyy}")
                        .FontSize(12).FontColor(AccentColor);

                    if (_stagiaire.DateFinParticipation.HasValue)
                    {
                        var frenchCulture = new System.Globalization.CultureInfo("fr-FR");
                        string interruptionDate = _stagiaire.DateFinParticipation.Value.ToString("dd MMMM yyyy", frenchCulture);
                        string motif = string.IsNullOrWhiteSpace(_stagiaire.RaisonInterruptionMesure)
                            ? ""
                            : $" Motif : {_stagiaire.RaisonInterruptionMesure}.";
                        col.Item().AlignCenter()
                            .Text($"Mesure interrompue le {interruptionDate}.{motif}")
                            .FontSize(11).Italic().FontColor(Colors.Red.Darken2);
                    }
                }

                // Participant
                col.Item().PaddingTop(6).Text(t =>
                {
                    t.Span("Participant-e : ").FontSize(11).Underline();
                    t.Span($" {_stagiaire.LastName?.ToUpper()} {_stagiaire.Firstname}").FontSize(13).Bold();
                });

                // 1. Évaluation linguistique
                col.Item().Element(c =>
                    RenderSection(c, "Cours de français", _evaluationText, bodySize));

                // 2. Blocs spécifiques
                if (IsTelecours)
                {
                    if (!string.IsNullOrWhiteSpace(_homeVisitsText))
                        col.Item().Element(c => RenderSection(c, "Visites à domicile", _homeVisitsText, bodySize));

                    if (!string.IsNullOrWhiteSpace(_followUpActions))
                        col.Item().Element(c => RenderSection(c, "Insertion sociale", _followUpActions, bodySize));

                    if (!string.IsNullOrWhiteSpace(_objectivesText))
                        col.Item().Element(c => RenderSection(c, "Objectifs", _objectivesText, bodySize));
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(_socialInsertionText))
                        col.Item().Element(c => RenderSection(c, "Insertion socio-professionnelle", _socialInsertionText, bodySize));

                    if (!string.IsNullOrWhiteSpace(_digitalSkillsText))
                        col.Item().Element(c => RenderSection(c, "Utilisation de l'informatique", _digitalSkillsText, bodySize));

                    col.Item().Element(c => RenderAccompanyingSection(c, bodySize));

                    if (!string.IsNullOrWhiteSpace(_followUpActions))
                        col.Item().Element(c => RenderSection(c, "Perspectives et objectifs", _followUpActions, bodySize));
                }

                // 3. Niveau actuel
                col.Item().PaddingTop(8).Text(t =>
                {
                    var oral  = _oralLevel    ?? _stagiaire.OralLevel   ?? "—";
                    var ecrit = _writtenLevel ?? _stagiaire.WrittenLevel ?? "—";
                    t.Span("Niveau actuel : ").FontSize(11).FontColor(Colors.Grey.Darken2);
                    if (oral == ecrit)
                    {
                        t.Span(oral).FontSize(12).Bold().FontColor(AccentColor);
                    }
                    else
                    {
                        t.Span("Oral ").FontSize(11).FontColor(AccentColor).Italic();
                        t.Span(oral).FontSize(12).Bold().FontColor(AccentColor);
                        t.Span("  |  Écrit ").FontSize(11).FontColor(AccentColor).Italic();
                        t.Span(ecrit).FontSize(12).Bold().FontColor(AccentColor);
                    }
                });
            });
        }

        // ── Section texte générique ────────────────────────────────────────────
        private void RenderSection(IContainer container, string title, string content,
                                   float bodySize = 10f)
        {
            container.Column(col =>
            {
                col.Item().PaddingBottom(4)
                    .Text(title).FontSize(12).FontColor(AccentColor).Italic();

                col.Item()
                    .Text(string.IsNullOrWhiteSpace(content) ? "Non renseigné" : content)
                    .FontSize(bodySize).LineHeight(1.25f).Justify();

                col.Item().PaddingVertical(5)
                    .LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
            });
        }

        // ── Section accompagnement individualisé — texte simple ────────────────
        private void RenderAccompanyingSection(IContainer container, float bodySize = 10f)
        {
            container.Column(col =>
            {
                col.Item().PaddingBottom(4)
                    .Text("Accompagnement individualisé")
                    .FontSize(12).FontColor(AccentColor).Italic();

                var lines = BuildAccompanyingLines();

                if (lines.Any())
                {
                    col.Item().Column(inner =>
                    {
                        foreach (var line in lines)
                            inner.Item().Text(line).FontSize(bodySize).LineHeight(1.3f);
                    });
                }
                else if (string.IsNullOrWhiteSpace(_individualAccompanyingText))
                {
                    string civilite = _stagiaire.Gender?.GenderName?.StartsWith("F", StringComparison.OrdinalIgnoreCase) == true
                        ? "Madame" : "Monsieur";
                    col.Item()
                        .Text($"{civilite} n'a suivi aucun accompagnement ni atelier facultatif.")
                        .FontSize(bodySize).LineHeight(1.3f);
                }

                if (!string.IsNullOrWhiteSpace(_individualAccompanyingText))
                {
                    col.Item().PaddingTop(lines.Any() ? 4 : 0)
                        .Text(_individualAccompanyingText)
                        .FontSize(bodySize).LineHeight(1.25f).Justify();
                }

                col.Item().PaddingVertical(5)
                    .LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
            });
        }

        /// <summary>
        /// Construit les lignes de texte simple pour l'accompagnement.
        /// Regroupe les activités par type et les formate en une phrase par type :
        ///   "N appuis (label1, label2)."
        ///   "N atelier(s) de français (label1, label2)."
        /// </summary>
        private List<string> BuildAccompanyingLines()
        {
            var lines = new List<string>();

            if (!_activities.Any()) return lines;

            // Accompagnements (type "acco")
            var accoList = _activities.Where(a => a.Type == "acco").ToList();
            if (accoList.Any())
            {
                int    total  = accoList.Sum(a => a.Count);
                string labels = string.Join(", ", accoList.Select(a => a.Label.ToLower()));
                string mot    = total > 1 ? "appuis" : "appui";
                lines.Add($"{total} {mot} ({labels}).");
            }

            // Ateliers (type "atelier")
            var atelierList = _activities.Where(a => a.Type == "atelier").ToList();
            if (atelierList.Any())
            {
                int    total  = atelierList.Sum(a => a.Count);
                string labels = string.Join(", ", atelierList.Select(a => a.Label.ToLower()));
                string mot    = total > 1 ? "ateliers" : "atelier";
                lines.Add($"{total} {mot} ({labels}).");
            }

            return lines;
        }
    }
}