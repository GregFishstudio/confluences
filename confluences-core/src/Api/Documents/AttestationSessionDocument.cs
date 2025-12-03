using Confluences.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Api.Documents
{
    public class AttestationSessionDocument : IDocument
    {
        private readonly ApplicationUser _stagiaire;
        private readonly List<string> _ateliers;
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

        public AttestationSessionDocument(
            ApplicationUser stagiaire,
            List<string> ateliers,
            double globalRate,
            int year,
            string quarter,
            byte[] logoBytes)
        {
            _stagiaire = stagiaire;
            _ateliers = ateliers ?? new List<string>();
            _globalRate = globalRate;
            _year = year;
            _quarter = quarter;
            _logo = logoBytes ?? Array.Empty<byte>();
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = $"Attestation Session - {_stagiaire.LastName}",
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
                col.Spacing(25);

                // TITRE
                col.Item().AlignCenter().Text("ATTESTATION").Style(_titleStyle).LetterSpacing(0.5f);
                
                // SOUS-TITRE
                col.Item().AlignCenter().Text($"SESSION {_quarter} / {_year}")
                    .FontSize(14).SemiBold().FontColor(MediumGrey);

                col.Item().Width(80).AlignCenter().PaddingTop(6).LineHorizontal(2).LineColor(AccentColor);

                // PHRASE INTRODUCTIVE
                col.Item().PaddingTop(20).AlignCenter().Text("L'association Confluences atteste que")
                    .FontSize(12);

                // NOM STAGIAIRE
                col.Item().AlignCenter().PaddingVertical(5).Text($"{_stagiaire.Firstname} {_stagiaire.LastName}")
                    .FontSize(28).Bold().FontColor(AccentColor);

                // PHRASE SUITE
                col.Item().AlignCenter().Text(t => {
                    t.Span("a suivi les mesures d'insertion avec un taux de présence de ");
                    t.Span($"{_globalRate:F0}%").Bold().FontColor(PrimaryColor);
                    t.Span(".");
                });

                // LISTE DES ATELIERS
                col.Item().PaddingTop(30).Element(RenderAteliersList);

                // SIGNATURE
                col.Item().PaddingTop(50).Element(RenderSignature);
            });
        }

        private void RenderAteliersList(IContainer container)
        {
            container.Column(col =>
            {
                col.Spacing(10);

                col.Item().PaddingBottom(10).Text("Activités et Ateliers suivis :")
                    .FontSize(14).Bold().FontColor(PrimaryColor);
                
                // Barre de séparation
                col.Item().Width(40).PaddingBottom(10).LineHorizontal(1).LineColor(AccentColor);

                if (_ateliers != null && _ateliers.Any())
                {
                    foreach (var atelier in _ateliers)
                    {
                        col.Item().Row(row =>
                        {
                            // CORRECTION : On remplace Canvas/Paint par un simple bloc avec Background
                            row.ConstantItem(15).PaddingTop(8).Element(e => 
                            {
                                e.Height(4).Width(4).Background(AccentColor);
                            });
                            
                            row.RelativeItem().Text(atelier)
                                .FontSize(12).FontColor(PrimaryColor);
                        });
                    }
                }
                else
                {
                    col.Item().Text("Aucun atelier spécifique enregistré pour cette session.")
                        .Italic().FontColor(MediumGrey);
                }
            });
        }

        private void RenderSignature(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text($"Fait à Lausanne, le {DateTime.Now.ToString("dd MMMM yyyy", _frenchCulture)}")
                        .FontSize(10).FontColor(MediumGrey);
                });

                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Width(180).LineHorizontal(1).LineColor(AccentColor);
                    col.Item().PaddingTop(6).Text("La Direction")
                        .SemiBold().FontSize(12).FontColor(PrimaryColor);
                });
            });
        }

        private void RenderFooter(IContainer container)
        {
            container.PaddingTop(15).BorderTop(1).BorderColor(Colors.Grey.Lighten2).AlignCenter()
                .Text("Document généré par Confluences. Veuillez contacter l'administration pour toute vérification.")
                .FontSize(8).FontColor(Colors.Grey.Medium);
        }
    }
}