using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class Add3AudiosForTeacher : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AudioLinkSecond",
                table: "Theories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AudioLinkThird",
                table: "Theories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AudioLinkSecond",
                table: "ExercicesAlone",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AudioLinkThird",
                table: "ExercicesAlone",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AudioLinkSecond",
                table: "Exercices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AudioLinkThird",
                table: "Exercices",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioLinkSecond",
                table: "Theories");

            migrationBuilder.DropColumn(
                name: "AudioLinkThird",
                table: "Theories");

            migrationBuilder.DropColumn(
                name: "AudioLinkSecond",
                table: "ExercicesAlone");

            migrationBuilder.DropColumn(
                name: "AudioLinkThird",
                table: "ExercicesAlone");

            migrationBuilder.DropColumn(
                name: "AudioLinkSecond",
                table: "Exercices");

            migrationBuilder.DropColumn(
                name: "AudioLinkThird",
                table: "Exercices");
        }
    }
}
