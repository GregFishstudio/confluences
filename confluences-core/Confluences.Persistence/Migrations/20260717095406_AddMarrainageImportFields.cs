using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class AddMarrainageImportFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CharteSignee",
                table: "Marrainages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ExtraitCasier",
                table: "Marrainages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FilleulNom",
                table: "Marrainages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PeriodeDemande",
                table: "Marrainages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelanceNotes",
                table: "Marrainages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Suivi",
                table: "Marrainages",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CharteSignee",
                table: "Marrainages");

            migrationBuilder.DropColumn(
                name: "ExtraitCasier",
                table: "Marrainages");

            migrationBuilder.DropColumn(
                name: "FilleulNom",
                table: "Marrainages");

            migrationBuilder.DropColumn(
                name: "PeriodeDemande",
                table: "Marrainages");

            migrationBuilder.DropColumn(
                name: "RelanceNotes",
                table: "Marrainages");

            migrationBuilder.DropColumn(
                name: "Suivi",
                table: "Marrainages");
        }
    }
}
