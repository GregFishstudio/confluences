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

            string heures = isTeleCours ? "9 heures par semaine de cours en ligne" : "15 heures par semaine";
            string niveau = !string.IsNullOrWhiteSpace(_oralLevel) ? ExtractBaseLevel(_oralLevel) : "—";

            container.PaddingTop(25).Column(col =>
            {
                col.Spacing(14);

                // ── Titre ────────────────────────────────────────────────────────
                col.Item().AlignCenter().Text("Attestation").FontSize(26).Bold();

                // ── L'association atteste que ─────────────────────────────────────
                col.Item().AlignCenter().Text(t =>
                {
                    t.Span("L'association ").FontSize(13);
                    t.Span("Confluences ").FontSize(14);
                    t.Span("atteste que").FontSize(13);
                });

                // ── Nom du/de la participant·e ────────────────────────────────────
                col.Item().PaddingVertical(8).Text(t =>
                {
                    t.AlignCenter();
                    t.Span($"{_stagiaire.Firstname} {_stagiaire.LastName}").FontSize(24).ExtraBold();
                });

                // ── Formation (nom) ───────────────────────────────────────────────
                col.Item().AlignCenter().Text(t =>
                {
                    t.AlignCenter();
                    t.Span("a suivi la formation ").FontSize(13);
                    t.Span($"« {displayCourseName} »").FontSize(13).ExtraBold();
                });

                // ── Heures sur une ligne séparée ──────────────────────────────────
                col.Item().AlignCenter().Text(t =>
                {
                    t.AlignCenter();
                    t.Span($"({heures})").FontSize(11).Italic();
                });

                // ── Liste des matières fixes ──────────────────────────────────────
                col.Item().PaddingLeft(60).Column(list =>
                {
                    list.Spacing(6);
                    list.Item().Text($"•   Français de niveau {niveau}").FontSize(12).Bold();
                    if (isTeleCours)
                    {
                        list.Item().Text("•   Compétences numériques de base").FontSize(12).Bold();
                    }
                    else
                    {
                        list.Item().Text("•   Informatique et bureautique").FontSize(12).Bold();
                        list.Item().Text("•   Aide à la recherche d'emploi").FontSize(12).Bold();
                    }
                });

                // ── Accompagnements et ateliers (F&I uniquement) ──────────────────
                if (!isTeleCours)
                {
                    // Fusion des activités avec le même label (ex: "Français" en acco ET atelier)
                    var allActivities = _rawActivities
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
                        .GroupBy(a => a.label)
                        .Select(g => (
                            label: g.Key,
                            type:  g.OrderByDescending(x => x.count).First().type,
                            count: g.Sum(x => x.count)
                        ))
                        .OrderBy(a => a.type == "acco" ? 0 : 1)
                        .ThenBy(a => a.label)
                        .ToList();

                    bool hasAttended = allActivities.Any(a => a.count > 0);

                    if (hasAttended)
                    {
                        col.Item().AlignCenter()
                            .Text("A aussi bénéficié d'accompagnements et d'ateliers facultatifs :")
                            .FontSize(11).Bold();
                    }
                    else
                    {
                        col.Item().AlignCenter()
                            .Text("n'a pas suivi les accompagnements ni les ateliers facultatifs suivants.")
                            .FontSize(11);
                    }

                    if (allActivities.Any())
                    {
                        byte[] tagPng = GenerateTagCloudPng(allActivities);
                        col.Item().PaddingHorizontal(10).PaddingTop(4).Image(tagPng).FitWidth();
                    }
                }

                // ── Dates + taux ──────────────────────────────────────────────────
                col.Item().PaddingTop(14).AlignCenter().Column(c =>
                {
                    c.Item().AlignCenter()
                        .Text($"du {FormatDayFr(_startDate)} au {FormatDayFr(_endDate, withYear: true)}")
                        .FontSize(12);
                    c.Item().AlignCenter()
                        .Text($"(taux de participation : {_tauxParticipation:F0} %)")
                        .FontSize(12);
                });

                // ── Signature ─────────────────────────────────────────────────────
                col.Item().PaddingTop(22).Row(row =>
                {
                    row.RelativeItem().PaddingLeft(20)
                        .Text($"Lausanne, le {FormatDayFr(_endDate, withYear: true)}");
                    row.RelativeItem().AlignRight().PaddingRight(20).Column(sig =>
                    {
                        if (_signatureBytes != null && _signatureBytes.Length > 0)
                            sig.Item().AlignCenter().PaddingBottom(5).Height(60).Image(_signatureBytes).FitHeight();
                        sig.Item().AlignCenter().Text("Caroline Sanmartin").Bold().FontSize(11);
                        sig.Item().AlignCenter().Text("Responsable de la formation").FontSize(9);
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

        // Retourne "A1" ou "A2" à partir de "A1.1", "A1.2", "A2.1", etc.
        private static string ExtractBaseLevel(string level)
        {
            var m = System.Text.RegularExpressions.Regex.Match(level.Trim(), @"[A-Za-z]\d");
            return m.Success ? m.Value.ToUpper() : level;
        }

        // ── Nuage de mots (tag cloud SkiaSharp) ───────────────────────────────────
        private static byte[] GenerateTagCloudPng(List<(string label, string type, int count)> activities)
        {
            const int SCALE = 3;
            const int contentWidthPt = 450;
            const int W = contentWidthPt * SCALE;

            float padX        = 10f * SCALE;
            float padY        =  6f * SCALE;
            float chipSpacing =  8f * SCALE;
            float rowSpacing  = 10f * SCALE;
            float margin      =  8f * SCALE;
            float radius      = 10f * SCALE;

            // Palette vive pour les activités suivies
            var palette = new[]
            {
                SKColor.Parse("#E84393"),  // rose
                SKColor.Parse("#00A99D"),  // turquoise
                SKColor.Parse("#F7941D"),  // orange
                SKColor.Parse("#662D91"),  // violet
                SKColor.Parse("#29ABE2"),  // bleu ciel
                SKColor.Parse("#8DC63F"),  // vert
                SKColor.Parse("#ED1C24"),  // rouge
                SKColor.Parse("#006837"),  // vert foncé
            };
            // Couleur unique ultra-pastel pour les activités non suivies
            var notAttFill   = SKColor.Parse("#E0F4EE");   // #85C6A9 dilué à ~15%
            var notAttBorder = SKColor.Parse("#85C6A9");   // couleur logo d'origine
            var notAttText   = SKColor.Parse("#2E6B53");   // vert foncé lisible

            // Calcul des dimensions de chaque puce
            int paletteIdx = 0;
            var items = new List<(string text, SKColor fill, SKColor border, SKColor textColor, float fontSize, float w, float h)>();

            foreach (var (label, type, count) in activities)
            {
                bool attended = count > 0;
                // Taille proportionnelle au nombre de participations
                float ptSize = attended
                    ? Math.Min(Math.Max(8.5f + count * 1.2f, 8.5f), 15f)
                    : 7f;
                float fontSize = ptSize * SCALE;

                string text = attended ? $"{label}  {count}" : label;

                SKColor fill, border, textColor;
                if (attended)
                {
                    fill      = palette[paletteIdx++ % palette.Length];
                    border    = fill;
                    textColor = SKColors.White;
                }
                else
                {
                    fill      = notAttFill;
                    border    = notAttBorder;
                    textColor = notAttText;
                }

                float tw, ch;
                using (var mp = new SKPaint { TextSize = fontSize, IsAntialias = true })
                    tw = mp.MeasureText(text);
                float cw = tw + padX * 2;
                ch = fontSize + padY * 2;

                items.Add((text, fill, border, textColor, fontSize, cw, ch));
            }

            // Disposition en rangées, chaque rangée centrée
            var rows = new List<List<int>>();
            var currentRow = new List<int> { 0 };
            float rowWidth = items.Count > 0 ? items[0].w : 0;

            for (int i = 1; i < items.Count; i++)
            {
                float needed = rowWidth + chipSpacing + items[i].w;
                if (needed > W - 2 * margin)
                {
                    rows.Add(currentRow);
                    currentRow = new List<int> { i };
                    rowWidth   = items[i].w;
                }
                else
                {
                    currentRow.Add(i);
                    rowWidth += chipSpacing + items[i].w;
                }
            }
            if (currentRow.Count > 0) rows.Add(currentRow);

            // Hauteur totale
            float totalH = margin;
            foreach (var row in rows)
            {
                float rh = 0;
                foreach (int i in row) rh = Math.Max(rh, items[i].h);
                totalH += rh + rowSpacing;
            }
            totalH += margin;

            int H = Math.Max((int)totalH, 1);
            using var surface = SKSurface.Create(
                new SKImageInfo(W, H, SKColorType.Rgba8888, SKAlphaType.Premul));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            float y = margin;
            foreach (var row in rows)
            {
                float rowH = 0;
                float totalW = 0;
                foreach (int i in row)
                {
                    rowH   = Math.Max(rowH, items[i].h);
                    totalW += items[i].w;
                }
                totalW += (row.Count - 1) * chipSpacing;

                float x = (W - totalW) / 2f;

                foreach (int idx in row)
                {
                    var (text, fill, border, textColor, fontSize, cw, ch) = items[idx];
                    float chipTop = y + (rowH - ch) / 2f;
                    var rect = new SKRect(x, chipTop, x + cw, chipTop + ch);
                    var rr   = new SKRoundRect(rect, radius);

                    using var fp = new SKPaint { Color = fill,   Style = SKPaintStyle.Fill,   IsAntialias = true };
                    using var bp = new SKPaint { Color = border, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f * SCALE, IsAntialias = true };
                    canvas.DrawRoundRect(rr, fp);
                    canvas.DrawRoundRect(rr, bp);

                    using var tp = new SKPaint
                        { Color = textColor, TextSize = fontSize, IsAntialias = true, TextAlign = SKTextAlign.Center };
                    canvas.DrawText(text, rect.MidX, rect.Top + (ch - fontSize) / 2f + fontSize * 0.82f, tp);

                    x += cw + chipSpacing;
                }
                y += rowH + rowSpacing;
            }

            using var img  = surface.Snapshot();
            using var data = img.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
    }
}
