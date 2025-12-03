// File: /src/src/Api/Documents/BilanStageDocument.cs

using Confluences.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using System;
using System.Globalization;
using System.Linq;

namespace Api.Documents
{
    public class BilanStageDocument : IDocument
    {
        private readonly Stage _stage;
        private readonly ApplicationUser _stagiaire;
        private readonly byte[] _logo;

        // Palette professionnelle épurée (Harmonisée avec AttestationTrimestreDocument)
        private readonly CultureInfo _frenchCulture = new CultureInfo("fr-FR");
        private static readonly Color PrimaryColor = Color.FromHex("#2D3748");     // Gris anthracite (Utilisé comme MainColor)
        private static readonly Color AccentColor = Color.FromHex("#00B4B0");      // Vert-turquoise (Utilisé comme AccentColor)
        private static readonly Color MediumGrey = Color.FromHex("#718096");       // Gris moyen

        // Typographie harmonisée
        private readonly TextStyle _titleStyle = TextStyle.Default
            .FontFamily("Arial")
            .FontSize(24)
            .Bold()
            .FontColor(PrimaryColor);

        private readonly TextStyle _subtitleStyle = TextStyle.Default
            .FontFamily("Arial")
            .FontSize(16)
            .SemiBold()
            .FontColor(PrimaryColor);

        // Style pour le contenu des longs champs (Bilan, Remarque, Suivi)
        private readonly TextStyle _contentTextStyle = TextStyle.Default
            .FontSize(11)
            .LineHeight(1.5f)
            .FontColor(PrimaryColor); // Utilisation de PrimaryColor pour le texte principal

        // Style pour le texte de remplacement (fallback)
        private readonly TextStyle _fallbackTextStyle = TextStyle.Default
            .Italic()
            .FontColor(MediumGrey)
            .FontSize(10);

        public BilanStageDocument(
            Stage stage,
            ApplicationUser stagiaire,
            byte[] logoBytes)
        {
            _stage = stage;
            _stagiaire = stagiaire;
            _logo = logoBytes ?? Array.Empty<byte>();
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = $"Bilan de stage - {_stagiaire.Firstname} {_stagiaire.LastName}",
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
                        if (_logo.Length > 0)
                        {
                            logo.Image(_logo).FitArea();
                        }
                        else
                        {
                            logo.Text("Confluences").FontSize(16).FontColor(AccentColor).SemiBold();
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

                col.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            });
        }

        private void RenderContent(IContainer container)
        {
            container.Column(col =>
            {
                col.Spacing(30);

                // --- 1. Titre ---
                col.Item().AlignCenter().Text("BILAN DE STAGE")
                    .Style(_titleStyle)
                    .LetterSpacing(0.4f);

                // Ligne de séparation courte et accentuée
                col.Item().Width(60).AlignCenter().PaddingTop(6)
                    .LineHorizontal(2)
                    .LineColor(AccentColor);
                
                // --- 2. Nom du Stagiaire ---
                col.Item().AlignCenter().PaddingTop(15).Text($"{_stagiaire.Firstname} {_stagiaire.LastName}")
                    .FontSize(28)
                    .Bold()
                    .FontColor(AccentColor); // Nom du stagiaire en couleur d'accent

                // --- 3. Informations du Stage (Style Aéré) ---
                col.Item().PaddingTop(20).Element(RenderStageInfoBlockAere);
                
                // --- 4. Bilan (Texte principal) ---
                col.Item().PaddingTop(20).Element(RenderBilanText);

                // --- 5. Remarques et Suivi ---
                col.Item().PaddingTop(20).Element(RenderRemarquesAndSuivi);

                // --- 6. Signature (déplacée dans RenderContent pour plus de contrôle) ---
                col.Item().PaddingTop(40).Element(RenderSignature);
            });
        }

