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

        // Palette (Identique au modèle)
        private readonly CultureInfo _frenchCulture = new CultureInfo("fr-FR");
        private static readonly Color PrimaryColor = Color.FromHex("#2D3748");
        private static readonly Color AccentColor = Color.FromHex("#00B4B0");
        private static readonly Color MediumGrey = Color.FromHex("#718096");

        // Typographie
        private readonly TextStyle _titleStyle = TextStyle.Default.FontFamily("Arial").FontSize(24).Bold().FontColor(PrimaryColor);
        private readonly TextStyle _contentTextStyle = TextStyle.Default.FontSize(11).LineHeight(1.5f).FontColor(PrimaryColor);
        private readonly TextStyle _fallbackTextStyle = TextStyle.Default.Italic().FontColor(MediumGrey).FontSize(10);

        public BilanSessionDocument(
            ApplicationUser stagiaire,
            string evaluationText,
            string followUpActions,
            double globalRate,
            int year,
            string quarter,
            byte[] logoBytes)
        {
            _stagiaire = stagiaire;
            _evaluationText = evaluationText;
            _followUpActions = followUpActions;
            _globalRate = globalRate;
            _year = year;
            _quarter = quarter;
            _logo = logoBytes ?? Array.Empty<byte>();
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = $"Bilan Session - {_stagiaire.LastName} - {_year} T{_quarter}",
            Author = "Association Confluences"
        };

        public void Compose(IDocumentContainer document)
        {
            document.Page(page =>
            {
                page.Margin(50);
                page.DefaultTextStyle(TextStyle.Default.FontFamily("Calibri").FontSize(11).FontColor(PrimaryColor));
                page.Header().Element(RenderHeader);
                page.Content().Element(RenderContent);
                page.Footer().Element(RenderFooter);
            });
        }

        // --- EN-TÊTE (Identique BilanStageDocument) ---
        private void RenderHeader(IContainer container)
        {
            container.PaddingBottom(25).Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.ConstantItem(100).Height(45).PaddingTop(5).Element(logo =>
                    {
                        if (_logo.Length > 0) logo.Image(_logo).FitArea();
                        else logo.Text("Confluences").FontSize(16).FontColor(AccentColor).SemiBold();
                    });

                    row.RelativeItem().AlignRight().PaddingTop(8).Column(c =>
                    {
                        c.Spacing(3);
                        c.Item().Text("Association Confluences").SemiBold().FontSize(10).FontColor(PrimaryColor);
                        c.Item().Text("Chemin de Renens 52e, 1004 Lausanne").FontSize(9).FontColor(MediumGrey);
                        c.Item().Text("confluences.vaud@gmail.com").FontSize(9).FontColor(MediumGrey);
                    });
                });
                col.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            });
        }

        private void RenderContent(IContainer container)
        {
            container.Column(col =>
            {
                col.Spacing(30);

                // Titre
                col.Item().AlignCenter().Text($"BILAN DE SESSION ({_quarter})")
                    .Style(_titleStyle).LetterSpacing(0.4f);

                col.Item().Width(80).AlignCenter().PaddingTop(6).LineHorizontal(2).LineColor(AccentColor);

                // Nom
                col.Item().AlignCenter().PaddingTop(15).Text($"{_stagiaire.Firstname} {_stagiaire.LastName}")
                    .FontSize(28).Bold().FontColor(AccentColor);

                // Infos Session
                col.Item().PaddingTop(20).Element(RenderSessionInfo);

                // Sections Texte
                col.Item().PaddingTop(20).Element(c => RenderSection(c, "Évaluation Qualitative & Progression", _evaluationText));
                col.Item().PaddingTop(20).Element(c => RenderSection(c, "Pistes de travail & Suivi", _followUpActions));

                // Signature
                col.Item().PaddingTop(40).Element(RenderSignature);
            });
        }

        private void RenderSessionInfo(IContainer container)
        {
            container.AlignCenter().Column(col =>
            {
                col.Spacing(8);
                col.Item().Text(text =>
                {
                    text.Span("Année : ").SemiBold().FontSize(12).FontColor(PrimaryColor);
                    text.Span($"{_year}").FontSize(12);
                    text.Span("   |   Trimestre : ").SemiBold().FontSize(12).FontColor(PrimaryColor);
                    text.Span(_quarter).FontSize(12);
                });

                col.Item().Text(text =>
                {
                    text.Span("Taux de présence global : ").SemiBold().FontSize(12).FontColor(PrimaryColor);
                    text.Span($"{_globalRate:F1}%").FontSize(12).FontColor(AccentColor).SemiBold();
                });
            });
        }

        private void RenderSection(IContainer container, string title, string content)
        {
            container.Column(col =>
            {
                col.Spacing(10);
                col.Item().PaddingBottom(4).Text(title).FontSize(16).Bold().FontColor(PrimaryColor);
                col.Item().Width(50).PaddingBottom(8).LineHorizontal(1).LineColor(AccentColor.WithAlpha(100));

                if (string.IsNullOrWhiteSpace(content))
                    col.Item().Text("Aucune information saisie.").Style(_fallbackTextStyle);
                else
                    col.Item().Text(content).Style(_contentTextStyle);
            });
        }

        private void RenderSignature(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text($"Lausanne, le {DateTime.Now.ToString("dd MMMM yyyy", _frenchCulture)}")
                        .FontSize(10).FontColor(MediumGrey);
                });

                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Width(180).LineHorizontal(1).LineColor(AccentColor);
                    col.Item().PaddingTop(6).Text("La Direction / Le Formateur")
                        .SemiBold().FontSize(12).FontColor(PrimaryColor);
                });
            });
        }

        // --- PIED DE PAGE (Identique BilanStageDocument) ---
        private void RenderFooter(IContainer container)
        {
            container.PaddingTop(15).BorderTop(1).BorderColor(Colors.Grey.Lighten2).AlignCenter()
                .Text("Document généré par Confluences. Veuillez contacter l'administration pour toute vérification.")
                .FontSize(8).FontColor(Colors.Grey.Medium);
        }
    }
}