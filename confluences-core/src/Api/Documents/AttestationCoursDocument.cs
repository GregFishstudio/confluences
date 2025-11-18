using Confluences.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;

namespace Api.Documents
{
    public class AttestationCoursDocument
    {
        private readonly Stage _stage;

        public AttestationCoursDocument(Stage stage)
        {
            _stage = stage;
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
                        .Text("Attestation de cours")
                        .FontSize(26)
                        .Bold();

                    page.Content().Column(col =>
                    {
                        col.Spacing(12);

                        col.Item().Text($"Stagiaire : {stagiaire.Firstname} {stagiaire.LastName}");
                        col.Item().Text($"Date début : {_stage.Debut:dd.MM.yyyy}");
                        col.Item().Text($"Date fin : {_stage.Fin:dd.MM.yyyy}");

                        col.Item().PaddingTop(20)
                            .Text("Nous attestons que le stagiaire a participé aux activités prévues.");

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
