using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class AddedJobSearchAssistance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TypeJobSearchAssistanceOccurrences",
                columns: table => new
                {
                    TypeJobSearchAssistanceOccurrenceId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypeJobSearchAssistanceOccurrences", x => x.TypeJobSearchAssistanceOccurrenceId);
                });

            migrationBuilder.CreateTable(
                name: "TypeJobSearchAssistances",
                columns: table => new
                {
                    TypeJobSearchAssistanceId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypeJobSearchAssistances", x => x.TypeJobSearchAssistanceId);
                });

            migrationBuilder.CreateTable(
                name: "JobSearchAssistances",
                columns: table => new
                {
                    JobSearchAssistanceId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Town = table.Column<string>(type: "text", nullable: true),
                    ZipCode = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Website = table.Column<string>(type: "text", nullable: true),
                    KeyWords = table.Column<string>(type: "text", nullable: true),
                    TypeJobSearchAssistanceId = table.Column<int>(type: "integer", nullable: true),
                    TypeJobSearchAssistanceOccurrenceId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSearchAssistances", x => x.JobSearchAssistanceId);
                    table.ForeignKey(
                        name: "FK_JobSearchAssistances_TypeJobSearchAssistanceOccurrences_Typ~",
                        column: x => x.TypeJobSearchAssistanceOccurrenceId,
                        principalTable: "TypeJobSearchAssistanceOccurrences",
                        principalColumn: "TypeJobSearchAssistanceOccurrenceId");
                    table.ForeignKey(
                        name: "FK_JobSearchAssistances_TypeJobSearchAssistances_TypeJobSearch~",
                        column: x => x.TypeJobSearchAssistanceId,
                        principalTable: "TypeJobSearchAssistances",
                        principalColumn: "TypeJobSearchAssistanceId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobSearchAssistances_TypeJobSearchAssistanceId",
                table: "JobSearchAssistances",
                column: "TypeJobSearchAssistanceId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSearchAssistances_TypeJobSearchAssistanceOccurrenceId",
                table: "JobSearchAssistances",
                column: "TypeJobSearchAssistanceOccurrenceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobSearchAssistances");

            migrationBuilder.DropTable(
                name: "TypeJobSearchAssistanceOccurrences");

            migrationBuilder.DropTable(
                name: "TypeJobSearchAssistances");
        }
    }
}
