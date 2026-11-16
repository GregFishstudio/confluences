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
        // Périodes de 4 semaines depuis le début de la session, jusqu'à celle de l'attestation
        // (la dernière) : relevé principal + suivi cumulatif.
        public List<PeriodeReleve> Periodes { get; set; } = new();
    }

    public class PeriodeReleve
    {
        public string   Label      { get; set; } = "";   // "Semaines 5 à 8"
        public DateTime Start      { get; set; }
        public DateTime End        { get; set; }
        public int      TargetDays { get; set; }         // jours programmés de la période (20, 10, 12…)
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
        private static readonly Color ColConge    = Color.FromHex("#8FA98F");

        private static readonly SKColor SkPresent = SKColor.Parse("#2CA6A4");
        private static readonly SKColor SkAbsent  = SKColor.Parse("#E07050");
        private static readonly SKColor SkExcuse  = SKColor.Parse("#B5C5D5");
        private static readonly SKColor SkRetard  = SKColor.Parse("#6BAED6");
        private static readonly SKColor SkVisite  = SKColor.Parse("#CE93D8");
        private static readonly SKColor SkConge   = SKColor.Parse("#8FA98F");

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
                .FontFamily("Arial").FontSize(11).FontColor(PrimaryColor);
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
                            .Text("Association Confluences").FontSize(9.5f).SemiBold().FontColor(PrimaryColor);
                        col.Item().Text("Chemin de Renens 52e · Lausanne").FontSize(9).FontColor(MediumGrey);
                        col.Item().Text("www.confluences.ch").FontSize(9).FontColor(MediumGrey);
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
                        .FontSize(10).FontColor(PrimaryColor);
                });
            });
        }

        // ── Statistiques d'une période ────────────────────────────────────────
        // Mêmes règles que la fiche de présence : absences = A + E + ½ R ; fériés, vacances et
        // congés autorisés (C) réduisent les jours retenus — un congé n'est jamais une absence ;
        // "n" = jour hors inscription (ni absence, ni déduit).
        private sealed class Stats
        {
            public int A, E, R, S, V, F, Vac, C, Total;
            public int Target;
            public double AbsScore => A + E + R * 0.5;
            public int Retenus => Target > 0
                ? Math.Max(1, Target - F - Vac - C)
                : Math.Max(0, Total - F - Vac - C - V);
            // Taux = 100 − taux d'absence arrondi demi-bas (12.5 → 12), comme la fiche de présence.
            public double Rate => Retenus > 0
                ? Math.Max(0.0, 100.0 - Math.Ceiling(AbsScore / Retenus * 100.0 - 0.5))
                : 100.0;
        }

        private Stats ComputeStats(DateTime start, DateTime end, int target)
        {
            var id = _stagiaire.Id.ToString();
            var days = _presences.Where(p =>
                    p.StagiaireId != null &&
                    p.StagiaireId.Equals(id, StringComparison.OrdinalIgnoreCase) &&
                    p.Date.Date >= start && p.Date.Date <= end &&
                    N(p.Statut) != "n")
                .Select(p => N(p.Statut))
                .ToList();
            return new Stats
            {
                A = days.Count(s => s == "a"), E = days.Count(s => s == "e"), R = days.Count(s => s == "r"),
                S = days.Count(s => s == "s"), V = days.Count(s => s == "v"), F = days.Count(s => s == "f"),
                Vac = days.Count(s => s == "vac"), C = days.Count(s => s == "c"), Total = days.Count,
                Target = target,
            };
        }

        // ── Contenu principal ─────────────────────────────────────────────────
        private void RenderContent(IContainer container)
        {
            bool isTelecours = _session.SessionType?.ToLower().Contains("tele") == true;

            // Périodes de la session jusqu'à celle de l'attestation ; la dernière est le relevé
            // principal, toutes alimentent le suivi cumulatif.
            var periodes = _session.Periodes?.Count > 0
                ? _session.Periodes
                : new List<PeriodeReleve> { new PeriodeReleve { Label = "Période", Start = _startDate, End = _endDate, TargetDays = _session.SessionTargetDays } };
            var courante = periodes[^1];
            var st = ComputeStats(courante.Start, courante.End, courante.TargetDays);

            int retenus = st.Retenus;
            double presentsExact = Math.Max(0.0, retenus - st.AbsScore);
            double sessionRate = st.Rate;

            // Les visites à domicile (télécours) sont une catégorie à part — ni une présence en
            // classe, ni une absence. Les congés autorisés ont leur propre part : ils sont visibles,
            // mais exclus des jours retenus.
            double visitesSlice  = isTelecours ? st.V : 0.0;
            double presentsSlice = Math.Max(0.0, presentsExact - visitesSlice);
            int    presentsAffiche = (int)Math.Round(presentsSlice);

            var slices = new List<(string label, string pieLabel, double count, SKColor skColor, Color pdfColor)>
            {
                ("Présent·e",              "Présent·e",        presentsSlice, SkPresent, ColPresent),
                ("Visite à domicile",      "Visite à domicile", visitesSlice,  SkVisite,  ColVisite),
                ("Absent·e non justifié·e", "Absent·e non justifié·e", st.A,  SkAbsent,  ColAbsent),
                ("Excusé·e",               "Excusé·e",         st.E,          SkExcuse,  ColExcuse),
                ("Participation partielle","Part. part.",      st.R * 0.5,    SkRetard,  ColRetard),
                ("Congé autorisé",         "Congé autorisé",   st.C,          SkConge,   ColConge),
            }.Where(s => s.count > 0).ToList();

            byte[] chartPng = GeneratePieChartPng(
                slices.Select(s => (s.pieLabel, s.count, s.skColor)).ToList(),
                retenus + st.C);

            string startFmt = courante.Start.ToString("dd MMMM yyyy", _frenchCulture);
            string endFmt   = courante.End.ToString("dd MMMM yyyy", _frenchCulture);
            string mesureLabel = isTelecours ? "Mesure Français Télécours" : "Français & Intégration";

            container.Column(col =>
            {
                col.Spacing(9);

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
                        ("Participant·e", $"{_stagiaire.Firstname} {_stagiaire.LastName}"),
                        ("Formation",     mesureLabel),
                        ("Période",       $"{startFmt} au {endFmt}" + (periodes.Count > 1 || courante.Label != "Période" ? $" ({courante.Label.ToLower(_frenchCulture)})" : "")),
                    };
                    if (_stagiaire.DateFinParticipation.HasValue)
                    {
                        string interruptionDate = _stagiaire.DateFinParticipation.Value.ToString("dd MMMM yyyy", _frenchCulture);
                        string motif = string.IsNullOrWhiteSpace(_stagiaire.RaisonInterruptionMesure)
                            ? ""
                            : $" Motif : {_stagiaire.RaisonInterruptionMesure}.";
                        infoRows.Add(("", $"Mesure interrompue le {interruptionDate}.{motif}"));
                    }
                    if (!string.IsNullOrEmpty(_session.SessionName))
                        infoRows.Add(("Session", $"{_session.SessionName} | Période {_session.SessionMonth}/3"));

                    foreach (var (rowLabel, rowValue) in infoRows)
                    {
                        table.Cell()
                            .Background(AccentColor)
                            .BorderBottom(0.5f).BorderColor(Colors.White)
                            .Padding(6).PaddingLeft(10)
                            .Text(rowLabel)
                            .SemiBold().FontSize(10).FontColor(Colors.White);

                        table.Cell()
                            .BorderBottom(0.5f).BorderLeft(0.5f).BorderColor(Colors.Grey.Lighten2)
                            .Padding(6).PaddingLeft(10)
                            .Text(rowValue)
                            .FontSize(10).FontColor(PrimaryColor);
                    }
                });

                // ── Boîtes statistiques ────────────────────────────────────────
                col.Item().Row(row =>
                {
                    row.Spacing(4);
                    row.RelativeItem().Element(c => StatBox(c,
                        $"{presentsAffiche}", Plural(presentsAffiche, "Présence", "Présences"),
                        Color.FromHex("#E0F5F3"), AccentColor));

                    row.RelativeItem().Element(c => StatBox(c,
                        $"{st.A}", Plural(st.A, "Absence non justifiée", "Absences non justifiées"),
                        Color.FromHex("#FDECEA"), Color.FromHex("#C0392B")));

                    row.RelativeItem().Element(c => StatBox(c,
                        $"{st.E}", Plural(st.E, "Absence excusée", "Absences excusées"),
                        Color.FromHex("#FEF9E7"), Color.FromHex("#E67E22")));

                    row.RelativeItem().Element(c => StatBox(c,
                        $"{st.R}", Plural(st.R, "Participation partielle", "Participations partielles"),
                        Color.FromHex("#E8F4FC"), Color.FromHex("#1A6EA8")));

                    row.RelativeItem().Element(c => StatBox(c,
                        $"{st.C}", Plural(st.C, "Congé autorisé", "Congés autorisés"),
                        Color.FromHex("#EEF2EC"), Color.FromHex("#4E6E4E")));

                    if (st.S > 0)
                        row.RelativeItem().Element(c => StatBox(c,
                            $"{st.S}", Plural(st.S, "Journée de stage", "Journées de stage"),
                            Color.FromHex("#E5F7E0"), Color.FromHex("#3E8E41")));

                    if (isTelecours && st.V > 0)
                        row.RelativeItem().Element(c => StatBox(c,
                            $"{st.V}", Plural(st.V, "Visite à domicile", "Visites à domicile"),
                            Color.FromHex("#F3E5F5"), Color.FromHex("#7B1FA2")));
                });

                // ── Camembert + pourcentage ────────────────────────────────────
                col.Item().PaddingTop(2)
                    .Text("Répartition des présences")
                    .FontSize(12).Bold().FontColor(AccentColor);

                col.Item().Row(row =>
                {
                    row.ConstantItem(250).Height(170).Image(chartPng).FitArea();

                    row.RelativeItem().AlignMiddle().PaddingLeft(12).Column(c =>
                    {
                        c.Item().AlignCenter()
                            .Text($"{sessionRate:F0}%")
                            .FontSize(32).Bold().FontColor(Color.FromHex("#2C5F8A"));
                        c.Item().AlignCenter()
                            .Text("de participation sur la période")
                            .FontSize(10).FontColor(MediumGrey);
                        c.Item().PaddingTop(6).AlignCenter()
                            .Text($"{retenus} jour{(retenus > 1 ? "s" : "")} pris en compte sur {(st.Target > 0 ? st.Target : st.Total)} programmé{((st.Target > 0 ? st.Target : st.Total) > 1 ? "s" : "")}")
                            .FontSize(9).FontColor(PrimaryColor);
                        c.Item().AlignCenter()
                            .Text($"{FormatJours(st.AbsScore)} d'absence équivalente")
                            .FontSize(9).FontColor(MediumGrey);
                        if (st.C > 0)
                            c.Item().AlignCenter()
                                .Text($"{FormatJours(st.C)} de congé autorisé exclu{(st.C > 1 ? "s" : "")} (pas une absence)")
                                .FontSize(9).FontColor(Color.FromHex("#4E6E4E"));
                        if (st.F + st.Vac > 0)
                            c.Item().AlignCenter()
                                .Text($"{FormatJours(st.F + st.Vac)} férié{(st.F + st.Vac > 1 ? "s" : "")} / vacances exclu{(st.F + st.Vac > 1 ? "s" : "")}")
                                .FontSize(9).FontColor(MediumGrey);
                    });
                });

                // ── Suivi cumulatif depuis le début de la session ──────────────
                col.Item().PaddingTop(4)
                    .Text("Suivi cumulatif depuis le début de la session")
                    .FontSize(12).Bold().FontColor(AccentColor);

                var lignes = periodes.Select(p => (p.Label, Stats: ComputeStats(p.Start, p.End, p.TargetDays))).ToList();
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2.2f);
                        c.RelativeColumn(1.4f);
                        c.RelativeColumn(1.2f);
                        c.RelativeColumn(1.4f);
                        c.RelativeColumn(1f);
                    });

                    void Head(string t) => table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1)
                        .PaddingVertical(4).PaddingHorizontal(6).Text(t).FontSize(8.5f).SemiBold().FontColor(MediumGrey);
                    Head("Période");
                    Head("Jours programmés");
                    Head("Congés autorisés");
                    Head("Absence équivalente");
                    Head("Présence");

                    void Line(string label, Stats s, bool cumul)
                    {
                        var bg = cumul ? Color.FromHex("#EEF6F6") : Colors.White;
                        int prog = s.Target > 0 ? s.Target : s.Total;
                        string progTxt = s.Retenus != prog ? $"{prog} ({s.Retenus} retenus)" : $"{prog}";
                        void Cell(string t, bool bold = false, Color? color = null)
                        {
                            var text = table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                                .PaddingVertical(4).PaddingHorizontal(6).Text(t).FontSize(9).FontColor(color ?? PrimaryColor);
                            if (bold || cumul) text.Bold();
                        }
                        Cell(label);
                        Cell(progTxt);
                        Cell(s.C > 0 ? FormatJours(s.C) : "—");
                        Cell(FormatJours(s.AbsScore));
                        Cell($"{s.Rate:F0} %", true, cumul ? AccentColor : (Color?)null);
                    }

                    foreach (var (label, s) in lignes) Line(label, s, false);

                    if (lignes.Count > 1)
                    {
                        var cumul = new Stats
                        {
                            A = lignes.Sum(l => l.Stats.A), E = lignes.Sum(l => l.Stats.E), R = lignes.Sum(l => l.Stats.R),
                            S = lignes.Sum(l => l.Stats.S), V = lignes.Sum(l => l.Stats.V), F = lignes.Sum(l => l.Stats.F),
                            Vac = lignes.Sum(l => l.Stats.Vac), C = lignes.Sum(l => l.Stats.C), Total = lignes.Sum(l => l.Stats.Total),
                            Target = lignes.All(l => l.Stats.Target > 0) ? lignes.Sum(l => l.Stats.Target) : 0,
                        };
                        Line($"Cumul {CumulLabel(periodes)}", cumul, true);
                    }
                });

                col.Item().Text("Absence équivalente : une absence (excusée ou non) compte 1 jour, une participation partielle ½ jour. " +
                                "Les congés autorisés, fériés et vacances ne sont pas des absences : ils sont retirés des jours pris en compte.")
                    .FontSize(7.5f).FontColor(MediumGrey);
            });
        }

        // "Semaines 1 à 4" + "Semaines 9 à 12" → "semaines 1 à 12"
        private static string CumulLabel(List<PeriodeReleve> periodes)
        {
            var first = periodes[0].Label;
            var last  = periodes[^1].Label;
            const string prefix = "Semaines ";
            if (first.StartsWith(prefix) && last.StartsWith(prefix))
            {
                var from = first.Substring(prefix.Length).Split(' ')[0];
                var to   = last.Substring(prefix.Length).Split(' ').Last();
                return $"semaines {from} à {to}";
            }
            return "depuis le début";
        }

        // ── Boîte statistique colorée ─────────────────────────────────────────
        private static void StatBox(IContainer container, string value, string label,
            Color bgColor, Color valueColor)
        {
            container.Padding(1).Background(bgColor).Padding(10).AlignCenter().Column(c =>
            {
                c.Item().AlignCenter().Text(value).FontSize(24).Bold().FontColor(valueColor);
                c.Item().AlignCenter().Text(label).FontSize(10).FontColor(Color.FromHex("#4A4A4A"));
            });
        }

        // ── Camembert plein avec traits de repère et étiquettes ───────────────
        private static byte[] GeneratePieChartPng(
            List<(string label, double count, SKColor color)> slices,
            int total)
        {
            const int W = 800, H = 475;
            const float cx = 388f, cy = 238f;
            const float r  = 163f;

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
                    // SKPath.ArcTo dessine un arc invisible quand sweep == 360°
                    // (le point de départ et d'arrivée coïncident) → on plafonne légèrement.
                    float sweep = Math.Min(359.99f, (float)(count / grandTotal * 360.0));
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
                    float p2x = cx + (r + 35f) * cosA;
                    float p2y = cy + (r + 35f) * sinA;

                    bool right = cosA >= 0;
                    float p3x = right ? p2x + 33f : p2x - 33f;
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
                        TextSize = 26f, TextAlign = right ? SKTextAlign.Left : SKTextAlign.Right
                    };

                    // Libellés longs (ex. "Visite à domicile") : retour à la ligne plutôt que de
                    // dépasser le bord du canevas.
                    var lines = WrapLabel(label, txt, 150f);
                    float lineHeight = txt.TextSize + 4f;
                    float firstLineY = p3y + 7f - (lines.Count - 1) * lineHeight / 2f;
                    for (int li = 0; li < lines.Count; li++)
                        canvas.DrawText(lines[li], right ? p3x + 4f : p3x - 4f, firstLineY + li * lineHeight, txt);
                }
            }

            using var img  = surface.Snapshot();
            using var data = img.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        // Découpe un libellé trop large pour tenir sur une ligne en plusieurs lignes, en coupant
        // sur les espaces (jamais au milieu d'un mot).
        private static List<string> WrapLabel(string label, SKPaint paint, float maxWidth)
        {
            var words = (label ?? "").Split(' ');
            if (words.Length <= 1 || paint.MeasureText(label) <= maxWidth)
                return new List<string> { label };

            var lines = new List<string>();
            var current = words[0];
            for (int i = 1; i < words.Length; i++)
            {
                var candidate = current + " " + words[i];
                if (paint.MeasureText(candidate) <= maxWidth)
                    current = candidate;
                else
                {
                    lines.Add(current);
                    current = words[i];
                }
            }
            lines.Add(current);
            return lines;
        }

        private static string FormatJours(double count)
        {
            if (count == Math.Floor(count))
            {
                int n = (int)count;
                return $"{n} jour{(n > 1 ? "s" : "")}";
            }
            // ex: 0.5 → "0,5 jour"  |  1.5 → "1,5 jour"  |  4.5 → "4,5 jours"
            return count.ToString("0.#", new CultureInfo("fr-FR")) + (count >= 2 ? " jours" : " jour");
        }

        private static string N(string statut)
            => string.IsNullOrWhiteSpace(statut) ? "p" : statut.Trim().ToLower();

        private static string Plural(double n, string singular, string plural)
            => n <= 1 ? singular : plural;
    }
}
