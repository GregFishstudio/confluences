using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class AddMarrainageInactivityReasons : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MarrainageInactivityReasons",
                columns: table => new
                {
                    MarrainageInactivityReasonId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarrainageInactivityReasons", x => x.MarrainageInactivityReasonId);
                });

            // Reprend les raisons jusqu'ici codées en dur côté frontend, pour ne rien perdre au
            // passage à une liste éditable en base.
            migrationBuilder.InsertData(
                table: "MarrainageInactivityReasons",
                column: "Label",
                values: new object[]
                {
                    "Contraintes familiales",
                    "En emploi",
                    "Déménagement",
                    "Fin de formation du/de la filleul·e",
                    "Manque de disponibilité",
                    "Autre",
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarrainageInactivityReasons");
        }
    }
}
