using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class AddTypeIntershipActivityRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TypeIntershipActivityId",
                table: "Stages",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Stages_TypeIntershipActivityId",
                table: "Stages",
                column: "TypeIntershipActivityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Stages_TypeIntershipActivities_TypeIntershipActivityId",
                table: "Stages",
                column: "TypeIntershipActivityId",
                principalTable: "TypeIntershipActivities",
                principalColumn: "TypeIntershipActivityId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stages_TypeIntershipActivities_TypeIntershipActivityId",
                table: "Stages");

            migrationBuilder.DropIndex(
                name: "IX_Stages_TypeIntershipActivityId",
                table: "Stages");

            migrationBuilder.DropColumn(
                name: "TypeIntershipActivityId",
                table: "Stages");
        }
    }
}
