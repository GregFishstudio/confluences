using Confluences.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Api.Documents
{
    public class ActivityEntry
    {
        public string Nom  { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public class AttestationSessionDocument : IDocument
    {
        private readonly ApplicationUser _stagiaire;
        private readonly string          _courseName;
        private readonly List<string>    _rawActivities;
        private readonly double          _tauxParticipation;
        private readonly byte[]          _logoBytes;
        private readonly byte[]          _signatureBytes;
        private readonly string          _oralLevel;
        private readonly string          _writtenLevel;
        private readonly DateTime        _startDate;
        private readonly DateTime        _endDate;
        private readonly string          _trimestre;
        private readonly int             _year;
        private readonly CultureInfo     _fr = new CultureInfo("fr-FR");

        public AttestationSessionDocument(
            ApplicationUser stagiaire,
            string courseName,
            string oralLevel,
            string writtenLevel,
            List<string> ateliersSuivis,
            double tauxParticipation,
            int year,
            string trimestre,
            byte[] logoBytes,
            byte[] signatureBytes,
            DateTime startDate,
            DateTime endDate)
        {
            _stagiaire         = stagiaire;
            _courseName        = courseName ?? "Français & Intégration";
            _oralLevel         = oralLevel;
            _writtenLevel      = writtenLevel;
            _rawActivities     = ateliersSuivis ?? new List<string>();
            _tauxParticipation = tauxParticipation;
            _year              = year;
            _trimestre         = trimestre;
            _logoBytes         = logoBytes     ?? Array.Empty<byte>();
            _signatureBytes    = signatureBytes ?? Array.Empty<byte>();
            _startDate         = startDate;
            _endDate           = endDate;
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
            bool isTeleCours = _courseName.Contains("TéléCours", StringComparison.OrdinalIgnoreCase)
                            || _courseName.Contains("Télécours", StringComparison.OrdinalIgnoreCase);

            // Nom du cours sans le suffixe de niveau entre parenthèses
            string displayCourseName = _courseName;
            int parenIdx = displayCourseName.IndexOf(" (Niveau", StringComparison.OrdinalIgnoreCase);
            if (parenIdx > 0) displayCourseName = displayCourseName.Substring(0, parenIdx);

            string heures = isTeleCours ? "9 heures par semaine en télé-enseignement" : "15 heures par semaine";
            string niveau = !string.IsNullOrWhiteSpace(_oralLevel) ? _oralLevel : "—";

            container.PaddingTop(40).Column(col =>
            {
                col.Spacing(20);

                // ── Titre ────────────────────────────────────────────────────────
                col.Item().AlignCenter().Text("Attestation").FontSize(28).Bold();

                // ── L'association atteste que ─────────────────────────────────────
                col.Item().AlignCenter().Text(t =>
                {
                    t.Span("L'association ").FontSize(14);
                    t.Span("CONFLUENCES ").FontSize(15).ExtraBold();
                    t.Span("atteste que").FontSize(14);
                });

                // ── Nom du/de la participant·e ────────────────────────────────────
                col.Item().AlignCenter().PaddingVertical(10)
                    .Text($"{_stagiaire.Firstname} {_stagiaire.LastName}")
                    .FontSize(35).ExtraBold();

                // ── Formation + heures sur la même ligne ─────────────────────────
                col.Item().AlignCenter().Text(t =>
                {
                    t.Span("a suivi la formation ").FontSize(14);
                    t.Span($"« {displayCourseName} »").FontSize(14).ExtraBold();
                    t.Span($" ({heures})").FontSize(12).Italic();
                });

                // ── Liste des matières fixes ──────────────────────────────────────
                col.Item().PaddingLeft(60).Column(list =>
                {
                    list.Spacing(6);
                    list.Item().Text($"•   Français de niveau {niveau}").FontSize(13).Bold();
                    if (isTeleCours)
                    {
                        list.Item().Text("•   Compétences numériques de base").FontSize(13).Bold();
                    }
                    else
                    {
                        list.Item().Text("•   Informatique et bureautique").FontSize(13).Bold();
                        list.Item().Text("•   Aide à la recherche d'emploi").FontSize(13).Bold();
                    }
                });

                // ── Accompagnements et ateliers (F&I uniquement) ──────────────────
                if (!isTeleCours)
                {
                    col.Item().PaddingTop(4).AlignCenter()
                        .Text("A aussi bénéficié d'accompagnements et d'ateliers facultatifs :")
                        .FontSize(12).Bold();

                    if (_rawActivities.Any())
                    {
                        var activities = _rawActivities
                            .Select(a =>
                            {
                                var parts = a.Split('|');
                                int count = parts.Length > 2 && int.TryParse(parts[2], out var c) ? c : 0;
                                return (
                                    label: parts[0],
                                    type:  parts.Length > 1 ? parts[1].ToLower() : "atelier",
                                    count: count
                                );
                            })
                            .Where(a => a.count >= 1)
                            .OrderBy(a => a.type == "acco" ? 0 : 1)
                            .ThenBy(a => a.label)
                            .ToList();

                        byte[] tagPng = GenerateTagCloudPng(activities);
                        col.Item().PaddingHorizontal(10).PaddingTop(6).Image(tagPng).FitWidth();
                    }
                }

                // ── Dates + taux ──────────────────────────────────────────────────
                col.Item().PaddingTop(20).AlignCenter().Column(c =>
                {
                    c.Item().AlignCenter()
                        .Text($"du {FormatDayFr(_startDate)} au {FormatDayFr(_endDate, withYear: true)}")
                        .FontSize(13);
                    c.Item().AlignCenter()
                        .Text($"(taux de participation : {_tauxParticipation:F0} %)")
                        .FontSize(13);
                });

                // ── Signature ─────────────────────────────────────────────────────
                col.Item().PaddingTop(30).Row(row =>
                {
                    row.RelativeItem().PaddingLeft(20)
                        .Text($"Lausanne, le {FormatDayFr(_endDate, withYear: true)}");
                    row.RelativeItem().AlignRight().PaddingRight(20).Column(sig =>
                    {
                        if (_signatureBytes != null && _signatureBytes.Length > 0)
                            sig.Item().AlignCenter().PaddingBottom(5).Height(70).Image(_signatureBytes).FitHeight();
                        sig.Item().AlignCenter().Text("Caroline Sanmartin").Bold().FontSize(12);
                        sig.Item().AlignCenter().Text("Responsable de la formation").FontSize(10);
                    });
                });
            });
        }

        // ── Helpers dates ──────────────────────────────────────────────────────────
        private string FormatDayFr(DateTime date, bool withYear = false)
        {
            string day   = date.Day == 1 ? "1er" : date.Day.ToString();
            string month = date.ToString("MMMM", _fr);
            return withYear
                ? $"{day} {month} {date.Year}"
                : $"{day} {month}";
        }

        // ── Nuage de mots (tag cloud SkiaSharp) ───────────────────────────────────
        private static byte[] GenerateTagCloudPng(List<(string label, string type, int count)> activities)
        {
            const int SCALE = 3;
            const int contentWidthPt = 410;
            const int W = contentWidthPt * SCALE;

            float fontSize    =  8f * SCALE;   // réduit : était 12
            float padX        =  8f * SCALE;
            float padY        =  4f * SCALE;
            float chipSpacing =  6f * SCALE;
            float rowSpacing  =  7f * SCALE;
            float margin      =  6f * SCALE;
            float radius      =  4f * SCALE;

            var accoFill     = SKColor.Parse("#D8F3F0");
            var accoBorder   = SKColor.Parse("#2CA6A4");
            var accoAttended = SKColor.Parse("#2CA6A4");   // fond plein si participé
            var atFill       = SKColor.Parse("#DDEEFF");
            var atBorder     = SKColor.Parse("#4A90E2");
            var atAttended   = SKColor.Parse("#4A90E2");
            var textLight    = SKColor.Parse("#1A2E44");
            var textWhite    = SKColors.White;

            using var measurePaint = new SKPaint { TextSize = fontSize, IsAntialias = true };

            float chipH = fontSize + padY * 2;
            float x = margin, y = margin;
            float maxY = margin + chipH;

            var chips = new List<(string text, string type, int count, SKRect rect)>();

            foreach (var (label, type, count) in activities)
            {
                string text = count > 0 ? $"{label}  ×{count}" : label;
                float tw = measurePaint.MeasureText(text);
                float cw = tw + padX * 2;

                if (x + cw > W - margin)
                {
                    x  = margin;
                    y += chipH + rowSpacing;
                }

                chips.Add((text, type, count, new SKRect(x, y, x + cw, y + chipH)));
                x   += cw + chipSpacing;
                maxY = Math.Max(maxY, y + chipH + margin);
            }

            int H = (int)maxY;
            if (H < 1) H = (int)(chipH + 2 * margin);

            using var surface = SKSurface.Create(
                new SKImageInfo(W, H, SKColorType.Rgba8888, SKAlphaType.Premul));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            foreach (var (text, type, count, rect) in chips)
            {
                bool attended  = count > 0;
                var  fillColor = attended
                    ? (type == "acco" ? accoAttended : atAttended)
                    : (type == "acco" ? accoFill     : atFill);
                var borderColor = type == "acco" ? accoBorder : atBorder;
                var textColor   = attended ? textWhite : textLight;

                var rr = new SKRoundRect(rect, radius);
                using var fp = new SKPaint { Color = fillColor,   Style = SKPaintStyle.Fill,   IsAntialias = true };
                using var bp = new SKPaint { Color = borderColor, Style = SKPaintStyle.Stroke, StrokeWidth = 1f * SCALE, IsAntialias = true };
                canvas.DrawRoundRect(rr, fp);
                canvas.DrawRoundRect(rr, bp);

                using var tp = new SKPaint
                    { Color = textColor, TextSize = fontSize, IsAntialias = true, TextAlign = SKTextAlign.Center };
                canvas.DrawText(text, rect.MidX, rect.Top + padY + fontSize * 0.82f, tp);
            }

            using var img  = surface.Snapshot();
            using var data = img.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
    }
}
