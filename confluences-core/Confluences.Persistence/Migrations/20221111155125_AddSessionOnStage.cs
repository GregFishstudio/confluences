using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class AddSessionOnStage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SessionId",
                table: "Stages",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stages_SessionId",
                table: "Stages",
                column: "SessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Stages_Sessions_SessionId",
                table: "Stages",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "SessionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stages_Sessions_SessionId",
                table: "Stages");

            migrationBuilder.DropIndex(
                name: "IX_Stages_SessionId",
                table: "Stages");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "Stages");
        }
    }
}
