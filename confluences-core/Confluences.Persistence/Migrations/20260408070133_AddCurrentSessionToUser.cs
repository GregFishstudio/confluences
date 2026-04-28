using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class AddCurrentSessionToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentSessionNumber",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentSessionYear",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentSessionNumber",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CurrentSessionYear",
                table: "AspNetUsers");
        }
    }
}
