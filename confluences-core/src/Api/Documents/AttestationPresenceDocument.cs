using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Confluences.Domain.Entities;
using Api.Models.Dto.Attendance;

namespace Api.Documents
{
    public class AttestationPresenceDocument : IDocument
    {
        private readonly DateTime _startDate;
        private readonly DateTime _endDate;
        private readonly List<AttendanceEntryDto> _presences;
        private readonly ApplicationUser _stagiaire;
        private readonly byte[] _logo;

        // Culture française pour le formatage des dates
        private readonly CultureInfo _frenchCulture = new CultureInfo("fr-FR");

        // Styles et Couleurs
        private static readonly Color PrimaryColor = Color.FromHex("#2D3748");
        private static readonly Color AccentColor = Color.FromHex("#00B4B0");
        private static readonly Color MediumGrey = Color.FromHex("#718096");
        
        private readonly TextStyle _titleStyle = TextStyle.Default
            .FontFamily("Arial").FontSize(24).Bold().FontColor(PrimaryColor);
        private readonly TextStyle _contentTextStyle = TextStyle.Default
            .FontFamily("Arial").FontSize(11).FontColor(PrimaryColor).LineHeight(1.5f);

        // Constructeur
        public AttestationPresenceDocument(
            DateTime startDate,
            DateTime endDate,
            List<AttendanceEntryDto> presences,
            List<ApplicationUser> stagiaires,
            byte[] logo = null)
        {
            _startDate = startDate.Date; // Assurer minuit
            _endDate = endDate.Date;     // Assurer minuit
            
            _presences = presences ?? new List<AttendanceEntryDto>();
            
            // Vérification robuste du stagiaire
            _stagiaire = stagiaires?.FirstOrDefault() 
                ?? throw new ArgumentException("La liste de stagiaires est vide ou nulle.");
            
            _logo = logo ?? Array.Empty<byte>();
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = $"Attestation de présence - {_stagiaire.LastName} {_stagiaire.Firstname}",
            Author = "Association Confluences"
        };

        public void Compose(IDocumentContainer document)
        {
            document.Page(page =>
            {
                page.Margin(50);
                page.DefaultTextStyle(_contentTextStyle);
                page.Size(PageSizes.A4);

                page.Header().Element(RenderHeader);
                page.Content().Element(RenderContent);
                page.Footer().Element(RenderFooter);
            });
        }

        private void RenderHeader(IContainer container)
        {
            container.PaddingBottom(15).Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.ConstantItem(100).Height(45).Element(logo =>
                    {
                        if (_logo.Length > 0) logo.Image(_logo).FitArea();
                        else logo.Text("Confluences").FontSize(16).FontColor(AccentColor).SemiBold();
                    });

                    row.RelativeItem().AlignRight().Column(c =>
                    {
                        c.Spacing(3);
                        c.Item().Text("Association Confluences").SemiBold().FontSize(10).FontColor(PrimaryColor);
                        c.Item().Text("Chemin de Renens 52e, 1004 Lausanne").FontSize(9).FontColor(MediumGrey);
                        c.Item().Text("confluences.vaud@gmail.com").FontSize(9).FontColor(MediumGrey);
                    });
                });

