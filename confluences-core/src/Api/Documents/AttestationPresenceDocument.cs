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
    /// <summary>
    /// Contexte de session transmis depuis le controller.
    ///   SessionMonth      : 1, 2 ou 3 (mois dans le trimestre)
    ///   SessionType       : "fi" ou "telecours"
    ///   SessionTargetDays : jours cibles cumulés
    ///     F&I       → mois1=20  mois2=40  mois3=60
    ///     Télécours → mois1=10  mois2=20  mois3=32
    ///   SessionName       : libellé affiché, ex "S2-2026"
    ///   SessionStartDate  : premier jour de la session (pour l'affichage de la période)
    /// </summary>
    public class SessionContext
    {
        public int      SessionMonth      { get; set; } = 1;
        public string   SessionType       { get; set; } = "fi";
        public int      SessionTargetDays { get; set; } = 20;
        public string   SessionName       { get; set; } = "";
        public DateTime SessionStartDate  { get; set; } = DateTime.MinValue;
    }

    public class AttestationPresenceDocument : IDocument
    {
        private readonly DateTime   _startDate;   // début de la session (pour l'affichage période)
        private readonly DateTime   _endDate;     // dernier jour du mois affiché (= _printDate)
        private readonly DateTime   _monthStart;  // 1er jour du mois affiché (pour filtrer les présences)
        private readonly List<AttendanceEntryDto> _presences;
        private readonly ApplicationUser _stagiaire;
        private readonly byte[]     _logo;
        private readonly SessionContext _session;

        private readonly CultureInfo _frenchCulture = new CultureInfo("fr-FR");

        // ── Couleurs PDF ──────────────────────────────────────────────────────
        private static readonly Color PrimaryColor = Color.FromHex("#2D3748");
        private static readonly Color AccentColor  = Color.FromHex("#00B4B0");
        private static readonly Color MediumGrey   = Color.FromHex("#718096");

        private static readonly Color ColPresent  = Color.FromHex("#1A237E");
        private static readonly Color ColAbsent   = Color.FromHex("#B71C1C");
        private static readonly Color ColExcuse   = Color.FromHex("#E65100");
        private static readonly Color ColRetard   = Color.FromHex("#01579B");
        private static readonly Color ColStage    = Color.FromHex("#1B5E20");
        private static readonly Color ColVacances = Color.FromHex("#2E7D32");
        private static readonly Color ColFerie    = Color.FromHex("#827717");

        // ── Couleurs SkiaSharp ────────────────────────────────────────────────
        private static readonly SKColor SkPresent  = SKColor.Parse("#1A237E");
        private static readonly SKColor SkAbsent   = SKColor.Parse("#B71C1C");
        private static readonly SKColor SkExcuse   = SKColor.Parse("#E65100");
        private static readonly SKColor SkRetard   = SKColor.Parse("#01579B");
        private static readonly SKColor SkStage    = SKColor.Parse("#1B5E20");
        private static readonly SKColor SkVacances = SKColor.Parse("#2E7D32");
        private static readonly SKColor SkFerie    = SKColor.Parse("#F9A825");

        private readonly TextStyle _titleStyle;
        private readonly TextStyle _contentTextStyle;

        // ── Constructeur ──────────────────────────────────────────────────────
        public AttestationPresenceDocument(
            DateTime startDate,
            DateTime endDate,
            List<AttendanceEntryDto> presences,
            List<ApplicationUser> stagiaires,
            byte[]         logo    = null,
            byte[]         _unused = null,   // ancien signatureBytes — conservé pour compat
            SessionContext session = null)
        {
            // _endDate = dernier jour du mois affiché (passé par le controller)
            _endDate    = endDate.Date;
            _monthStart = new DateTime(_endDate.Year, _endDate.Month, 1);

            // Si une date de début de session est fournie dans le contexte, on l'utilise
            // comme début de la période affichée ; sinon on prend le début du mois.
            _session   = session ?? new SessionContext();
            _startDate = _session.SessionStartDate > DateTime.MinValue
                ? _session.SessionStartDate.Date
                : startDate.Date;

            _presences = presences ?? new List<AttendanceEntryDto>();
            _stagiaire = stagiaires?.FirstOrDefault()
                ?? throw new ArgumentException("La liste de stagiaires est vide ou nulle.");
            _logo = logo ?? Array.Empty<byte>();

            _titleStyle = TextStyle.Default
                .FontFamily("Arial").FontSize(22).Bold().FontColor(PrimaryColor);
            _contentTextStyle = TextStyle.Default
                .FontFamily("Arial").FontSize(11).FontColor(PrimaryColor).LineHeight(1.4f);
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title  = $"Fiche de présence - {_stagiaire.LastName} {_stagiaire.Firstname}",
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

        // ── En-tête ───────────────────────────────────────────────────────────
        private void RenderHeader(IContainer container)
        {
            container.PaddingBottom(15).Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.ConstantItem(100).Height(45).Element(logo =>
                    {
                        if (_logo.Length > 0)
                            logo.Image(_logo).FitArea();
                        else
                            logo.Text("Confluences").FontSize(16).FontColor(AccentColor).SemiBold();
                    });

                    row.RelativeItem().AlignRight().Column(c =>
                    {
                        c.Spacing(2);
                        c.Item().Text("Association Confluences").SemiBold().FontSize(10).FontColor(PrimaryColor);
                        c.Item().Text("Chemin de Renens 52e, 1004 Lausanne").FontSize(9).FontColor(MediumGrey);
                        c.Item().Text("confluences.vaud@gmail.com").FontSize(9).FontColor(MediumGrey);
                        c.Item().Text("www.confluences.ch").FontSize(9).FontColor(MediumGrey);
                    });
                });
                col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            });
        }

        // ── Contenu ───────────────────────────────────────────────────────────
        private void RenderContent(IContainer container)
        {
            var id = _stagiaire.Id.ToString();

            // On filtre uniquement les présences du mois affiché
            var filtered = _presences
                .Where(p =>
                    p.StagiaireId != null &&
                    p.StagiaireId.Equals(id, StringComparison.OrdinalIgnoreCase) &&
                    p.Date.Date >= _monthStart &&
                    p.Date.Date <= _endDate)
                .OrderBy(p => p.Date)
                .ToList();

            // ── Comptage ──────────────────────────────────────────────────────
           // ── Comptage ──────────────────────────────────────────────────────────────────
int total    = filtered.Count;   // jours réels en DB pour le mois
int absents  = filtered.Count(p => N(p.Statut) == "a");
int excuses  = filtered.Count(p => N(p.Statut) == "e");
int retards  = filtered.Count(p => N(p.Statut) == "r");
int stages   = filtered.Count(p => N(p.Statut) == "s");
int vacances = filtered.Count(p => N(p.Statut) == "v");
int feries   = filtered.Count(p => N(p.Statut) == "f");

int totalMoisComptabilise = total - feries - vacances;

// Absences pondérées
double absenceScore = absents + excuses + (retards * 0.5);

// targetDays = 10 pour télécours mois 1
int targetDays  = _session.SessionTargetDays;
int totalAffiche = targetDays > 0 ? targetDays : totalMoisComptabilise;

// ✅ Présents = targetDays - absences (pas les jours réels en DB)
// On ne peut pas avoir plus de présents que le total cible
int presentsAffiche = Math.Max(0, (int)Math.Round(totalAffiche - absenceScore));

            // "Total jours de cours" affiché = jours cibles de la session pour ce mois
            // (ex: 10j pour télécours mois 1, pas les 22 jours calendaires)
            double sessionRate = targetDays > 0
                ? Math.Max(0, (targetDays - absenceScore) / targetDays * 100)
                : (totalMoisComptabilise > 0
                    ? (totalMoisComptabilise - absenceScore) / totalMoisComptabilise * 100
                    : 100);

            // ── Camembert (basé sur jours réels du mois) ──────────────────────
            double pieRate = totalMoisComptabilise > 0
                ? (totalMoisComptabilise - absenceScore) / totalMoisComptabilise * 100
                : 100;

            // ── Camembert — basé sur targetDays uniquement ─────────────────────────────
// On n'affiche pas vacances/fériés dans le camembert (ils sont exclus du calcul)
// Total du camembert = totalAffiche (targetDays)
var slices = new List<(string label, int count, SKColor skColor, Color pdfColor)>
{
    ("Présent",  presentsAffiche, SkPresent, ColPresent),
    ("Absent",   absents,         SkAbsent,  ColAbsent),
    ("Excusé",   excuses,         SkExcuse,  ColExcuse),
    ("Retard",   retards,         SkRetard,  ColRetard),
}.Where(s => s.count > 0).ToList();

// Le total du camembert = totalAffiche (pas le total réel des jours en DB)
int pieTotal = totalAffiche;

byte[] chartPng = GeneratePieChartPng(
    slices.Select(s => (s.label, s.count, s.skColor)).ToList(),
    pieTotal,       // ← base = 10j cibles, pas 22j réels
    sessionRate);

            // Période : début de la session → dernier jour du mois affiché
            string startFmt = _startDate.ToString("dd MMMM yyyy", _frenchCulture);
            string endFmt   = _endDate.ToString("dd MMMM yyyy", _frenchCulture);

            // Type de formation lisible
            bool   isTelecours  = _session.SessionType?.ToLower().Contains("tele") == true;
            string typeFormation = isTelecours ? "Français Télécours" : "Français & Intégration";

            container.Column(col =>
            {
                col.Spacing(20);

                // ── Titre ──────────────────────────────────────────────────────
                col.Item().AlignCenter()
                    .Text("ATTESTATION DE PRÉSENCE")
                    .Style(_titleStyle);

                // ── Participant ────────────────────────────────────────────────
                col.Item().AlignCenter().PaddingTop(5).Text(t =>
                {
                    t.Span("Participant-e : ").FontSize(14);
                    t.Span($"{_stagiaire.Firstname} {_stagiaire.LastName}")
                        .FontSize(14).Bold().FontColor(AccentColor);
                });

                // ── Type de formation ──────────────────────────────────────────
                col.Item().AlignCenter().Text(t =>
                {
                    t.Span("Formation : ").FontSize(11).FontColor(MediumGrey);
                    t.Span(typeFormation).FontSize(11).SemiBold().FontColor(PrimaryColor);
                });

                // ── Période (début session → fin mois affiché) ─────────────────
                col.Item().AlignCenter()
                    .Text($"Période du {startFmt} au {endFmt}").FontSize(12);

                // ── Info session ───────────────────────────────────────────────
                if (!string.IsNullOrEmpty(_session.SessionName))
                {
                    col.Item().AlignCenter().Text(t =>
                    {
                        t.Span($"Session : {_session.SessionName}  |  ").FontSize(10).FontColor(MediumGrey);
                        t.Span($"Mois {_session.SessionMonth}/3  ").FontSize(10).FontColor(MediumGrey);
                        t.Span($"({_session.SessionTargetDays}j cumulés attendus)")
                            .FontSize(10).FontColor(AccentColor);
                    });
                }

                // ── Résumé ─────────────────────────────────────────────────────
                col.Item().PaddingTop(10).Border(1).BorderColor(Colors.Grey.Lighten3)
                    .Padding(15).Column(summary =>
                {
                    summary.Spacing(8);

                    // Ligne 1 : total · présents · retards
                    summary.Item().Row(r =>
                    {
                        r.RelativeItem().Text(text =>
                        {
                            text.Span("Total jours de cours : ").SemiBold();
                            text.Span($"{totalAffiche}");
                        });
                        // Ligne "Jours présents"
r.RelativeItem().Text(text =>
{
    text.Span("Jours présents : ").SemiBold().FontColor(Colors.Green.Darken2);
    text.Span($"{presentsAffiche}");
    if (stages > 0)
        text.Span($"  (dont {stages} stage)").FontSize(9).FontColor(ColStage);
});
                        r.RelativeItem().Text(text =>
                        {
                            text.Span("Retards : ").SemiBold().FontColor(Colors.Blue.Medium);
                            text.Span($"{retards}");
                            text.Span(" (= 0.5j abs)").FontSize(8).FontColor(MediumGrey);
                        });
                    });

                    // Ligne 2 : absents · excusés (séparés) · vacances · fériés
                    summary.Item().Row(r =>
                    {
                        r.RelativeItem().Text(text =>
                        {
                            text.Span("Absents : ").SemiBold().FontColor(Colors.Red.Darken2);
                            text.Span($"{absents}");
                            // Excusés affichés en dessous, pas "dont"
                            if (excuses > 0)
                            {
                                text.Span("  |  Excusés : ").FontSize(9).FontColor(Colors.Orange.Darken2);
                                text.Span($"{excuses}").FontSize(9).FontColor(Colors.Orange.Darken2);
                            }
                        });
                        r.RelativeItem().Text(text =>
                        {
                            text.Span("Vacances : ").SemiBold().FontColor(Colors.Green.Medium);
                            text.Span($"{vacances}");
                        });
                        r.RelativeItem().Text(text =>
                        {
                            text.Span("Fériés : ").SemiBold().FontColor(Colors.Yellow.Darken3);
                            text.Span($"{feries}");
                        });
                    });

                    summary.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    // ── Taux session uniquement ────────────────────────────────
                    summary.Item().AlignCenter().Text(text =>
                    {
                        text.Span("Taux de participation : ").FontSize(14).Bold();
                        var c = sessionRate >= 80 ? Colors.Green.Darken2
                              : sessionRate >= 50 ? Colors.Orange.Darken2
                              : Colors.Red.Darken2;
                        text.Span($"{sessionRate:F1}%").FontSize(16).Bold().FontColor(c);
                        if (targetDays > 0)
                            text.Span($"  / {targetDays}j").FontSize(10).FontColor(MediumGrey);
                    });

                    summary.Item().Text(
                        "Calcul : Présent=1j  |  Retard=0,5j absence  |  Excusé=absence  |  Fériés/Vacances exclus")
                        .FontSize(8).FontColor(MediumGrey).Italic();
                });

                // ── Camembert + légende ────────────────────────────────────────
                col.Item().Row(row =>
                {
                    row.ConstantItem(220).Height(220).AlignMiddle()
                        .Image(chartPng).FitArea();

                    row.RelativeItem().AlignMiddle().PaddingLeft(14).Column(legend =>
                    {
                        legend.Spacing(5);
                        legend.Item().Text("Répartition des jours")
                            .FontSize(11).Bold().FontColor(PrimaryColor);
                        legend.Item().PaddingBottom(3)
                            .LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        foreach (var (label, count, _, pdfColor) in slices)
{
    double pct = pieTotal > 0 ? (double)count / pieTotal * 100 : 0;
    legend.Item().Row(r =>
    {
        r.ConstantItem(12).Height(12).Background(pdfColor);
        r.ConstantItem(6);
        r.RelativeItem().AlignMiddle().Text(t =>
        {
            t.Span($"{label} : ").FontSize(10);
            t.Span($"{count} j").Bold().FontSize(10);
            t.Span($"  ({pct:F0}%)").FontSize(9).FontColor(MediumGrey);
        });
    });
}

// Séparateur vacances/fériés (exclus du calcul)
if (vacances > 0 || feries > 0)
{
    legend.Item().PaddingTop(4).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
}

if (vacances > 0)
{
    legend.Item().Row(r =>
    {
        r.ConstantItem(12).Height(12).Background(ColVacances);
        r.ConstantItem(6);
        r.RelativeItem().AlignMiddle().Text(t =>
        {
            t.Span("Vacances : ").FontSize(9).FontColor(MediumGrey);
            t.Span($"{vacances} j").Bold().FontSize(9).FontColor(MediumGrey);
            t.Span(" (exclus)").FontSize(8).FontColor(MediumGrey).Italic();
        });
    });
}

if (feries > 0)
{
    legend.Item().Row(r =>
    {
        r.ConstantItem(12).Height(12).Background(ColFerie);
        r.ConstantItem(6);
        r.RelativeItem().AlignMiddle().Text(t =>
        {
            t.Span("Fériés : ").FontSize(9).FontColor(MediumGrey);
            t.Span($"{feries} j").Bold().FontSize(9).FontColor(MediumGrey);
            t.Span(" (exclus)").FontSize(8).FontColor(MediumGrey).Italic();
        });
    });
}
                    });
                });

                // ── Date d'impression (bas de page, sans signature) ────────────
                col.Item().PaddingTop(10).AlignRight().Text(
                    $"Lausanne, le {_endDate.ToString("dd MMMM yyyy", _frenchCulture)}")
                    .FontSize(9).FontColor(MediumGrey).Italic();
            });
        }

        // ── Pied de page ──────────────────────────────────────────────────────
        private void RenderFooter(IContainer container)
        {
            container.PaddingTop(10).BorderTop(1).BorderColor(Colors.Grey.Lighten2)
                .AlignCenter()
                .Text("Association Confluences - Document de suivi interne")
                .FontSize(8).FontColor(MediumGrey);
        }

        // ── Camembert SkiaSharp ───────────────────────────────────────────────
        private static byte[] GeneratePieChartPng(
            List<(string label, int count, SKColor color)> slices,
            int total,
            double presenceScore)
        {
            const int size = 500;
            const int cx   = size / 2;
            const int cy   = size / 2;
            const float r  = size / 2f - 24f;

            using var surface = SKSurface.Create(
                new SKImageInfo(size, size, SKColorType.Rgba8888, SKAlphaType.Premul));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            if (total == 0 || slices.Count == 0)
            {
                using var emptyPaint = new SKPaint
                    { Color = new SKColor(220,220,220), IsAntialias = true, Style = SKPaintStyle.Fill };
                canvas.DrawCircle(cx, cy, r, emptyPaint);
            }
            else
            {
                float startAngle = -90f;

                foreach (var (_, count, color) in slices)
                {
                    float sweep = (float)count / total * 360f;
                    var   rect  = new SKRect(cx - r, cy - r, cx + r, cy + r);

                    using var fillPaint = new SKPaint
                        { Color = color, IsAntialias = true, Style = SKPaintStyle.Fill };
                    using var path = new SKPath();
                    path.MoveTo(cx, cy);
                    path.ArcTo(rect, startAngle, sweep, false);
                    path.Close();
                    canvas.DrawPath(path, fillPaint);

                    using var sep = new SKPaint
                    {
                        Color = SKColors.White, IsAntialias = true,
                        Style = SKPaintStyle.Stroke, StrokeWidth = 2.5f
                    };
                    canvas.DrawPath(path, sep);

                    float pct = (float)count / total * 100f;
                    if (pct >= 5f)
                    {
                        float mid = startAngle + sweep / 2f;
                        float rad = (float)(mid * Math.PI / 180.0);
                        float lr  = r * 0.64f;
                        float lx  = cx + lr * (float)Math.Cos(rad);
                        float ly  = cy + lr * (float)Math.Sin(rad);

                        using var tp = new SKPaint
                        {
                            Color = SKColors.White, IsAntialias = true,
                            TextSize = 24f, FakeBoldText = true, TextAlign = SKTextAlign.Center
                        };
                        using var sp = new SKPaint
                        {
                            Color = new SKColor(0,0,0,80), IsAntialias = true,
                            TextSize = 24f, FakeBoldText = true, TextAlign = SKTextAlign.Center
                        };
                        canvas.DrawText($"{pct:F0}%", lx + 1, ly + 10, sp);
                        canvas.DrawText($"{pct:F0}%", lx,     ly + 9,  tp);
                    }
                    startAngle += sweep;
                }

                // Trou central
                float holeR = r * 0.36f;
                using var holePaint = new SKPaint
                    { Color = SKColors.White, IsAntialias = true, Style = SKPaintStyle.Fill };
                canvas.DrawCircle(cx, cy, holeR, holePaint);

                using var holeBorder = new SKPaint
                {
                    Color = new SKColor(220,220,220), IsAntialias = true,
                    Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f
                };
                canvas.DrawCircle(cx, cy, holeR, holeBorder);

                // Taux au centre
                using var bigText = new SKPaint
                {
                    Color = SKColor.Parse("#2D3748"), IsAntialias = true,
                    TextSize = 28f, FakeBoldText = true, TextAlign = SKTextAlign.Center
                };
                using var subText = new SKPaint
                {
                    Color = SKColor.Parse("#718096"), IsAntialias = true,
                    TextSize = 16f, TextAlign = SKTextAlign.Center
                };
                canvas.DrawText($"{presenceScore:F0}%", cx, cy + 11, bigText);
                canvas.DrawText("présence",             cx, cy + 33, subText);
            }

            using var img  = surface.Snapshot();
            using var data = img.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        private static string N(string statut)
            => string.IsNullOrWhiteSpace(statut) ? "p" : statut.Trim().ToLower();
    }
}