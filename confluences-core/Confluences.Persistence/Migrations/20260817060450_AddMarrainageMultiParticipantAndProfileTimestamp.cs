using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class AddMarrainageMultiParticipantAndProfileTimestamp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RaisonInactivite",
                table: "Marrainages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProfilUpdatedAt",
                table: "AspNetUsers",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MarrainageParticipants",
                columns: table => new
                {
                    MarrainageId = table.Column<int>(type: "integer", nullable: false),
                    ParticipantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarrainageParticipants", x => new { x.MarrainageId, x.ParticipantId });
                    table.ForeignKey(
                        name: "FK_MarrainageParticipants_AspNetUsers_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MarrainageParticipants_Marrainages_MarrainageId",
                        column: x => x.MarrainageId,
                        principalTable: "Marrainages",
                        principalColumn: "MarrainageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarrainageParticipants_ParticipantId",
                table: "MarrainageParticipants",
                column: "ParticipantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarrainageParticipants");

            migrationBuilder.DropColumn(
                name: "RaisonInactivite",
                table: "Marrainages");

            migrationBuilder.DropColumn(
                name: "ProfilUpdatedAt",
                table: "AspNetUsers");
        }
    }
}
