using Confluences.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using System;
using System.Globalization;

namespace Api.Documents
{
    public class BilanSessionDocument : IDocument
    {
        private readonly ApplicationUser _stagiaire;
        private readonly string _evaluationText;
        private readonly string _followUpActions;
        private readonly double _globalRate;
        private readonly int _year;
        private readonly string _quarter;
        private readonly byte[] _logo;
        private readonly string _courseName;
        private readonly DateTime _startDate;
        private readonly DateTime _endDate;

        // Détection de la mesure
        private bool IsTelecours => _courseName.ToLower().Contains("télécours") || _courseName.ToLower().Contains("telecours");

        // Palette
        private readonly CultureInfo _frenchCulture = new CultureInfo("fr-FR");
        private static readonly Color PrimaryColor = Color.FromHex("#1A202C");
        private static readonly Color AccentColor = Color.FromHex("#00B4B0");
        private static readonly Color MediumGrey = Color.FromHex("#718096");
        private static readonly Color LightGrey = Color.FromHex("#EDF2F7");

        public BilanSessionDocument(
            ApplicationUser stagiaire,
            string evaluationText,
            string followUpActions,
            string oralLevel,
            string writtenLevel,
            double globalRate,
            int year,
            string quarter,
            byte[] logoBytes,
            string courseName,    // <--- NOUVEAU
            DateTime startDate,   // <--- NOUVEAU
            DateTime endDate)     // <--- NOUVEAU
        {
            _stagiaire = stagiaire;
            _evaluationText = evaluationText;
            _followUpActions = followUpActions;
            _globalRate = globalRate;
            _year = year;
            _quarter = quarter;
            _logo = logoBytes ?? Array.Empty<byte>();
            _courseName = courseName ?? "Français & Intégration";
            _startDate = startDate;
            _endDate = endDate;
        }

        public void Compose(IDocumentContainer document)
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Inch);
                page.DefaultTextStyle(x => x.FontFamily("Helvetica").FontSize(11).FontColor(PrimaryColor));

                page.Header().Element(RenderHeader);
                page.Content().Element(RenderContent);
                page.Footer().AlignCenter().Text(x => {
                    x.Span("Association Confluences - Chemin de Renens 52e, 1004 Lausanne").FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        }

        private void RenderHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    if (_logo.Length > 0)
                        col.Item().Height(55).Image(_logo).FitHeight();
                    else
                        col.Item().Text("CONFLUENCES").FontSize(22).ExtraBold().FontColor(AccentColor);
                });

                row.RelativeItem().AlignRight().PaddingTop(8).Column(c =>
                {
                    c.Spacing(3);
                    c.Item().Text("Association Confluences").SemiBold().FontSize(10);
                    c.Item().Text("Chemin de Renens 52e, 1004 Lausanne").FontSize(9).FontColor(MediumGrey);
                    c.Item().Text("confluences.vaud@gmail.com").FontSize(9).FontColor(MediumGrey);
                });
            });
        }

        private void RenderContent(IContainer container)
        {
            container.PaddingTop(30).Column(col =>
            {
                col.Spacing(20);

                // TITRE DYNAMIQUE (Mesure + Dates)
                col.Item().AlignCenter().Column(c => {
                    c.Item().AlignCenter().Text(t => {
                        t.Span("Bilan de mesure ").FontSize(16).SemiBold();
                        t.Span(_courseName).FontSize(16).Bold().FontColor(AccentColor);
                    });
                    c.Item().AlignCenter().Text(t => {
                        t.Span("Période du ");
                        t.Span(_startDate.ToString("dd MMMM", _frenchCulture)).Bold();
                        t.Span(" au ");
                        t.Span(_endDate.ToString("dd MMMM yyyy", _frenchCulture)).Bold();
                    });
                    c.Item().PaddingTop(5).AlignCenter().Width(60).LineHorizontal(2).LineColor(AccentColor);
                });

                // BLOC NOM (Identique Attestation)
                col.Item().PaddingTop(10).AlignCenter().Background(LightGrey).Padding(12).Text($"{_stagiaire.Firstname} {_stagiaire.LastName}")
                    .FontSize(24).ExtraBold().FontColor(PrimaryColor).AlignCenter();

                // INFO TAUX
                col.Item().AlignCenter().Text(t => {
                    t.Span("Taux global de présence sur le trimestre : ").FontSize(10).FontColor(MediumGrey);
                    t.Span($"{_globalRate:F1}%").FontSize(10).Bold().FontColor(PrimaryColor);
                });

                // SECTIONS DE TEXTE
                col.Item().PaddingTop(10).Element(c => RenderSection(c, "Évaluation Qualitative & Progression", _evaluationText));
                col.Item().PaddingTop(10).Element(c => RenderSection(c, "Pistes de travail & Suivi", _followUpActions));

// --- BLOC NIVEAUX ACTUELS (AJOUTÉ ICI) ---
        col.Item().PaddingTop(20).Background(LightGrey).Padding(10).Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                c.Item().Text("Niveau actuel (Oral) :").FontSize(10).SemiBold().FontColor(MediumGrey);
                c.Item().Text(_stagiaire.OralLevel ?? "Non évalué").FontSize(14).Bold().FontColor(AccentColor);
            });

            row.RelativeItem().Column(c =>
            {
                c.Item().Text("Niveau actuel (Écrit) :").FontSize(10).SemiBold().FontColor(MediumGrey);
                c.Item().Text(_stagiaire.WrittenLevel ?? "Non évalué").FontSize(14).Bold().FontColor(AccentColor);
            });
});
                // SIGNATURE
                col.Item().PaddingTop(40).Row(row => {
                    row.RelativeItem().Text($"Lausanne, le {DateTime.Now.ToString("dd MMMM yyyy", _frenchCulture)}");
                    row.RelativeItem().AlignRight().Column(sig => {
                        sig.Item().Text("Direction / Responsable de formation").Bold();
                        sig.Item().PaddingTop(40).Width(150).LineHorizontal(1).LineColor(PrimaryColor);
                    });
                });
            });
        }

        private void RenderSection(IContainer container, string title, string content)
        {
            container.Column(col =>
            {
                col.Spacing(8);
                col.Item().Text(title).FontSize(14).Bold().FontColor(PrimaryColor);
                col.Item().LineHorizontal(1).LineColor(LightGrey);

                if (string.IsNullOrWhiteSpace(content))
                    col.Item().Text("Aucune information saisie pour cette section.").Italic().FontColor(MediumGrey).FontSize(10);
                else
                    col.Item().Text(content).FontSize(11).LineHeight(1.4f);
            });
        }
    }
}