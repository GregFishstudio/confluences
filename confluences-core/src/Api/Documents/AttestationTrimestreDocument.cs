using System;
using System.Collections.Generic;
using System.Globalization;
using Confluences.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Api.Documents
{
    public class AttestationTrimestreDocument : IDocument
    {
        private readonly ApplicationUser _stagiaire;
        private readonly IReadOnlyList<Stage> _stages;
        private readonly int _year;
        private readonly string _trimestre;
        private readonly byte[] _logo;

        // Palette de couleurs moderne - CONVERTIE EN TYPE COLOR POUR FIXER CS0172
        private static readonly CultureInfo FrenchCulture = new CultureInfo("fr-FR");
        // Les couleurs sont désormais des objets Color
        private static readonly Color PrimaryColor = Color.FromHex("#2D3748");     // Gris anthracite
        private static readonly Color AccentColor = Color.FromHex("#00B4B0");      // Vert-turquoise
        private static readonly Color LightGrey = Color.FromHex("#F7FAFC");        // Gris très pâle
        private static readonly Color MediumGrey = Color.FromHex("#718096");       // Gris moyen
        private static readonly Color DarkGrey = Color.FromHex("#4A5568");         // Gris foncé

        public AttestationTrimestreDocument(
            ApplicationUser stagiaire,
            List<Stage> stages,
            int year,
            string trimestre,
            byte[] logoBytes)
        {
            _stagiaire = stagiaire ?? throw new ArgumentNullException(nameof(stagiaire));
            _stages = stages ?? new List<Stage>();
            _year = year;
            _trimestre = string.IsNullOrWhiteSpace(trimestre) ? "" : trimestre;
            _logo = logoBytes ?? Array.Empty<byte>();
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = $"Attestation - {_stagiaire.Firstname} {_stagiaire.LastName} {_trimestre} {_year}",
            Author = "Association Confluences"
        };

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(50);
                page.DefaultTextStyle(TextStyle.Default.FontFamily("Calibri").FontSize(11));
                page.Size(PageSizes.A4);

                page.Header().Element(RenderHeader);
                page.Content().Element(RenderContent);
                page.Footer().Element(RenderFooter);
            });
        }

        private void RenderHeader(IContainer container)
        {
            container.PaddingBottom(25).Column(col =>
            {
                col.Item().Row(row =>
                {
                    // Logo positionné à gauche avec espace respirant
                    row.ConstantItem(100).Height(45).PaddingTop(5).Element(logo =>
                    {
                        if (_logo.Length > 0)
                        {
                            logo.Image(_logo).FitArea();
                        }
                        else
                        {
                            logo
                                .AlignLeft()
                                .AlignMiddle()
                                .Text("Confluences")
                                .FontSize(16)
                                .SemiBold()
                                .FontColor(PrimaryColor);
                        }
                    });

                    // Informations de contact alignées à droite
                    row.RelativeItem().AlignRight().PaddingTop(8).Column(c =>
                    {
                        c.Spacing(3);
                        c.Item().Text("Association Confluences")
                            .SemiBold()
                            .FontSize(10)
                            .FontColor(PrimaryColor);
                        c.Item().Text("Chemin de Renens 52e, 1004 Lausanne")
                            .FontSize(9)
                            .FontColor(MediumGrey);
                        c.Item().Text("confluences.vaud@gmail.com")
                            .FontSize(9)
                            .FontColor(MediumGrey);
                    });
                });

                // Ligne de séparation fine
                col.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            });
        }

        private void RenderContent(IContainer container)
        {
            container.Column(col =>
            {
                col.Spacing(25);

                // Titre principal avec ligne d'accent
                col.Item().AlignCenter().Column(titleCol =>
                {
                    titleCol.Item().Text("ATTESTATION TRIMESTRIELLE")
                        .FontSize(24)
                        .Bold()
                        .FontColor(PrimaryColor)
                        .LetterSpacing(0.4f);

                    // Ligne de séparation accentée
                    titleCol.Item().PaddingTop(6).Width(60).AlignCenter()
                        .LineHorizontal(2)
                        .LineColor(AccentColor);

                    titleCol.Item().PaddingTop(8).Text($"{_trimestre} {_year}")
                        .FontSize(18)
                        .SemiBold()
                        .FontColor(AccentColor);
                });

                // Section du stagiaire - point focal
                col.Item().AlignCenter().Column(stagiaireCol =>
                {
                    stagiaireCol.Item().Text("Attribuée à")
                        .FontSize(12)
                        .FontColor(MediumGrey);

                    stagiaireCol.Item().PaddingTop(8).Text($"{_stagiaire.Firstname} {_stagiaire.LastName}")
                        .FontSize(28)
                        .Bold()
                        .FontColor(AccentColor)
                        .LetterSpacing(0.5f);
                });

                col.Item().Element(RenderMainContent);
            });
        }

        private void RenderMainContent(IContainer container)
        {
            container.Column(col =>
            {
                // Introduction épurée
                col.Item().PaddingVertical(20).Text(text =>
                {
                    text.Span("Ceci certifie que ").FontSize(11).FontColor(DarkGrey);
                    text.Span($"{_stagiaire.Firstname} {_stagiaire.LastName}")
                        .SemiBold()
                        .FontColor(AccentColor);
                    text.Span(" a participé aux activités de formation suivantes :")
                        .FontSize(11)
                        .FontColor(DarkGrey);
                });

                // Tableau moderne
                if (_stages.Count > 0)
                {
                    col.Item().Element(RenderModernTable);
                }
                else
                {
                    col.Item().Element(RenderEmptyState);
                }

                // Bloc récapitulatif
                col.Item().PaddingTop(25).Element(RenderSummaryBlock);

                // Signature professionnelle
                col.Item().PaddingTop(40).Element(RenderSignature);
            });
        }

        private void RenderModernTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4);   // Activité
                    columns.RelativeColumn(3);   // Entreprise
                    columns.RelativeColumn(3);   // Période
                });

                // En-tête moderne avec fond subtil
                table.Header(header =>
                {
                    header.Cell().Element(ModernHeaderCell("ACTIVITÉ"));
                    header.Cell().Element(ModernHeaderCell("ENTREPRISE"));
                    header.Cell().Element(ModernHeaderCell("PÉRIODE"));
                });

                // Contenu du tableau - FIX: LightGrey est maintenant de type Color
                for (int i = 0; i < _stages.Count; i++)
                {
                    var stage = _stages[i];
                    var isEvenRow = i % 2 == 0;
                    var backgroundColor = isEvenRow ? Colors.White : LightGrey; // FIX CS0172

                    // Activité
                    table.Cell().Element(cell => cell
                        .Background(backgroundColor)
                        .Padding(12)
                        .BorderBottom(1)
                        .BorderColor(Colors.Grey.Lighten3)
                        .Text(stage.TypeIntershipActivity?.Nom ?? "Activité inconnue")
                        .FontSize(10)
                        .FontColor(DarkGrey));

                    // Entreprise
                    table.Cell().Element(cell => cell
                        .Background(backgroundColor)
                        .Padding(12)
                        .BorderBottom(1)
                        .BorderColor(Colors.Grey.Lighten3)
                        .Text(stage.Entreprise?.Nom ?? "N/A")
                        .FontSize(10)
                        .FontColor(DarkGrey));

                    // Période
                    var debut = stage.Debut.ToString("dd MMM yyyy", FrenchCulture);
                    var fin = stage.Fin?.ToString("dd MMM yyyy", FrenchCulture) ?? "En cours";
                    
                    table.Cell().Element(cell => cell
                        .Background(backgroundColor)
                        .Padding(12)
                        .BorderBottom(1)
                        .BorderColor(Colors.Grey.Lighten3)
                        .Text($"Du {debut}\nau {fin}")
                        .FontSize(9)
                        .FontColor(MediumGrey));
                }
            });
        }

        private Action<IContainer> ModernHeaderCell(string label)
        {
            return container => container
                .Padding(12)
                .Background(LightGrey)
                .BorderBottom(1)
                .BorderColor(AccentColor)
                .Text(label)
                .SemiBold()
                .FontSize(10)
                .FontColor(PrimaryColor)
                .LetterSpacing(0.3f);
        }

        private void RenderEmptyState(IContainer container)
        {
            container
                .Padding(30)
                .Background(LightGrey)
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .AlignCenter()
                .Text("Aucune activité enregistrée pour ce trimestre")
                .Italic()
                .FontColor(MediumGrey)
                .FontSize(11);
        }

        private void RenderSummaryBlock(IContainer container)
        {
            container
                .Background(LightGrey)
                .Padding(20)
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Column(col =>
                {
                    col.Item().Text("RÉCAPITULATIF DU TRIMESTRE")
                        .SemiBold()
                        .FontSize(12)
                        .FontColor(PrimaryColor)
                        .LetterSpacing(0.3f);

                    col.Item().PaddingTop(12).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(text =>
                            {
                                text.Span("• Nombre total d'activités : ").SemiBold().FontColor(DarkGrey);
                                text.Span(_stages.Count.ToString()).SemiBold().FontColor(AccentColor);
                            });
                        });

                        if (_stages.Count > 0)
                        {
                            var first = _stages[0];
                            var last = _stages[_stages.Count - 1];
                            var start = first.Debut.ToString("dd MMMM yyyy", FrenchCulture);
                            var end = last.Fin?.ToString("dd MMMM yyyy", FrenchCulture) ?? "présent";

                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().Text(text =>
                                {
                                    text.Span("• Période : ").SemiBold().FontColor(DarkGrey);
                                    text.Span($"Du {start} au {end}").FontColor(MediumGrey);
                                });
                            });
                        }
                    });
                });
        }

        private void RenderSignature(IContainer container)
        {
            container.Row(row =>
            {
                // Date et lieu
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text($"Lausanne, le {DateTime.Now.ToString("dd MMMM yyyy", FrenchCulture)}")
                        .FontSize(10)
                        .FontColor(MediumGrey);
                });

                // Signature
                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Width(180).LineHorizontal(1).LineColor(AccentColor);
                    col.Item().PaddingTop(6).Text("La Direction")
                        .SemiBold()
                        .FontSize(12)
                        .FontColor(PrimaryColor);
                    
                    col.Item().PaddingTop(4).Column(directors =>
                    {
                        directors.Item().Text("Guillaume Rouet").FontSize(9).FontColor(MediumGrey);
                        directors.Item().Text("Salvatore De Lucia").FontSize(9).FontColor(MediumGrey);
                    });
                });
            });
        }

        private void RenderFooter(IContainer container)
        {
            container.PaddingTop(15).BorderTop(1).BorderColor(Colors.Grey.Lighten2).AlignCenter()
                .Text("Association Confluences • www.confluences.ch • confluences.vaud@gmail.com • +41 77 520 02 84")
                .FontSize(8)
                .FontColor(Colors.Grey.Medium);
        }
    }
}