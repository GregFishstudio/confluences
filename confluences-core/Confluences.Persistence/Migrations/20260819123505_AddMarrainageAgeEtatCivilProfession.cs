using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class AddMarrainageAgeEtatCivilProfession : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "Marrainages",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EtatCivil",
                table: "Marrainages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profession",
                table: "Marrainages",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                table: "Marrainages");

            migrationBuilder.DropColumn(
                name: "EtatCivil",
                table: "Marrainages");

            migrationBuilder.DropColumn(
                name: "Profession",
                table: "Marrainages");
        }
    }
}
