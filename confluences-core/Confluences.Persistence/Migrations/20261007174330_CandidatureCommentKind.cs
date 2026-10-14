using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class CandidatureCommentKind : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Kind",
                table: "CandidatureComments",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            // Les anciennes saisies libres "Transmission AS / ML" deviennent la première remarque
            // datée de l'historique correspondant (la colonne d'origine est conservée).
            migrationBuilder.Sql(@"
INSERT INTO ""CandidatureComments"" (""CandidatureId"", ""Text"", ""CreatedAt"", ""Kind"")
SELECT ""CandidatureId"", ""TransmissionAS"", COALESCE(""UpdatedAt"", ""CreatedAt""), 'transmission'
FROM ""Candidatures""
WHERE COALESCE(TRIM(""TransmissionAS""), '') <> '';");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kind",
                table: "CandidatureComments");
        }
    }
}
