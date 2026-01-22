using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class AddBilanSessionColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "SessionReports",
                newName: "SessionReportId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "SessionReports",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DigitalSkillsText",
                table: "SessionReports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomeVisitsText",
                table: "SessionReports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IndividualAccompanyingText",
                table: "SessionReports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SocialInsertionText",
                table: "SessionReports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "SessionReports",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "SessionReports");

            migrationBuilder.DropColumn(
                name: "DigitalSkillsText",
                table: "SessionReports");

            migrationBuilder.DropColumn(
                name: "HomeVisitsText",
                table: "SessionReports");

            migrationBuilder.DropColumn(
                name: "IndividualAccompanyingText",
                table: "SessionReports");

            migrationBuilder.DropColumn(
                name: "SocialInsertionText",
                table: "SessionReports");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "SessionReports");

            migrationBuilder.RenameColumn(
                name: "SessionReportId",
                table: "SessionReports",
                newName: "Id");
        }
    }
}
