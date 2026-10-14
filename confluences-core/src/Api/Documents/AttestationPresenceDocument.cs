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

        // ── Contenu principal ─────────────────────────────────────────────────
        private void RenderContent(IContainer container)
        {
            var id = _stagiaire.Id.ToString();

            var filteredAll = _presences
                .Where(p =>
                    p.StagiaireId != null &&
                    p.StagiaireId.Equals(id, StringComparison.OrdinalIgnoreCase) &&
                    p.Date.Date >= _startDate &&
                    p.Date.Date <= _endDate)
                .OrderBy(p => p.Date)
                .ToList();

            // "n" = jour hors inscription (avant DateDebutParticipation / après une interruption) :
            // ni absence, ni déduit des jours cibles — même calcul que la fiche de présence, qui
            // fait référence.
            var filtered   = filteredAll.Where(p => N(p.Statut) != "n").ToList();

            bool isTelecours = _session.SessionType?.ToLower().Contains("tele") == true;

            int absents  = filtered.Count(p => N(p.Statut) == "a");
            int excuses  = filtered.Count(p => N(p.Statut) == "e");
            int retards  = filtered.Count(p => N(p.Statut) == "r");
            int stages   = filtered.Count(p => N(p.Statut) == "s");
            int visites  = filtered.Count(p => N(p.Statut) == "v"); // visite à domicile (télécours)
            // Seuls les jours de cours sont transmis (télécours : lun/mar/jeu) : fériés et vacances
            // tombant un jour de cours réduisent les jours cibles, comme dans la fiche de présence.
            int feries   = filtered.Count(p => N(p.Statut) == "f");
            int vacances = filtered.Count(p => N(p.Statut) == "vac");
            int total    = filtered.Count;

            double absenceScore = absents + excuses + (retards * 0.5);
            int    targetDays   = _session.SessionTargetDays;

            int totalAffiche = targetDays > 0
                ? Math.Max(1, targetDays - feries - vacances)
                : (total - feries - vacances - visites);

            double presentsExact   = Math.Max(0.0, totalAffiche - absenceScore);

            // Taux de présence = 100 − taux d'absence arrondi comme dans la fiche de présence
            // (arrondi demi-bas : 12.5→12, 12.6→13), pour que les deux chiffres correspondent toujours.
            double sessionRate = totalAffiche > 0
                ? Math.Max(0.0, 100.0 - Math.Ceiling(absenceScore / totalAffiche * 100.0 - 0.5))
                : 100.0;

            // Les visites à domicile (télécours) sont une catégorie à part — ni une présence en
            // classe, ni une absence — et ne doivent donc pas gonfler le carré "Présences" (elles
            // ont leur propre carré et leur propre part de camembert, cohérents entre eux).
            double visitesSlice  = isTelecours ? visites : 0.0;
            double presentsSlice = Math.Max(0.0, presentsExact - visitesSlice);
            int    presentsAffiche = (int)Math.Round(presentsSlice);

            // label = texte long pour la légende ; pieLabel = texte affiché directement sur le camembert
            var slices = new List<(string label, string pieLabel, double count, SKColor skColor, Color pdfColor)>
            {
                ("Présent·e",              "Présent·e",        presentsSlice, SkPresent, ColPresent),
                ("Visite à domicile",      "Visite à domicile", visitesSlice,  SkVisite,  ColVisite),
                ("Absent·e non justifié·e", "Absent·e non justifié·e", absents, SkAbsent, ColAbsent),
                ("Excusé·e",               "Excusé·e",         excuses,       SkExcuse,  ColExcuse),
                ("Participation partielle","Part. part.",      retards * 0.5, SkRetard,  ColRetard),
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
                    if (_stagiaire.DateFinParticipation.HasValue)
                    {
                        string interruptionDate = _stagiaire.DateFinParticipation.Value.ToString("dd MMMM yyyy", _frenchCulture);
                        string motif = string.IsNullOrWhiteSpace(_stagiaire.RaisonInterruptionMesure)
                            ? ""
                            : $" Motif : {_stagiaire.RaisonInterruptionMesure}.";
                        infoRows.Add(("", $"Mesure interrompue le {interruptionDate}.{motif}"));
                    }
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
                            .SemiBold().FontSize(10).FontColor(Colors.White);

                        table.Cell()
                            .BorderBottom(0.5f).BorderLeft(0.5f).BorderColor(Colors.Grey.Lighten2)
                            .Padding(7).PaddingLeft(10)
                            .Text(rowValue)
                            .FontSize(10).FontColor(PrimaryColor);
                    }
                });

                // ── Boîtes statistiques ────────────────────────────────────────
                col.Item().Row(row =>
                {
                    row.RelativeItem().Element(c => StatBox(c,
                        $"{presentsAffiche}", Plural(presentsAffiche, "Présence", "Présences"),
                        Color.FromHex("#E0F5F3"), AccentColor));

                    row.RelativeItem().Element(c => StatBox(c,
                        $"{absents}", Plural(absents, "Absence non justifiée", "Absences non justifiées"),
                        Color.FromHex("#FDECEA"), Color.FromHex("#C0392B")));

                    row.RelativeItem().Element(c => StatBox(c,
                        $"{excuses}", Plural(excuses, "Absence Excusée", "Absences Excusées"),
                        Color.FromHex("#FEF9E7"), Color.FromHex("#E67E22")));

                    row.RelativeItem().Element(c => StatBox(c,
                        $"{retards}", Plural(retards, "Participation partielle", "Participations partielles"),
                        Color.FromHex("#E8F4FC"), Color.FromHex("#1A6EA8")));

                    if (stages > 0)
                        row.RelativeItem().Element(c => StatBox(c,
                            $"{stages}", Plural(stages, "Journée Stage", "Journées Stages"),
                            Color.FromHex("#E5F7E0"), Color.FromHex("#3E8E41")));

                    if (isTelecours && visites > 0)
                        row.RelativeItem().Element(c => StatBox(c,
                            $"{visites}", Plural(visites, "Visite à domicile", "Visites à domicile"),
                            Color.FromHex("#F3E5F5"), Color.FromHex("#7B1FA2")));
                });

                // ── Titre de section ───────────────────────────────────────────
                col.Item().PaddingTop(4)
                    .Text("Répartition des présences")
                    .FontSize(13).Bold().FontColor(AccentColor);

                // ── Camembert + pourcentage ────────────────────────────────────
                col.Item().Row(row =>
                {
                    row.ConstantItem(300).Height(250).Image(chartPng).FitArea();

                    row.RelativeItem().AlignMiddle().PaddingLeft(12).Column(c =>
                    {
                        c.Item().AlignCenter()
                            .Text($"{sessionRate:F0}%")
                            .FontSize(36).Bold().FontColor(Color.FromHex("#2C5F8A"));
                        c.Item().AlignCenter()
                            .Text("de participation")
                            .FontSize(11).FontColor(MediumGrey);
                    });
                });

                // ── Légende ────────────────────────────────────────────────────
                col.Item().Row(row =>
                {
                    // "Absent·e" et "Participation partielle" figurent déjà dans les boîtes
                    // statistiques ci-dessus — inutile de les répéter dans la légende du camembert.
                    var legendSlices = slices.Where(s =>
                        s.label != "Présent·e" && s.label != "Excusé·e" &&
                        s.label != "Absent·e non justifié·e" && s.label != "Participation partielle");

                    foreach (var (label, _, count, _, pdfColor) in legendSlices)
                    {
                        bool isPP    = label == "Participation partielle";
                        string valStr = isPP
                            ? $"{retards} fois"
                            : FormatJours(count);
                        row.AutoItem().PaddingRight(12).Row(r =>
                        {
                            r.ConstantItem(12).Height(12).AlignMiddle().Background(pdfColor);
                            r.ConstantItem(5);
                            r.AutoItem().AlignMiddle().Text(t =>
                            {
                                t.Span($"{label} : ").FontSize(9f);
                                t.Span(valStr).Bold().FontSize(9f);
                            });
                        });
                    }

                    // Fériés uniquement — Visite à dom. est déjà dans les slices ci-dessus
                    if (feries > 0)
                        row.AutoItem().Row(r =>
                        {
                            r.ConstantItem(12).Height(12).AlignMiddle().Background(ColFerie);
                            r.ConstantItem(5);
                            r.AutoItem().AlignMiddle().Text(t =>
                            {
                                t.Span("Fériés : ").FontSize(9f).FontColor(MediumGrey);
                                t.Span($"{feries} j").Bold().FontSize(9f).FontColor(MediumGrey);
                            });
                        });
                });
            });
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
            // ex: 0.5 → "0,5 jour"  |  1.5 → "1,5 jour"
            return count.ToString("0.#", new CultureInfo("fr-FR")) + " jour";
        }

        private static string N(string statut)
            => string.IsNullOrWhiteSpace(statut) ? "p" : statut.Trim().ToLower();

        private static string Plural(double n, string singular, string plural)
            => n <= 1 ? singular : plural;
    }
}
