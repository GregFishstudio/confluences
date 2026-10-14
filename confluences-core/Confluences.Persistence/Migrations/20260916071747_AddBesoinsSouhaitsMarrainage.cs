using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class AddBesoinsSouhaitsMarrainage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BesoinsSouhaitsMarrainage",
                table: "AspNetUsers",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BesoinsSouhaitsMarrainageCommentaire",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BesoinsSouhaitsMarrainage",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BesoinsSouhaitsMarrainageCommentaire",
                table: "AspNetUsers");
        }
    }
}
