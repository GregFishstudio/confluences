using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class HistoriqueStatutsCommentairesParrain : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Portee",
                table: "MarrainageComments",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CandidatureStatutChanges",
                columns: table => new
                {
                    CandidatureStatutChangeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CandidatureId = table.Column<int>(type: "integer", nullable: false),
                    AncienStatut = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Statut = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Motif = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Commentaire = table.Column<string>(type: "text", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ChangedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidatureStatutChanges", x => x.CandidatureStatutChangeId);
                    table.ForeignKey(
                        name: "FK_CandidatureStatutChanges_AspNetUsers_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CandidatureStatutChanges_Candidatures_CandidatureId",
                        column: x => x.CandidatureId,
                        principalTable: "Candidatures",
                        principalColumn: "CandidatureId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidatureStatutChanges_CandidatureId_ChangedAt",
                table: "CandidatureStatutChanges",
                columns: new[] { "CandidatureId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CandidatureStatutChanges_ChangedByUserId",
                table: "CandidatureStatutChanges",
                column: "ChangedByUserId");

            // Date de naissance des parrains/marraines reprise de leur réponse au questionnaire
            // (« 03.03.1992 », « 3/3/1992 », « 1992-03-03 ») quand le champ de la fiche est vide.
            // Une réponse qui n'est pas une date valide est ignorée.
            migrationBuilder.Sql(@"
UPDATE ""Marrainages"" m SET ""DateNaissance"" = x.dn
FROM (
    SELECT DISTINCT ON (t.id) t.id,
        CASE WHEN t.mo BETWEEN 1 AND 12 AND t.an BETWEEN 1901 AND 2100 THEN
            CASE WHEN t.jo BETWEEN 1 AND extract(day FROM make_date(t.an, t.mo, 1) + interval '1 month - 1 day')::int
                THEN make_date(t.an, t.mo, t.jo) END
        END AS dn
    FROM (
        SELECT a.""MarrainageId"" AS id,
            CASE WHEN f.m IS NOT NULL THEN f.m[1]::int ELSE i.m[3]::int END AS jo,
            CASE WHEN f.m IS NOT NULL THEN f.m[2]::int ELSE i.m[2]::int END AS mo,
            CASE WHEN f.m IS NOT NULL THEN f.m[3]::int ELSE i.m[1]::int END AS an
        FROM ""MarrainageAnswers"" a
        JOIN ""MarrainageQuestions"" q ON q.""MarrainageQuestionId"" = a.""MarrainageQuestionId""
        CROSS JOIN LATERAL regexp_match(btrim(a.""Value""), '^([0-9]{1,2})[./-]([0-9]{1,2})[./-]([0-9]{4})$') AS f(m)
        CROSS JOIN LATERAL regexp_match(btrim(a.""Value""), '^([0-9]{4})-([0-9]{2})-([0-9]{2})$') AS i(m)
        WHERE lower(q.""Label"") LIKE '%naissance%' AND (f.m IS NOT NULL OR i.m IS NOT NULL)
    ) t
    ORDER BY t.id, dn NULLS LAST
) x
WHERE m.""MarrainageId"" = x.id AND m.""DateNaissance"" IS NULL
    AND x.dn IS NOT NULL AND x.dn < CURRENT_DATE;");

            // Contrat Maori coché sur la candidature d'une personne déjà inscrite : reporté sur sa
            // fiche participant (jusqu'ici, seulement lors de l'inscription dans une session).
            migrationBuilder.Sql(@"
UPDATE ""AspNetUsers"" u SET ""ContratMaori"" = TRUE
FROM ""Candidatures"" c
WHERE c.""ParticipantId"" = u.""Id"" AND c.""ContratMaori"" AND NOT u.""ContratMaori"";");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidatureStatutChanges");

            migrationBuilder.DropColumn(
                name: "Portee",
                table: "MarrainageComments");
        }
    }
}