        private void RenderStageInfoBlockAere(IContainer container)
        {
            container.AlignCenter().Column(col =>
            {
                col.Spacing(8);

                // Type d'Activité & Métier sur une seule ligne centrée
                col.Item().Text(text =>
                {
                    text.Span("Activité : ").SemiBold().FontSize(12).FontColor(PrimaryColor);
                    text.Span(_stage.TypeIntershipActivity?.Nom ?? "Non spécifié").FontSize(12);
                    
                    text.Span("   | Métier : ").SemiBold().FontSize(12).FontColor(PrimaryColor);
                    text.Span(_stage.TypeMetier?.Libelle ?? "Non spécifié").FontSize(12);
                });

                // Période & Taux sur une seule ligne centrée
                col.Item().Text(text =>
                {
                    var debut = _stage.Debut.ToString("dd MMMM yyyy", _frenchCulture);
                    var fin = _stage.Fin.HasValue ? _stage.Fin.Value.ToString("dd MMMM yyyy", _frenchCulture) : "En cours";
                    
                    text.Span("Période : ").SemiBold().FontSize(12).FontColor(PrimaryColor);
                    text.Span($"Du {debut} au {fin}").FontSize(12);

                    text.Span("   | Taux : ").SemiBold().FontSize(12).FontColor(PrimaryColor);
                    text.Span(_stage.TypeStage?.Nom ?? "N/A").FontSize(12).FontColor(AccentColor).SemiBold(); 
                });

                // Entreprise (si spécifiée)
                if (_stage.Entreprise != null)
                {
                    col.Item().Text(text =>
                    {
                        text.Span("Entreprise : ").SemiBold().FontSize(12).FontColor(PrimaryColor);
                        text.Span(_stage.Entreprise.Nom).FontSize(12);
                    });
                }
            });
        }

        private void RenderSection(IContainer container, string title, string content)
        {
            container.Column(col =>
            {
                col.Spacing(10); 

                // Titre de section stylisé
                // FIX CS1929: Apply PaddingBottom to the col.Item() container
                col.Item().PaddingBottom(4).Text(title)
                    .FontSize(16)
                    .Bold()
                    .FontColor(PrimaryColor);
                
                // Ligne de séparation subtile
                // FIX CS0023: Apply PaddingBottom to the col.Item() container *before* the LineHorizontal call
                col.Item().Width(50).PaddingBottom(8)
                    .LineHorizontal(1)
                    .LineColor(AccentColor.WithAlpha(100));

                if (string.IsNullOrWhiteSpace(content))
                {
                    col.Item().Text($"Aucun(e) {title.ToLower()} n'a été rédigé(e) pour le moment.")
                        .Style(_fallbackTextStyle);
                }
                else
                {
                    col.Item().Text(content)
                        .Style(_contentTextStyle);
                }
            });
        }

        private void RenderBilanText(IContainer container)
        {
            RenderSection(container, "Bilan du stage", _stage.Bilan);
        }
        
        private void RenderRemarquesAndSuivi(IContainer container)
        {
             container.Column(col =>
             {
                 col.Spacing(25); // Espacement entre Remarques et Suivi

                 // Remarques
                 col.Item().Element(c => RenderSection(c, "Remarques générales", _stage.Remarque));
                 
                 // Suivi
                 col.Item().Element(c => RenderSection(c, "Suivi (Actions futures)", _stage.ActionSuivi));
             });
        }

        private void RenderSignature(IContainer container)
        {
            container.Row(row =>
            {
                // Date et lieu
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text($"Lausanne, le {DateTime.Now.ToString("dd MMMM yyyy", _frenchCulture)}")
                        .FontSize(10)
                        .FontColor(MediumGrey);
                });

                // Signature
                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Width(180).LineHorizontal(1).LineColor(AccentColor);
                    col.Item().PaddingTop(6).Text("La Direction / Créateur-trice") 
                        .SemiBold()
                        .FontSize(12)
                        .FontColor(PrimaryColor);
                });
            });
        }

        private void RenderFooter(IContainer container)
        {
            container
                .PaddingTop(15)
                .BorderTop(1)
                .BorderColor(Colors.Grey.Lighten2)
                .AlignCenter()
                .Text("Document généré par Confluences. Veuillez contacter l'administration pour toute vérification.")
                .FontSize(8)
                .FontColor(Colors.Grey.Medium);
        }
    }
}