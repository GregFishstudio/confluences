using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Confluences.Domain.Entities;
using Api.Models.Dto.Attendance;

namespace Api.Documents
{
    public class SessionContext
    {
        public int      SessionMonth      { get; set; } = 1;
        public string   SessionType       { get; set; } = "fi";
        public int      SessionTargetDays { get; set; } = 20;
        public string   SessionName       { get; set; } = "";
        public DateTime SessionStartDate  { get; set; } = DateTime.MinValue;
        // Date de fin globale de la session (ex: 18 juin) — utilisée pour la signature
        public DateTime SessionEndDate    { get; set; } = DateTime.MinValue;
    }

    public class AttestationPresenceDocument : IDocument
    {
        private readonly DateTime   _startDate;
        private readonly DateTime   _endDate;
        private readonly List<AttendanceEntryDto> _presences;
        private readonly ApplicationUser _stagiaire;
        private readonly byte[]     _logo;
        private readonly SessionContext _session;

        private readonly CultureInfo _frenchCulture = new CultureInfo("fr-FR");

        private static readonly Color PrimaryColor = Color.FromHex("#1A2E44");
        private static readonly Color AccentColor  = Color.FromHex("#2CA6A4");
        private static readonly Color MediumGrey   = Color.FromHex("#6B7280");

        private static readonly Color ColPresent  = Color.FromHex("#2CA6A4");
        private static readonly Color ColAbsent   = Color.FromHex("#E07050");
        private static readonly Color ColExcuse   = Color.FromHex("#B5C5D5");
        private static readonly Color ColRetard   = Color.FromHex("#6BAED6");
        private static readonly Color ColVacances = Color.FromHex("#95A5A6");
        private static readonly Color ColFerie    = Color.FromHex("#BDC3C7");
        private static readonly Color ColVisite   = Color.FromHex("#CE93D8");

        private static readonly SKColor SkPresent = SKColor.Parse("#2CA6A4");
        private static readonly SKColor SkAbsent  = SKColor.Parse("#E07050");
        private static readonly SKColor SkExcuse  = SKColor.Parse("#B5C5D5");
        private static readonly SKColor SkRetard  = SKColor.Parse("#6BAED6");
        private static readonly SKColor SkVisite  = SKColor.Parse("#CE93D8");

        private readonly TextStyle _baseStyle;

        public AttestationPresenceDocument(
            DateTime startDate,
            DateTime endDate,
            List<AttendanceEntryDto> presences,
            List<ApplicationUser> stagiaires,
            byte[] logo    = null,
            byte[] _unused = null,
            SessionContext session = null)
        {
            _endDate = endDate.Date;
            _session = session ?? new SessionContext();
            _startDate = _session.SessionStartDate > DateTime.MinValue
                ? _session.SessionStartDate.Date
                : startDate.Date;

            _presences = presences ?? new List<AttendanceEntryDto>();
            _stagiaire = stagiaires?.FirstOrDefault()
                ?? throw new ArgumentException("La liste de stagiaires est vide ou nulle.");
            _logo = logo ?? Array.Empty<byte>();

            _baseStyle = TextStyle.Default
                .FontFamily("Arial").FontSize(10).FontColor(PrimaryColor);
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title  = $"Attestation de présence - {_stagiaire.LastName} {_stagiaire.Firstname}",
            Author = "Association Confluences"
        };

        public void Compose(IDocumentContainer document)
        {
            document.Page(page =>
            {
                page.Margin(40);
                page.DefaultTextStyle(_baseStyle);
                page.Size(PageSizes.A4);

                page.Header().Element(RenderHeader);
                page.Content().Element(RenderContent);
                page.Footer().Element(RenderFooter);
            });
        }

        // ── En-tête : logo + adresse à gauche / titre à droite ────────────────
        private void RenderHeader(IContainer container)
        {
            container.PaddingBottom(18)
                .Border(1).BorderColor(Colors.Grey.Lighten2)
                .Padding(14).Row(row =>
                {
                    // Gauche : logo + adresse
                    row.RelativeItem().Column(col =>
                    {
                        if (_logo.Length > 0)
                            col.Item().Height(44).Image(_logo).FitArea();
                        else
                            col.Item().Text("Confluences").FontSize(15).Bold().FontColor(AccentColor);

                        col.Item().PaddingTop(5)
                            .Text("Association Confluences").FontSize(8.5f).SemiBold().FontColor(PrimaryColor);
                        col.Item().Text("Chemin de Renens 52e · Lausanne").FontSize(8).FontColor(MediumGrey);
                        col.Item().Text("www.confluences.ch").FontSize(8).FontColor(MediumGrey);
                    });

                    // Droite : ATTESTATION DE PRÉSENCE
                    row.RelativeItem().AlignRight().AlignMiddle().Column(col =>
                    {
                        col.Item().AlignRight()
                            .Text("ATTESTATION").FontSize(24).Bold().FontColor(PrimaryColor);
                        col.Item().AlignRight()
                            .Text("DE PRÉSENCE").FontSize(24).Bold().FontColor(PrimaryColor);
                    });
                });
        }