                col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            });
        }

        private void RenderContent(IContainer container)
        {
            // --- LOGIQUE DE FILTRAGE CRITIQUE ---
            // 1. Convertir l'ID du stagiaire en chaîne pour une comparaison fiable.
            string requiredStagiaireId = _stagiaire.Id.ToString();

            // 2. Filtrer les présences pour l'ID et la période spécifiques.
            var filteredPresences = _presences
                .Where(p => 
                    // Comparaison d'ID (doit être string == string)
                    p.StagiaireId != null 
                    && p.StagiaireId.Equals(requiredStagiaireId, StringComparison.OrdinalIgnoreCase) 
                    // Filtrage par date (utilisation de .Date pour ignorer l'heure)
                    && p.Date.Date >= _startDate 
                    && p.Date.Date <= _endDate) 
                .ToList();

            // --- CALCULS ---
            int totalDays = filteredPresences.Count;
            int presents = filteredPresences.Count(p => p.Statut?.ToLower() == "p");
            int absents = filteredPresences.Count(p => p.Statut?.ToLower() == "a");
            int excuses = filteredPresences.Count(p => p.Statut?.ToLower() == "e");
            int retards = filteredPresences.Count(p => p.Statut?.ToLower() == "r");
            double attendanceRate = totalDays > 0 ? (double)presents / totalDays * 100 : 0;
            
            // --- COMPOSITION DU DOCUMENT ---
            container.Column(col =>
            {
                col.Spacing(20);

                // Titre principal
                col.Item().AlignCenter().Text("ATTESTATION MENSUELLE DE PRÉSENCE").Style(_titleStyle);
                col.Item().AlignCenter().Text($"{_stagiaire.Firstname} {_stagiaire.LastName}")
                    .FontSize(18).SemiBold().FontColor(AccentColor);

                // Période
                string startDateFormatted = _startDate.ToString("dd MMMM yyyy", _frenchCulture);
                string endDateFormatted = _endDate.ToString("dd MMMM yyyy", _frenchCulture);
                col.Item().AlignCenter().Text($"Période : {startDateFormatted} - {endDateFormatted}")
                    .FontSize(12).SemiBold();

                // Tableau Résumé des statistiques
                col.Item().PaddingTop(20).Border(1).BorderColor(Colors.Grey.Lighten3).Padding(10).Column(summary =>
                {
                    summary.Spacing(8);
                    
                    summary.Item().Text(text =>
                    {
                        text.Span("Total Jours Compilés : ").SemiBold();
                        text.Span($"{totalDays} jours");
                    });

                    // Mise en évidence des statuts
                    summary.Item().Text(text =>
                    {
                        text.Span("Jours présents : ").SemiBold().FontColor(Colors.Green.Darken2);
                        text.Span(presents.ToString()).FontColor(Colors.Green.Darken2).SemiBold();
                    });
                    
                    summary.Item().Text(text =>
                    {
                        text.Span("Jours absents : ").SemiBold().FontColor(Colors.Red.Darken2);
                        text.Span(absents.ToString()).FontColor(Colors.Red.Darken2).SemiBold();
                    });

                    summary.Item().Text(text =>
                    {
                        text.Span("Jours excusés : ").SemiBold().FontColor(Colors.Orange.Darken2);
                        text.Span(excuses.ToString()).FontColor(Colors.Orange.Darken2).SemiBold();
                    });

                    summary.Item().Text(text =>
                    {
                        text.Span("Retards : ").SemiBold();
                        text.Span(retards.ToString());
                    });
                    
                    summary.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    summary.Item().Text(text =>
                    {
                        text.Span("Taux de présence : ").FontSize(14).Bold().FontColor(PrimaryColor);
                        text.Span($"{attendanceRate:F1}%").FontSize(14).Bold().FontColor(AccentColor);
                    });
                });

                // Détails par jour (optionnel, pour vérification)
                col.Item().PaddingTop(20).Text("Détails des enregistrements journaliers :").SemiBold();
                col.Item().Table(table =>
                {
                    // Colonnes
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.ConstantColumn(100);
                    });

                    // En-tête du tableau
                    table.Header(header =>
                    {
                        header.Cell().BorderBottom(1).BorderColor(AccentColor).PaddingBottom(5).Text("Date").SemiBold();
                        header.Cell().BorderBottom(1).BorderColor(AccentColor).PaddingBottom(5).Text("Statut").SemiBold();
                    });

                    // Lignes de données
                    foreach (var p in filteredPresences.OrderBy(p => p.Date))
                    {
                        string status = p.Statut?.ToLower() switch
                        {
                            "p" => "Présent",
                            "a" => "Absent",
                            "e" => "Excusé",
                            "r" => "Retard",
                            _ => "Non spécifié"
                        };
                        
                        table.Cell().PaddingVertical(3).Text(p.Date.ToString("dd MMMM yyyy", _frenchCulture));
                        table.Cell().PaddingVertical(3).Text(status).FontColor(
                            p.Statut?.ToLower() == "p" ? Colors.Green.Darken2 :
                            p.Statut?.ToLower() == "a" ? Colors.Red.Darken2 : 
                            Colors.Black);
                    }
                });

                // Signature (Utiliser RenderSignature)
                col.Item().PaddingTop(30).Element(RenderSignature);
            });
        }

        private void RenderSignature(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text($"Fait à Lausanne, le {DateTime.Now.ToString("dd MMMM yyyy", _frenchCulture)}")
                        .FontSize(10)
                        .FontColor(MediumGrey);
                });

                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Width(180).LineHorizontal(1).LineColor(AccentColor);
                    col.Item().PaddingTop(6).Text("La Direction").SemiBold().FontSize(12).FontColor(PrimaryColor);
                });
            });
        }

        private void RenderFooter(IContainer container)
        {
            container.PaddingTop(15).BorderTop(1).BorderColor(Colors.Grey.Lighten2).AlignCenter()
                .Text("Document généré par Confluences. Veuillez contacter l'administration pour toute vérification.")
                .FontSize(8).FontColor(MediumGrey);
        }
    }
}