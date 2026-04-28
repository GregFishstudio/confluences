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
    // Cette classe aide à structurer les données à l'intérieur du document
    public class ActivityEntry
    {
        public string Nom { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // 'atelier' ou 'acco'
    }

    public class AttestationSessionDocument : IDocument
    {
        private readonly ApplicationUser _stagiaire;
        private readonly string _courseName;
        private readonly List<string> _rawActivities;
        private readonly double _tauxParticipation;
        private readonly byte[] _logoBytes;
        private readonly byte[] _signatureBytes;
        private readonly string _oralLevel;
        private readonly string _writtenLevel;
        private readonly DateTime _startDate;
        private readonly DateTime _endDate;
        private readonly string _trimestre;
        private readonly int _year;
        private readonly CultureInfo _fr = new CultureInfo("fr-FR");

        public AttestationSessionDocument(
            ApplicationUser stagiaire, 
            string courseName, 
            string oralLevel, 
            string writtenLevel, 
            List<string> ateliersSuivis, // On repasse en List<string> pour corriger l'erreur CS1503
            double tauxParticipation, 
            int year, 
            string trimestre, 
            byte[] logoBytes, 
            byte[] signatureBytes, 
            DateTime startDate, 
            DateTime endDate)
        {
            _stagiaire = stagiaire;
            _courseName = courseName ?? "Français & Intégration";
            _oralLevel = oralLevel;
            _writtenLevel = writtenLevel;
            _rawActivities = ateliersSuivis ?? new List<string>();
            _tauxParticipation = tauxParticipation;
            _year = year;
            _trimestre = trimestre;
            _logoBytes = logoBytes ?? Array.Empty<byte>();
            _signatureBytes = signatureBytes ?? Array.Empty<byte>();
            _startDate = startDate;
            _endDate = endDate;
        }

        public void Compose(IDocumentContainer document)
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0.75f, Unit.Inch);
                page.DefaultTextStyle(x => x.FontFamily("Helvetica").FontSize(11).FontColor(Colors.Black));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    if (_logoBytes.Length > 0)
                        row.RelativeItem().Height(60).Image(_logoBytes).FitHeight();
                    
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

        private void ComposeContent(IContainer container)
        {
            bool isTeleCours = _courseName.Contains("TéléCours", StringComparison.OrdinalIgnoreCase);

            container.PaddingTop(40).Column(col =>
            {
                col.Spacing(20);

                col.Item().AlignCenter().Text("Attestation").FontSize(20).Bold();

                col.Item().AlignCenter().Text(t => {
                    t.Span("L'association ").FontSize(14);
                    t.Span("CONFLUENCES ").FontSize(15).ExtraBold();
                    t.Span("atteste que").FontSize(14);
                });

                col.Item().AlignCenter().PaddingVertical(10).Text($"{_stagiaire.Firstname} {_stagiaire.LastName}").FontSize(35).ExtraBold();

                col.Item().AlignCenter().Column(c => {
                    c.Item().AlignCenter().Text(t => {
                        t.Span("a suivi la formation ").FontSize(14);
                        t.Span($"“{_courseName}”").FontSize(14).ExtraBold();
                    });
                    
                    c.Item().AlignCenter().Text(isTeleCours 
                        ? "(9 heures par semaine en télé-enseignement)" 
                        : "(15 heures par semaine)").FontSize(12).Italic();
                });

                // Liste des matières fixes
                col.Item().PaddingLeft(60).Column(list => {
                    list.Spacing(5);
                    list.Item().Text($"• Français de niveau {_oralLevel}").FontSize(13).Bold();
                    if (isTeleCours) {
                        list.Item().Text("• Compétences numériques de base").FontSize(13).Bold();
                    } else {
                        list.Item().Text("• Informatique et bureautique").FontSize(13).Bold();
                        list.Item().Text("• Aide à la recherche d’emploi").FontSize(13).Bold();
                    }
                });

                // Section Ateliers/Accompagnements (Uniquement pour F&I)
                if (!isTeleCours && _rawActivities.Any())
                {
                    col.Item().PaddingTop(10).AlignCenter().Text("a aussi bénéficié d’accompagnements et d’ateliers facultatifs :").FontSize(12).Italic();

                    col.Item().PaddingHorizontal(60).Column(sectionCol =>
                    {
                        // On parse les strings au format "Nom|Type"
                        var parsedActivities = _rawActivities
    .Select(a => {
        var parts = a.Split('|');
        return new {
            Nom   = parts[0],
            Type  = parts.Length > 1 ? parts[1].ToLower() : "atelier",
            Total = parts.Length > 2 && int.TryParse(parts[2], out var n) ? n : 1
        };
    })
    .OrderBy(x => x.Nom)
    .ToList();

                        foreach (var item in parsedActivities)
                        {
                            sectionCol.Item().Row(r => {
                                r.ConstantItem(35).Text($"{item.Total} x").FontSize(11).Italic().FontColor(Colors.Grey.Darken2);
                                r.RelativeItem().Text(item.Nom).FontSize(11);
                            });
                        }
                    });
                }

                col.Item().PaddingTop(20).AlignCenter().Column(c => {
                    c.Item().AlignCenter().Text($"pendant la session du {_startDate.ToString("dd MMMM", _fr)} au {_endDate.ToString("dd MMMM yyyy", _fr)}").FontSize(13);
                    c.Item().AlignCenter().Text($"(taux de participation : {_tauxParticipation:F0} %)").FontSize(13);
                });

                col.Item().PaddingTop(30).Row(row => {
                    row.RelativeItem().PaddingLeft(20).Text($"Lausanne, le {_endDate.ToString("dd MMMM yyyy", _fr)}");
                    row.RelativeItem().AlignRight().PaddingRight(20).Column(sig => {
                        if (_signatureBytes != null && _signatureBytes.Length > 0)
                            sig.Item().AlignCenter().PaddingBottom(5).Height(70).Image(_signatureBytes).FitHeight();

                        sig.Item().AlignCenter().Text("Caroline Sanmartin").Bold().FontSize(12);
                        sig.Item().AlignCenter().Text("Responsable de la formation").FontSize(10);
                    });
                });
            });
        }
    }
}