        // ── Pied de page ──────────────────────────────────────────────────────
        private void RenderFooter(IContainer container)
        {
            // Signature = fin de la partie en cours (ex: 24 mai pour P2), telle que saisie dans le calendrier
            container.Column(col =>
            {
                col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                col.Item().PaddingTop(6).Row(row =>
                {
                    row.RelativeItem()
                        .Text($"Lausanne, le {_endDate.ToString("dd MMMM yyyy", _frenchCulture)}")
                        .FontSize(9).FontColor(PrimaryColor);
                });
            });
        }

        // ── Contenu principal ─────────────────────────────────────────────────
        private void RenderContent(IContainer container)
        {
            var id = _stagiaire.Id.ToString();

            var filtered = _presences
                .Where(p =>
                    p.StagiaireId != null &&
                    p.StagiaireId.Equals(id, StringComparison.OrdinalIgnoreCase) &&
                    p.Date.Date >= _startDate &&
                    p.Date.Date <= _endDate)
                .OrderBy(p => p.Date)
                .ToList();

            bool isTelecours = _session.SessionType?.ToLower().Contains("tele") == true;

            int absents  = filtered.Count(p => N(p.Statut) == "a");
            int excuses  = filtered.Count(p => N(p.Statut) == "e");
            int retards  = filtered.Count(p => N(p.Statut) == "r");
            int stages   = filtered.Count(p => N(p.Statut) == "s");
            int visites  = filtered.Count(p => N(p.Statut) == "v"); // visite à domicile (télécours)
            // Télécours : pas de cours le mercredi ni le vendredi → fériés ces jours-là exclus
            int feries   = filtered.Count(p => N(p.Statut) == "f"
                               && !(isTelecours && (p.Date.DayOfWeek == DayOfWeek.Wednesday
                                                 || p.Date.DayOfWeek == DayOfWeek.Friday)));
            int total    = filtered.Count;

            double absenceScore = absents + excuses + (retards * 0.5);
            int    targetDays   = _session.SessionTargetDays;

            int totalAffiche = targetDays > 0
                ? Math.Max(1, targetDays - feries)
                : (total - feries - visites);

            double presentsExact   = Math.Max(0.0, totalAffiche - absenceScore);
            int    presentsAffiche = (int)Math.Round(presentsExact);

            // Arrondi demi-bas : > x.5 → haut, = x.5 → bas (ex: 97.5→97, 97.6→98)
            double sessionRate = totalAffiche > 0
                ? Math.Min(100.0, Math.Ceiling(presentsExact / totalAffiche * 100.0 - 0.5))
                : 100.0;

            // Pour le camembert, les visites à domicile (télécours) sont séparées des présences pures
            double visitesSlice  = isTelecours ? visites : 0.0;
            double presentsSlice = Math.Max(0.0, presentsExact - visitesSlice);

            // label = texte long pour la légende ; pieLabel = texte court pour le camembert
            var slices = new List<(string label, string pieLabel, double count, SKColor skColor, Color pdfColor)>
            {
                ("Présent·e",              "Présent·e",  presentsSlice, SkPresent, ColPresent),
                ("Visite à dom.",          "Visite",     visitesSlice,  SkVisite,  ColVisite),
                ("Absent·e",               "Absent·e",   absents,       SkAbsent,  ColAbsent),
                ("Excusé·e",               "Excusé·e",   excuses,       SkExcuse,  ColExcuse),
                ("Participation partielle","Part. part.", retards * 0.5, SkRetard,  ColRetard),
            }.Where(s => s.count > 0).ToList();

            byte[] chartPng = GeneratePieChartPng(
                slices.Select(s => (s.pieLabel, s.count, s.skColor)).ToList(),
                totalAffiche);

            string startFmt = _startDate.ToString("dd MMMM yyyy", _frenchCulture);
            string endFmt   = _endDate.ToString("dd MMMM yyyy", _frenchCulture);

            string mesureLabel    = isTelecours ? "Mesure Français Télécours" : "Français & Intégration";

            string participantLabel = "Participant·e";

            container.Column(col =>
            {
                col.Spacing(10);

                // ── Tableau d'informations ─────────────────────────────────────
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(120);
                        c.RelativeColumn();
                    });

                    var infoRows = new List<(string, string)>
                    {
                        (participantLabel, $"{_stagiaire.Firstname} {_stagiaire.LastName}"),
                        ("Formation",      mesureLabel),
                        ("Période",        $"{startFmt} au {endFmt}"),
                    };
                    if (!string.IsNullOrEmpty(_session.SessionName))
                        infoRows.Add(("Session",
                            $"{_session.SessionName} | Mois {_session.SessionMonth}/3"));

                    foreach (var (rowLabel, rowValue) in infoRows)
                    {
                        table.Cell()
                            .Background(AccentColor)
                            .BorderBottom(0.5f).BorderColor(Colors.White)
                            .Padding(7).PaddingLeft(10)
                            .Text(rowLabel)
                            .SemiBold().FontSize(9).FontColor(Colors.White);

                        table.Cell()
                            .BorderBottom(0.5f).BorderLeft(0.5f).BorderColor(Colors.Grey.Lighten2)
                            .Padding(7).PaddingLeft(10)
                            .Text(rowValue)
                            .FontSize(9).FontColor(PrimaryColor);
                    }
                });

