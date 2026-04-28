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
        private readonly string _socialInsertionText;
        private readonly string _digitalSkillsText;
        private readonly string _individualAccompanyingText;
        private readonly string _homeVisitsText;
        private readonly double _globalRate;
        private readonly int _year;
        private readonly string _quarter;
        private readonly byte[] _logo;
        private readonly string _courseName;
        private readonly DateTime _startDate;
        private readonly DateTime _endDate;
        private readonly CultureInfo _fr = new CultureInfo("fr-FR");
        // Détection de la mesure
        private bool IsTelecours => _courseName.ToLower().Contains("télécours") || _courseName.ToLower().Contains("telecours");

        // Palette & Culture
        private readonly CultureInfo _frenchCulture = new CultureInfo("fr-FR");
        private static readonly Color PrimaryColor = Colors.Black;
        private static readonly Color AccentColor = Color.FromHex("#4A90E2"); // Bleu pour les sous-titres
        private static readonly Color LightGrey = Color.FromHex("#F0F0F0");

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
            string courseName,
            DateTime startDate,
            DateTime endDate,
            string socialInsertionText = "",
            string digitalSkillsText = "",
            string individualAccompanyingText = "",
            string homeVisitsText = "")
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
            
            _socialInsertionText = socialInsertionText;
            _digitalSkillsText = digitalSkillsText;
            _individualAccompanyingText = individualAccompanyingText;
            _homeVisitsText = homeVisitsText;
        }

        public void Compose(IDocumentContainer document)
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0.75f, Unit.Inch);
                page.DefaultTextStyle(x => x.FontFamily("Helvetica").FontSize(11).FontColor(PrimaryColor));

                page.Header().Element(RenderHeader);
                page.Content().Element(RenderContent);
                
                // Ajout du pied de page pour la signature
            });
        }

        private void RenderHeader(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    // Logo à gauche
                    row.RelativeItem().Column(c =>
                    {
                        if (_logo.Length > 0)
                            c.Item().Height(60).Image(_logo).FitHeight();
                        else
                            c.Item().Text("CONFLUENCES").FontSize(20).ExtraBold().FontColor(AccentColor);
                    });

                    // Coordonnées à droite
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

        private void RenderContent(IContainer container)
        {
            container.PaddingTop(20).Column(col =>
            {
                col.Spacing(15);

                // Titre : Bilan de mesure + Dates
                col.Item().AlignCenter().Text(t =>
                {
                    t.Span("Bilan de mesure ").FontSize(14).FontColor(AccentColor);
                    t.Span(_courseName).FontSize(14).Bold().FontColor(AccentColor);
                    t.Span($" ({_startDate:dd.MM} - {_endDate:dd.MM.yyyy})").FontSize(14).FontColor(AccentColor);
                });

                // Participant
                col.Item().PaddingTop(10).Text(t =>
                {
                    t.Span("Participant-e : ").Underline();
                    t.Span($" {_stagiaire.Firstname?.ToUpper()} {_stagiaire.LastName}").FontSize(16).Bold();
                });

                // 1. Évaluation Qualitative
                col.Item().Element(c => RenderSection(c, $"Cours de français (niveau {_stagiaire.OralLevel})", _evaluationText));

                // 2. Blocs Spécifiques (Conditionnels)
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
                        col.Item().Element(c => RenderSection(c, "Insertion sociale", _socialInsertionText));
                    
                    if (!string.IsNullOrWhiteSpace(_digitalSkillsText))
                        col.Item().Element(c => RenderSection(c, "Utilisation de l’informatique", _digitalSkillsText));
                    
                    if (!string.IsNullOrWhiteSpace(_individualAccompanyingText))
                        col.Item().Element(c => RenderSection(c, "Accompagnement individualisé", _individualAccompanyingText));
                        
                    if (!string.IsNullOrWhiteSpace(_followUpActions))
                    col.Item().Element(c => RenderSection(c, "Perspectives et objectifs", _followUpActions));
                }

                // 3. Perspectives / Synthèse
                

                // 4. Niveau actuel
                col.Item().PaddingTop(10).Text(t =>
{
    t.Span("Niveau actuel : ").FontSize(11).FontColor(Colors.Grey.Darken2);
    
    // Bloc Oral
    t.Span(" Oral ").FontSize(11).FontColor(AccentColor).Italic();
    t.Span(_stagiaire.OralLevel ?? "A1.1").FontSize(12).Bold().FontColor(AccentColor);
    
    t.Span("  |  ").FontSize(11).FontColor(Colors.Grey.Lighten1); // Séparateur plus élégant
    
    // Bloc Écrit
    t.Span("Écrit ").FontSize(11).FontColor(AccentColor).Italic();
    t.Span(_stagiaire.WrittenLevel ?? "A1.1").FontSize(12).Bold().FontColor(AccentColor);
});
            });
        }

        private void RenderSection(IContainer container, string title, string content)
        {
            container.Column(col =>
            {
                col.Item().PaddingBottom(5).Text(title).FontSize(12).FontColor(AccentColor).Italic();
                
                col.Item().Text(string.IsNullOrWhiteSpace(content) ? "Non renseigné" : content)
                   .FontSize(10)
                   .LineHeight(1.2f)
                   .Justify();

                col.Item().PaddingVertical(5).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
            });
        }
    }
}