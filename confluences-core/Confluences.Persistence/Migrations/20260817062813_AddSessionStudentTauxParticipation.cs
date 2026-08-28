using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class AddSessionStudentTauxParticipation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "TauxParticipation",
                table: "SessionStudents",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TauxParticipationCalculeLe",
                table: "SessionStudents",
                type: "timestamp without time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TauxParticipation",
                table: "SessionStudents");

            migrationBuilder.DropColumn(
                name: "TauxParticipationCalculeLe",
                table: "SessionStudents");
        }
    }
}