                // ── Boîtes statistiques ────────────────────────────────────────
                col.Item().Row(row =>
                {
                    row.RelativeItem().Element(c => StatBox(c,
                        $"{presentsAffiche}", "Présences",
                        Color.FromHex("#E0F5F3"), AccentColor));

                    row.RelativeItem().Element(c => StatBox(c,
                        $"{absents}", "Absent·e·s",
                        Color.FromHex("#FDECEA"), Color.FromHex("#C0392B")));

                    row.RelativeItem().Element(c => StatBox(c,
                        $"{excuses}", "Excusé·e·s",
                        Color.FromHex("#FEF9E7"), Color.FromHex("#E67E22")));

                    row.RelativeItem().Element(c => StatBox(c,
                        $"{retards}", "Part. partielle",
                        Color.FromHex("#E8F4FC"), Color.FromHex("#1A6EA8")));

                    row.RelativeItem().Element(c => StatBox(c,
                        $"{sessionRate:F0}%", "Participation",
                        Color.FromHex("#D6EAF8"), Color.FromHex("#1A5E8A")));
                });

                // ── Titre de section ───────────────────────────────────────────
                col.Item().PaddingTop(4)
                    .Text("Répartition des présences")
                    .FontSize(12).Bold().FontColor(AccentColor);

                // ── Camembert + pourcentage ────────────────────────────────────
                col.Item().Row(row =>
                {
                    row.ConstantItem(240).Height(200).Image(chartPng).FitArea();

                    row.RelativeItem().AlignMiddle().PaddingLeft(12).Column(c =>
                    {
                        c.Item().AlignCenter()
                            .Text($"{sessionRate:F0}%")
                            .FontSize(34).Bold().FontColor(Color.FromHex("#2C5F8A"));
                        c.Item().AlignCenter()
                            .Text("de participation")
                            .FontSize(10).FontColor(MediumGrey);
                    });
                });

                // ── Légende ────────────────────────────────────────────────────
                col.Item().Row(row =>
                {
                    foreach (var (label, _, count, _, pdfColor) in slices)
                    {
                        bool isPP    = label == "Participation partielle";
                        string valStr = isPP
                            ? $"{retards} fois"
                            : FormatJours(count);
                        row.AutoItem().PaddingRight(12).Row(r =>
                        {
                            r.ConstantItem(11).Height(11).AlignMiddle().Background(pdfColor);
                            r.ConstantItem(5);
                            r.AutoItem().AlignMiddle().Text(t =>
                            {
                                t.Span($"{label} : ").FontSize(8f);
                                t.Span(valStr).Bold().FontSize(8f);
                            });
                        });
                    }

                    // Fériés uniquement — Visite à dom. est déjà dans les slices ci-dessus
                    if (feries > 0)
                        row.AutoItem().Row(r =>
                        {
                            r.ConstantItem(11).Height(11).AlignMiddle().Background(ColFerie);
                            r.ConstantItem(5);
                            r.AutoItem().AlignMiddle().Text(t =>
                            {
                                t.Span("Fériés : ").FontSize(8f).FontColor(MediumGrey);
                                t.Span($"{feries} j").Bold().FontSize(8f).FontColor(MediumGrey);
                            });
                        });
                });

