using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class AddMarrainageRelationDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateNaissance",
                table: "Marrainages",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Statut",
                table: "Marrainages",
                type: "text",
                nullable: true);

            // Les relations déjà existantes (créées avant l'ajout de ce statut) sont considérées
            // actives par défaut, cohérent avec le comportement précédent où le simple fait
            // d'être lié·e valait "actif".
            migrationBuilder.AddColumn<bool>(
                name: "Actif",
                table: "MarrainageParticipants",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateDebut",
                table: "MarrainageParticipants",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateFin",
                table: "MarrainageParticipants",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RaisonInactivite",
                table: "MarrainageParticipants",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateNaissance",
                table: "Marrainages");

            migrationBuilder.DropColumn(
                name: "Statut",
                table: "Marrainages");

            migrationBuilder.DropColumn(
                name: "Actif",
                table: "MarrainageParticipants");

            migrationBuilder.DropColumn(
                name: "DateDebut",
                table: "MarrainageParticipants");

            migrationBuilder.DropColumn(
                name: "DateFin",
                table: "MarrainageParticipants");

            migrationBuilder.DropColumn(
                name: "RaisonInactivite",
                table: "MarrainageParticipants");
        }
    }
}
