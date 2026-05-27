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
        private readonly double                    _globalRate;
        private readonly int                       _year;
        private readonly string                    _quarter;
        private readonly byte[]                    _logo;
        private readonly string                    _courseName;
        private readonly DateTime                  _startDate;
        private readonly DateTime                  _endDate;
        private readonly List<ActivitySummaryItem> _activities;

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
            List<ActivitySummaryItem> activities                 = null)
        {
            _stagiaire                  = stagiaire;
            _evaluationText             = evaluationText;
            _followUpActions            = followUpActions;
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

        // ── Contenu ────────────────────────────────────────────────────────────
        private void RenderContent(IContainer container)
        {
            container.PaddingTop(20).Column(col =>
            {
                col.Spacing(14);

                // Titre
                col.Item().AlignCenter().Text(t =>
                {
                    t.Span("Bilan de mesure ").FontSize(14).FontColor(AccentColor);
                    t.Span(_courseName).FontSize(14).Bold().FontColor(AccentColor);
                    t.Span($" ({_startDate:dd.MM} – {_endDate:dd.MM.yyyy})").FontSize(13).FontColor(AccentColor);
                });

                // Participant
                col.Item().PaddingTop(6).Text(t =>
                {
                    t.Span("Participant-e : ").FontSize(11).Underline();
                    t.Span($" {_stagiaire.LastName?.ToUpper()} {_stagiaire.Firstname}").FontSize(13).Bold();
                });

                // 1. Évaluation linguistique
                col.Item().Element(c =>
                    RenderSection(c, "Cours de français", _evaluationText));

                // 2. Blocs spécifiques
                if (IsTelecours)
                {
                    if (!string.IsNullOrWhiteSpace(_homeVisitsText))
                        col.Item().Element(c => RenderSection(c, "Visites à domicile", _homeVisitsText));

                    if (!string.IsNullOrWhiteSpace(_followUpActions))
                        col.Item().Element(c => RenderSection(c, "Insertion sociale", _followUpActions));
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(_socialInsertionText))
                        col.Item().Element(c => RenderSection(c, "Insertion socio-professionnelle", _socialInsertionText));

                    if (!string.IsNullOrWhiteSpace(_digitalSkillsText))
                        col.Item().Element(c => RenderSection(c, "Utilisation de l'informatique", _digitalSkillsText));

                    // Accompagnement individualisé — texte simple
                    col.Item().Element(c => RenderAccompanyingSection(c));

                    if (!string.IsNullOrWhiteSpace(_followUpActions))
                        col.Item().Element(c => RenderSection(c, "Perspectives et objectifs", _followUpActions));
                }

                // 3. Niveau actuel — ne dupliquer que si oral ≠ écrit
                col.Item().PaddingTop(8).Text(t =>
                {
                    var oral  = _stagiaire.OralLevel    ?? "—";
                    var ecrit = _stagiaire.WrittenLevel  ?? "—";
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
        private void RenderSection(IContainer container, string title, string content)
        {
            container.Column(col =>
            {
                col.Item().PaddingBottom(4)
                    .Text(title).FontSize(12).FontColor(AccentColor).Italic();

                col.Item()
                    .Text(string.IsNullOrWhiteSpace(content) ? "Non renseigné" : content)
                    .FontSize(10).LineHeight(1.25f).Justify();

                col.Item().PaddingVertical(5)
                    .LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
            });
        }

        // ── Section accompagnement individualisé — texte simple ────────────────
        // Format :
        //   5 appuis (français oral, écrit, administratif).
        //   2 ateliers de français (français).
        //   [notes complémentaires libres si renseignées]
        private void RenderAccompanyingSection(IContainer container)
        {
            container.Column(col =>
            {
                // Titre de section
                col.Item().PaddingBottom(4)
                    .Text("Accompagnement individualisé")
                    .FontSize(12).FontColor(AccentColor).Italic();

                // Construire les lignes de texte depuis la liste d'activités
                var lines = BuildAccompanyingLines();

                if (lines.Any())
                {
                    col.Item().Column(inner =>
                    {
                        foreach (var line in lines)
                        {
                            inner.Item()
                                .Text(line)
                                .FontSize(10).LineHeight(1.3f);
                        }
                    });
                }
                else if (string.IsNullOrWhiteSpace(_individualAccompanyingText))
                {
                    col.Item()
                        .Text("Non renseigné")
                        .FontSize(10).FontColor(Colors.Grey.Medium).Italic();
                }

                // Notes complémentaires libres (sous les lignes auto)
                if (!string.IsNullOrWhiteSpace(_individualAccompanyingText))
                {
                    col.Item().PaddingTop(lines.Any() ? 4 : 0)
                        .Text(_individualAccompanyingText)
                        .FontSize(10).LineHeight(1.25f).Justify();
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