                // ── Note de calcul ─────────────────────────────────────────────
                col.Item().PaddingTop(6)
                    .BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2)
                    .PaddingTop(6)
                    .Text("Calcul : Présent·e = 1 jour  ·  Participation partielle = 0,5 jour d'absence  ·  Excusé·e = absence justifiée  ·  Fériés exclus.")
                    .FontSize(7.5f).FontColor(MediumGrey).Italic();
            });
        }

        // ── Boîte statistique colorée ─────────────────────────────────────────
        private static void StatBox(IContainer container, string value, string label,
            Color bgColor, Color valueColor)
        {
            container.Padding(1).Background(bgColor).Padding(10).AlignCenter().Column(c =>
            {
                c.Item().AlignCenter().Text(value).FontSize(22).Bold().FontColor(valueColor);
                c.Item().AlignCenter().Text(label).FontSize(9).FontColor(Color.FromHex("#4A4A4A"));
            });
        }

        // ── Camembert plein avec traits de repère et étiquettes ───────────────
        private static byte[] GeneratePieChartPng(
            List<(string label, double count, SKColor color)> slices,
            int total)
        {
            const int W = 640, H = 380;
            const float cx = 310f, cy = 190f;
            const float r  = 130f;

            using var surface = SKSurface.Create(
                new SKImageInfo(W, H, SKColorType.Rgba8888, SKAlphaType.Premul));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            if (total == 0 || slices.Count == 0)
            {
                using var p = new SKPaint
                    { Color = new SKColor(220, 220, 220), IsAntialias = true, Style = SKPaintStyle.Fill };
                canvas.DrawCircle(cx, cy, r, p);
            }
            else
            {
                double grandTotal = slices.Sum(s => s.count);

                // Positionne le milieu de "Présent" à gauche (180°) pour la lisibilité
                double presentFrac = grandTotal > 0 ? slices[0].count / grandTotal : 1.0;
                float startAngle = 180f - (float)(presentFrac * 360.0 / 2.0);

                var drawn = new List<(string label, float midAngle)>();

                foreach (var (label, count, color) in slices)
                {
                    float sweep = (float)(count / grandTotal * 360.0);
                    var rect = new SKRect(cx - r, cy - r, cx + r, cy + r);

                    using var fill = new SKPaint
                        { Color = color, IsAntialias = true, Style = SKPaintStyle.Fill };
                    using var path = new SKPath();
                    path.MoveTo(cx, cy);
                    path.ArcTo(rect, startAngle, sweep, false);
                    path.Close();
                    canvas.DrawPath(path, fill);

                    using var sep = new SKPaint
                    {
                        Color = SKColors.White, IsAntialias = true,
                        Style = SKPaintStyle.Stroke, StrokeWidth = 2f
                    };
                    canvas.DrawPath(path, sep);

                    drawn.Add((label, startAngle + sweep / 2f));
                    startAngle += sweep;
                }

                // Traits de repère + étiquettes
                foreach (var (label, midAngle) in drawn)
                {
                    double rad = midAngle * Math.PI / 180.0;
                    float cosA = (float)Math.Cos(rad);
                    float sinA = (float)Math.Sin(rad);

                    float p1x = cx + r * cosA;
                    float p1y = cy + r * sinA;
                    float p2x = cx + (r + 28f) * cosA;
                    float p2y = cy + (r + 28f) * sinA;

                    bool right = cosA >= 0;
                    float p3x = right ? p2x + 26f : p2x - 26f;
                    float p3y = p2y;

                    using var line = new SKPaint
                    {
                        Color = new SKColor(160, 160, 160), IsAntialias = true,
                        Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f
                    };
                    canvas.DrawLine(p1x, p1y, p2x, p2y, line);
                    canvas.DrawLine(p2x, p2y, p3x, p3y, line);

                    using var txt = new SKPaint
                    {
                        Color = new SKColor(50, 50, 50), IsAntialias = true,
                        TextSize = 19f, TextAlign = right ? SKTextAlign.Left : SKTextAlign.Right
                    };
                    canvas.DrawText(label, right ? p3x + 4f : p3x - 4f, p3y + 7f, txt);
                }
            }

            using var img  = surface.Snapshot();
            using var data = img.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        private static string FormatJours(double count)
        {
            if (count == Math.Floor(count))
            {
                int n = (int)count;
                return $"{n} jour{(n > 1 ? "s" : "")}";
            }
            // ex: 0.5 → "0,5 jour"  |  1.5 → "1,5 jour"
            return count.ToString("0.#", new CultureInfo("fr-FR")) + " jour";
        }

        private static string N(string statut)
            => string.IsNullOrWhiteSpace(statut) ? "p" : statut.Trim().ToLower();
    }
}
