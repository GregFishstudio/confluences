using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class AddSessionReportsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessionReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StagiaireId = table.Column<string>(type: "text", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Quarter = table.Column<string>(type: "text", nullable: false),
                    EvaluationText = table.Column<string>(type: "text", nullable: false),
                    FollowUpActions = table.Column<string>(type: "text", nullable: false),
                    GlobalRate = table.Column<double>(type: "double precision", nullable: false),
                    WorkshopsJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionReports_AspNetUsers_StagiaireId",
                        column: x => x.StagiaireId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionReports_StagiaireId",
                table: "SessionReports",
                column: "StagiaireId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionReports");
        }
    }
}
