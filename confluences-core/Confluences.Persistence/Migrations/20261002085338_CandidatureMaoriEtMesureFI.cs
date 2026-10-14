using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class CandidatureMaoriEtMesureFI : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ContratMaori",
                table: "Candidatures",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Mesure : "A1"/"A2" deviennent "F&I", le niveau passe dans le statut de liste d'attente.
            migrationBuilder.Sql(@"
                UPDATE ""Candidatures""
                SET ""Statut"" = 'Sur liste d''attente ' || ""MesureVisee""
                WHERE ""Statut"" = 'Sur liste d''attente' AND ""MesureVisee"" IN ('A1', 'A2');

                UPDATE ""Candidatures""
                SET ""MesureVisee"" = 'F&I'
                WHERE ""MesureVisee"" IN ('A1', 'A2');");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContratMaori",
                table: "Candidatures");
        }
    }
}
