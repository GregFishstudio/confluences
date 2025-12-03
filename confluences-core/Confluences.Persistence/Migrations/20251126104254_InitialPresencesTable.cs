using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class InitialPresencesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Presences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StagiaireId = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Statut = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    StageId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Presences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Presences_AspNetUsers_StagiaireId",
                        column: x => x.StagiaireId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Presences_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "Stages",
                        principalColumn: "StageId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Presences_StageId",
                table: "Presences",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_Presences_StagiaireId_Date",
                table: "Presences",
                columns: new[] { "StagiaireId", "Date" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Presences");
        }
    }
}
