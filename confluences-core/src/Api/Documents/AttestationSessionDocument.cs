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
        private readonly string _courseName;
        private readonly List<string> _ateliersSuivis;
        private readonly double _tauxParticipation;
        private readonly int _year;
        private readonly string _trimestre;
        private readonly byte[] _logoBytes;
        private readonly string _oralLevel;
        private readonly string _writtenLevel;
        private readonly DateTime _startDate;
        private readonly DateTime _endDate;

        private bool IsTelecours => _courseName.ToLower().Contains("télécours") || _courseName.ToLower().Contains("telecours");

        private static readonly Color PrimaryColor = Color.FromHex("#1A202C");
        private static readonly Color MediumGrey = Color.FromHex("#718096");
        private static readonly Color AccentColor = Color.FromHex("#00B4B0");
        private static readonly Color LightGrey = Color.FromHex("#EDF2F7");
        private readonly CultureInfo _fr = new CultureInfo("fr-FR");

        public AttestationSessionDocument(ApplicationUser stagiaire, string courseName, string oralLevel, string writtenLevel, List<string> ateliersSuivis, double tauxParticipation, int year, string trimestre, byte[] logoBytes, DateTime startDate, DateTime endDate)
        {
            _stagiaire = stagiaire;
            _courseName = courseName ?? "Français & Intégration";
            _oralLevel = oralLevel;
            _writtenLevel = writtenLevel;
            _ateliersSuivis = ateliersSuivis ?? new List<string>();
            _tauxParticipation = tauxParticipation;
            _year = year;
            _trimestre = trimestre;
            _logoBytes = logoBytes ?? Array.Empty<byte>();
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

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                
                page.Footer().AlignCenter().Text(x => {
                    x.Span("Association Confluences - Chemin de Renens 52e, 1004 Lausanne").FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                // Logo à gauche
                row.RelativeItem().Column(col =>
                {
                    if (_logoBytes.Length > 0)
                        col.Item().Height(55).Image(_logoBytes).FitHeight();
                    else
                        col.Item().Text("CONFLUENCES").FontSize(22).ExtraBold().FontColor(AccentColor);
                });

                // BLOC ADRESSE EN HAUT À DROITE
                row.RelativeItem().AlignRight().PaddingTop(8).Column(c =>
                {
                    c.Spacing(3);
                    c.Item().Text("Association Confluences").SemiBold().FontSize(10).FontColor(PrimaryColor);
                    c.Item().Text("Chemin de Renens 52e, 1004 Lausanne").FontSize(9).FontColor(MediumGrey);
                    c.Item().Text("confluences.vaud@gmail.com").FontSize(9).FontColor(MediumGrey);
                });
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingTop(40).Column(col =>
            {
                col.Spacing(25);

                // Titre
                col.Item().AlignCenter().Column(c => {
                    c.Item().Text("ATTESTATION").FontSize(28).ExtraBold().LetterSpacing(0.1f);
                    c.Item().PaddingTop(4).AlignCenter().Width(80).LineHorizontal(2).LineColor(AccentColor);
                });

                // Introduction
                col.Item().PaddingTop(10).AlignCenter().Text(t => {
                    t.Span("L'association ").FontSize(13);
                    t.Span("CONFLUENCES").FontSize(13).Bold();
                    t.Span(" atteste que").FontSize(13);
                });

                // Nom du bénéficiaire
                col.Item().AlignCenter().Background(LightGrey).Padding(12).Text($"{_stagiaire.Firstname} {_stagiaire.LastName}")
                    .FontSize(24).ExtraBold().FontColor(PrimaryColor).AlignCenter();

                // Détails de la formation obligatoire
                col.Item().Column(c => {
                    if (IsTelecours)
                    {
                        c.Item().Text(t => {
                            t.Span("a suivi la formation ");
                            t.Span("“Français en Télécours”").Bold();
                            t.Span(" (3 heures par semaine)");
                        });
                    }
                    else
                    {
                        c.Item().Text(t => {
                            t.Span("a suivi la formation ");
                            t.Span("“Français & Intégration”").Bold();
                            t.Span(" (15 heures par semaine)");
                        });
                    }
                    
                    c.Item().PaddingTop(5).Text($"Niveau atteint : {_oralLevel ?? "A1"} (oral) / {_writtenLevel ?? "A1"} (écrit)").Italic().FontSize(10);
                });

                // SÉPARATION ET ATELIERS FACULTATIFS
                if (!IsTelecours && _ateliersSuivis.Any())
                {
                    col.Item().Column(aCol => {
                        // LA PHRASE DE SÉPARATION
                        aCol.Item().PaddingBottom(8).Text("a aussi bénéficié d’accompagnements et d’ateliers facultatifs :")
                            .SemiBold().FontColor(AccentColor);

                        aCol.Item().Table(table => {
                            table.ColumnsDefinition(columns => {
                                columns.ConstantColumn(15);
                                columns.RelativeColumn();
                            });
                            foreach (var atelier in _ateliersSuivis) {
                                table.Cell().PaddingTop(2).Text("•");
                                table.Cell().PaddingTop(2).Text(atelier).FontSize(10);
                            }
                        });
                    });
                }

                // Période et Taux
                col.Item().PaddingTop(10).Column(pCol => {
                    pCol.Item().Text(t => {
                        t.Span("Période : du ");
                        t.Span($"{_startDate.ToString("dd MMMM", _fr)} au {_endDate.ToString("dd MMMM yyyy", _fr)}").Bold();
                    });
                    pCol.Item().Text($"Taux global de participation : {_tauxParticipation:F1}%");
                });

                // Signature
                col.Item().PaddingTop(40).Row(row => {
                    row.RelativeItem().Text($"Lausanne, le {DateTime.Now.ToString("dd MMMM yyyy", _fr)}");
                    row.RelativeItem().AlignRight().Column(sig => {
                        sig.Item().Text("La Direction").Bold();
                        sig.Item().PaddingTop(40).Width(120).LineHorizontal(1);
                    });
                });
            });
        }
    }
}