using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class AddFileAttributesSoStudentCanGiveAllTypesOfHomework : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HomeworkFileName",
                table: "HomeworkV2Students",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomeworkFileSize",
                table: "HomeworkV2Students",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomeworkFileType",
                table: "HomeworkV2Students",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomeworkFileName",
                table: "HomeworkV2StudentExerciceAlones",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomeworkFileSize",
                table: "HomeworkV2StudentExerciceAlones",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomeworkFileType",
                table: "HomeworkV2StudentExerciceAlones",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HomeworkFileName",
                table: "HomeworkV2Students");

            migrationBuilder.DropColumn(
                name: "HomeworkFileSize",
                table: "HomeworkV2Students");

            migrationBuilder.DropColumn(
                name: "HomeworkFileType",
                table: "HomeworkV2Students");

            migrationBuilder.DropColumn(
                name: "HomeworkFileName",
                table: "HomeworkV2StudentExerciceAlones");

            migrationBuilder.DropColumn(
                name: "HomeworkFileSize",
                table: "HomeworkV2StudentExerciceAlones");

            migrationBuilder.DropColumn(
                name: "HomeworkFileType",
                table: "HomeworkV2StudentExerciceAlones");
        }
    }
}
