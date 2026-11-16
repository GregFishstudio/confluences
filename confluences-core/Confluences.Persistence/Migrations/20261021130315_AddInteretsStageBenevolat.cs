using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    // Tableau stages & bénévolat : métiers/domaines d'intérêt et degré d'intérêt des participant·e·s.
    public partial class AddInteretsStageBenevolat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DegreInteretStage",
                table: "AspNetUsers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ParticipantDomaines",
                columns: table => new
                {
                    ParticipantId = table.Column<string>(type: "text", nullable: false),
                    TypeDomaineId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantDomaines", x => new { x.ParticipantId, x.TypeDomaineId });
                    table.ForeignKey(
                        name: "FK_ParticipantDomaines_AspNetUsers_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParticipantDomaines_TypeDomaines_TypeDomaineId",
                        column: x => x.TypeDomaineId,
                        principalTable: "TypeDomaines",
                        principalColumn: "TypeDomaineId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParticipantMetiers",
                columns: table => new
                {
                    ParticipantId = table.Column<string>(type: "text", nullable: false),
                    TypeMetierId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantMetiers", x => new { x.ParticipantId, x.TypeMetierId });
                    table.ForeignKey(
                        name: "FK_ParticipantMetiers_AspNetUsers_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParticipantMetiers_TypeMetiers_TypeMetierId",
                        column: x => x.TypeMetierId,
                        principalTable: "TypeMetiers",
                        principalColumn: "TypeMetierId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantDomaines_TypeDomaineId",
                table: "ParticipantDomaines",
                column: "TypeDomaineId");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantMetiers_TypeMetierId",
                table: "ParticipantMetiers",
                column: "TypeMetierId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParticipantDomaines");

            migrationBuilder.DropTable(
                name: "ParticipantMetiers");

            migrationBuilder.DropColumn(
                name: "DegreInteretStage",
                table: "AspNetUsers");
        }
    }
}
