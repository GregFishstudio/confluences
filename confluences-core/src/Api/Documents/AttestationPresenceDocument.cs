// File: Api/Documents/AttestationPresenceDocument.cs

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using Api.Models.Dto.Attendance; // Utilisation temporaire du DTO pour le POC

namespace Api.Documents
{
    // Note: Vous devrez adapter ApplicationUser et Stage si nécessaire
    public class AttestationPresenceDocument : IDocument
    {
        private readonly DateTime _startDate;
        private readonly DateTime _endDate;
        private readonly List<AttendanceEntryDto> _presences;
        private readonly List<object> _stagiaires; // Utiliser la vraie entité ApplicationUser
        private readonly byte[] _logo; // Optionnel

        // Styles harmonisés
        private static readonly Color PrimaryColor = Color.FromHex("#2D3748");
        private static readonly Color AccentColor = Color.FromHex("#00B4B0");
        private static readonly Color LightBackground = Color.FromHex("#F7FAFC");
        private static readonly CultureInfo FrenchCulture = new CultureInfo("fr-FR");

        private readonly TextStyle _titleStyle = TextStyle.Default
            .FontSize(24)
            .Bold()
            .FontColor(PrimaryColor);

        public AttestationPresenceDocument(
            DateTime startDate,
            DateTime endDate,
            List<AttendanceEntryDto> presences,
            List<object> stagiaires,
            byte[] logo = null)
        {
            _startDate = startDate;
            _endDate = endDate;
            _presences = presences;
            _stagiaires = stagiaires;
            _logo = logo ?? Array.Empty<byte>();
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = $"Attestation de présence du {_startDate.ToShortDateString()} au {_endDate.ToShortDateString()}",
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
                page.Footer().Text(x =>
                {
                    x.AlignCenter();
                    x.Span($"Document généré le {DateTime.Now.ToString("dd MMMM yyyy", FrenchCulture)}").FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        }

        private void RenderHeader(IContainer container)
        {
            // Mettre en page l'en-tête (Logo, Coordonnées) comme dans les autres documents
            container.PaddingBottom(15).LineHorizontal(1).LineColor(AccentColor);
        }

        private void RenderContent(IContainer container)
        {
            container.Column(col =>
            {
                col.Spacing(20);

                // Titre
                col.Item().AlignCenter().Text("ATTESTATION DE PRÉSENCE").Style(_titleStyle);

                // Période
                col.Item().PaddingTop(10).Text(text =>
                {
                    text.Span("Période couverte : ").SemiBold();
                    text.Span($"Du {_startDate.ToString("dd MMMM yyyy", FrenchCulture)} au {_endDate.ToString("dd MMMM yyyy", FrenchCulture)}").FontColor(AccentColor);
                });
                
                // Synthèse (Exemple : nombre total d'absences)
                col.Item().PaddingTop(10).Border(1).BorderColor(LightBackground).Background(LightBackground).Padding(10).Text(text =>
                {
                    var totalAbsences = _presences.FindAll(p => p.Statut == "a").Count;
                    text.Span("Synthèse : ").SemiBold();
                    text.Span($"Nombre total d'absences enregistrées (Non participation) : ").FontColor(PrimaryColor);
                    text.Span($"{totalAbsences} jours").Bold().FontColor(Colors.Red.Medium);
                });

                // Tableau détaillé par stagiaire (À développer pour refléter la grille)
                col.Item().PaddingTop(20).Text("Détail des statuts par jour pour la période:").SemiBold();
                col.Item().Table(table =>
                {
                    // Colonnes de stagiaires, jours, etc. (Simplifié ici)
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(150); // Nom
                        columns.RelativeColumn(1);   // Statut global
                    });
                    
                    table.Header().Cell().Background(PrimaryColor).Padding(5).Text("Stagiaire").FontColor(Colors.White);
                    table.Header().Cell().Background(PrimaryColor).Padding(5).Text("Statut").FontColor(Colors.White);

                    // Boucle sur les stagiaires...
                    table.Cell().Border(1).Padding(5).Text("Stagiaire A");
                    table.Cell().Border(1).Padding(5).Text("Statut détaillé...");
                });
            });
        }
    }
}