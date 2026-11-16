using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    // Tableau de marrainage : table des régions, région + priorité de marrainage sur la fiche
    // participant·e, sexe des marraines/parrains, commentaire de suivi par relation.
    public partial class AddRegionsMarrainage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Sexe",
                table: "Marrainages",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Commentaire",
                table: "MarrainageParticipants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrioriteMarrainage",
                table: "AspNetUsers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "AspNetUsers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    RegionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nom = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Couleur = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    Ordre = table.Column<int>(type: "integer", nullable: false),
                    PistesRecherche = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.RegionId);
                });

            // Régions jusqu'ici figées dans le front (utils/regions.js), mêmes noms que ceux déjà
            // saisis sur les fiches marraines (Marrainages.Region).
            migrationBuilder.Sql(@"
INSERT INTO ""Regions"" (""Nom"", ""Couleur"", ""Ordre"") VALUES
('Lausanne', '#1565C0', 1),
('Moudon', '#6D4C41', 2),
('Riviera', '#00838F', 3),
('Morges', '#2E7D32', 4),
('Yverdon', '#EF6C00', 5),
('Aigle/Leysin', '#6A1B9A', 6),
('Nyon', '#AD1457', 7),
('Vallorbe', '#455A64', 8),
('Autres', '#9E9E9E', 9);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropColumn(
                name: "Sexe",
                table: "Marrainages");

            migrationBuilder.DropColumn(
                name: "Commentaire",
                table: "MarrainageParticipants");

            migrationBuilder.DropColumn(
                name: "PrioriteMarrainage",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "AspNetUsers");
        }
    }
}
