using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class AddMarrainageQuestionnaire : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MarrainageQuestions",
                columns: table => new
                {
                    MarrainageQuestionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Label = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Options = table.Column<string>(type: "text", nullable: true),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarrainageQuestions", x => x.MarrainageQuestionId);
                });

            migrationBuilder.CreateTable(
                name: "MarrainageAnswers",
                columns: table => new
                {
                    MarrainageAnswerId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MarrainageId = table.Column<int>(type: "integer", nullable: false),
                    MarrainageQuestionId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarrainageAnswers", x => x.MarrainageAnswerId);
                    table.ForeignKey(
                        name: "FK_MarrainageAnswers_MarrainageQuestions_MarrainageQuestionId",
                        column: x => x.MarrainageQuestionId,
                        principalTable: "MarrainageQuestions",
                        principalColumn: "MarrainageQuestionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MarrainageAnswers_Marrainages_MarrainageId",
                        column: x => x.MarrainageId,
                        principalTable: "Marrainages",
                        principalColumn: "MarrainageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarrainageAnswers_MarrainageId",
                table: "MarrainageAnswers",
                column: "MarrainageId");

            migrationBuilder.CreateIndex(
                name: "IX_MarrainageAnswers_MarrainageQuestionId",
                table: "MarrainageAnswers",
                column: "MarrainageQuestionId");

            // Questions du questionnaire d'inscription parrain/marraine (premier entretien).
            var questions = new (string Label, string Type, string Options, bool Required)[]
            {
                ("Adresse e-mail", "text", null, true),
                ("Nom et prénom", "text", null, true),
                ("Age", "number", null, true),
                ("Etat civil", "text", null, true),
                ("Commune de résidence", "text", null, true),
                ("Profession", "text", null, true),
                ("Numéro de téléphone", "text", null, true),
                ("Comment avez-vous entendu parler de ce parrainage / marrainage dans notre association ?", "choice",
                    "site internet de Confluences|affiche / tract|par une connaissance|par le site benevol-jobs.ch|Autre", true),
                ("Avez-vous déjà participé à une action de parrainage / marrainage ?", "textarea", null, false),
                ("Avez-vous d'autres engagements bénévoles ? Si oui lequel/dans quelle association ?", "textarea", null, false),
                ("En quelques mots, pouvez-vous nous donner des informations sur vous et vos centres d'intérêt.", "textarea", null, true),
                ("Quelles sont les raisons ou les motivations qui vous poussent à vous engager comme parrain / marraine ?", "textarea", null, true),
                ("Avez-vous des attentes particulières par rapport à ce parrainage / marrainage ?", "textarea", null, false),
                ("Dans l'idéal, vous aimeriez rencontrer :", "choice", "une femme|un homme|une famille|je n'ai pas de préférence", true),
                ("Dans quelle tranche d'âge ?", "text", null, false),
                ("Préférez-vous rencontrer votre filleul·e seul·e ou avec ses enfants ?", "text", null, true),
                ("Décrivez ce que vous souhaitez proposer comme activités à votre \"filleul-e\" :", "textarea", null, false),
                ("Quelles sont vos disponibilités ?", "textarea", null, false),
                ("À quelle fréquence souhaitez-vous rencontrer votre \"filleul-e\" ?", "text", null, true),
                ("Où pouvez-vous rencontrer ?", "text", null, true),
                ("Comment vous déplacez-vous ?", "text", null, true),
                ("Avez-vous pensé à la durée de votre engagement ? long terme ou durée déterminée", "text", null, false),
                ("Avez-vous des inquiétudes ou des questions ?", "textarea", null, false),
            };

            for (int i = 0; i < questions.Length; i++)
            {
                var q = questions[i];
                migrationBuilder.InsertData(
                    table: "MarrainageQuestions",
                    columns: new[] { "Label", "Type", "Options", "Required", "Order", "Active" },
                    values: new object[] { q.Label, q.Type, q.Options, q.Required, i + 1, true });
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarrainageAnswers");

            migrationBuilder.DropTable(
                name: "MarrainageQuestions");
        }
    }
}
