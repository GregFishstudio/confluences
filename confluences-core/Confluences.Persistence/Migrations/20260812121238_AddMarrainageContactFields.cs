using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class AddMarrainageContactFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailFilleul",
                table: "Marrainages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RemarqueParrain",
                table: "Marrainages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelephoneFilleul",
                table: "Marrainages",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailFilleul",
                table: "Marrainages");

            migrationBuilder.DropColumn(
                name: "RemarqueParrain",
                table: "Marrainages");

            migrationBuilder.DropColumn(
                name: "TelephoneFilleul",
                table: "Marrainages");
        }
    }
}
