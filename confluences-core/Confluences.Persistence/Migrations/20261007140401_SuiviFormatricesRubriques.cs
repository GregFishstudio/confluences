using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class SuiviFormatricesRubriques : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ActiviteProfOrigine",
                table: "AspNetUsers",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BesoinsSouhaitsEmploi",
                table: "AspNetUsers",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BesoinsSouhaitsEmploiCommentaire",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BesoinsSouhaitsFormation",
                table: "AspNetUsers",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BesoinsSouhaitsFormationCommentaire",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateObtentionPermis",
                table: "AspNetUsers",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitresObtenus",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiviteProfOrigine",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BesoinsSouhaitsEmploi",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BesoinsSouhaitsEmploiCommentaire",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BesoinsSouhaitsFormation",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BesoinsSouhaitsFormationCommentaire",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DateObtentionPermis",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TitresObtenus",
                table: "AspNetUsers");
        }
    }
}
