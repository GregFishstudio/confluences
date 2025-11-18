using Confluences.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;

namespace Api.Documents
{
    public class BilanTrimestrielDocument
    {
        private readonly Stage _stage;
        private readonly DateTime _dateDebut;
        private readonly DateTime _dateFin;

        public BilanTrimestrielDocument(Stage stage, DateTime dateDebut, DateTime dateFin)
        {
            _stage = stage;
            _dateDebut = dateDebut;
            _dateFin = dateFin;
        }

        public byte[] Generate()
        {
            var stagiaire = _stage.Stagiaire;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header()
                        .Text("Bilan trimestriel")
                        .FontSize(26)
                        .Bold();

                    page.Content().Column(col =>
                    {
                        col.Spacing(12);

                        col.Item().Text($"Stagiaire : {stagiaire.Firstname} {stagiaire.LastName}");
                        col.Item().Text($"Période : {_dateDebut:dd.MM.yyyy} → {_dateFin:dd.MM.yyyy}");
                        col.Item().PaddingTop(20).Text("Résumé :");

                        col.Item().Text("• Compétences acquises : …");
                        col.Item().Text("• Heures effectuées : …");
                        col.Item().Text("• Points forts : …");
                        col.Item().Text("• Axes d’amélioration : …");

                        col.Item().PaddingTop(40)
                            .Text("Signature : ____________________");
                    });

                    page.Footer().AlignCenter().Text("Confluences – document généré automatiquement");
                });
            })
            .GeneratePdf();
        }
    }
}
