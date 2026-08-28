using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class AddParticipantAndMarrainage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ActiviteProfSuisse",
                table: "AspNetUsers",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActiviteProfSuisseCommentaire",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Adresse",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AnneeArriveeSuisse",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssistantSocialEmail",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Autre",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AutresLangues",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BeneficiaireDe",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BesoinsSouhaitsBenevolat",
                table: "AspNetUsers",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BesoinsSouhaitsBenevoletCommentaire",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BesoinsSouhaitsFrancais",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BesoinsSouhaitsStages",
                table: "AspNetUsers",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BesoinsSouhaitsStagesCommentaire",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ContratMaori",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DomaineEtudeTitreObtenu",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EtatCivil",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NiveauDebutContrat",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NiveauFinContrat",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NombreEnfants",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParcoursDepuisArriveeSuisse",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParcoursFormationSuisse",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParcoursProfessionnel",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlusHautesEtudes",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlusHautesEtudesCommentaire",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RechercheLogement",
                table: "AspNetUsers",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RechercheLogementCommentaire",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Marrainages",
                columns: table => new
                {
                    MarrainageId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nom = table.Column<string>(type: "text", nullable: false),
                    Prenom = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Numero = table.Column<string>(type: "text", nullable: true),
                    Commune = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Actif = table.Column<bool>(type: "boolean", nullable: false),
                    ParticipantId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marrainages", x => x.MarrainageId);
                    table.ForeignKey(
                        name: "FK_Marrainages_AspNetUsers_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ParticipantFiles",
                columns: table => new
                {
                    ParticipantFileId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    FileType = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: true),
                    Data = table.Column<byte[]>(type: "bytea", nullable: false),
                    StudentId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantFiles", x => x.ParticipantFileId);
                    table.ForeignKey(
                        name: "FK_ParticipantFiles_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MarrainageComments",
                columns: table => new
                {
                    MarrainageCommentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MarrainageId = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AuthorId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarrainageComments", x => x.MarrainageCommentId);
                    table.ForeignKey(
                        name: "FK_MarrainageComments_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MarrainageComments_Marrainages_MarrainageId",
                        column: x => x.MarrainageId,
                        principalTable: "Marrainages",
                        principalColumn: "MarrainageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MarrainageFiles",
                columns: table => new
                {
                    MarrainageFileId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    FileType = table.Column<string>(type: "text", nullable: false),
                    Data = table.Column<byte[]>(type: "bytea", nullable: false),
                    MarrainageId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarrainageFiles", x => x.MarrainageFileId);
                    table.ForeignKey(
                        name: "FK_MarrainageFiles_Marrainages_MarrainageId",
                        column: x => x.MarrainageId,
                        principalTable: "Marrainages",
                        principalColumn: "MarrainageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarrainageComments_AuthorId",
                table: "MarrainageComments",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_MarrainageComments_MarrainageId",
                table: "MarrainageComments",
                column: "MarrainageId");

            migrationBuilder.CreateIndex(
                name: "IX_MarrainageFiles_MarrainageId",
                table: "MarrainageFiles",
                column: "MarrainageId");

            migrationBuilder.CreateIndex(
                name: "IX_Marrainages_ParticipantId",
                table: "Marrainages",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantFiles_StudentId",
                table: "ParticipantFiles",
                column: "StudentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarrainageComments");

            migrationBuilder.DropTable(
                name: "MarrainageFiles");

            migrationBuilder.DropTable(
                name: "ParticipantFiles");

            migrationBuilder.DropTable(
                name: "Marrainages");

            migrationBuilder.DropColumn(
                name: "ActiviteProfSuisse",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ActiviteProfSuisseCommentaire",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Adresse",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AnneeArriveeSuisse",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AssistantSocialEmail",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Autre",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AutresLangues",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BeneficiaireDe",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BesoinsSouhaitsBenevolat",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BesoinsSouhaitsBenevoletCommentaire",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BesoinsSouhaitsFrancais",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BesoinsSouhaitsStages",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BesoinsSouhaitsStagesCommentaire",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ContratMaori",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DomaineEtudeTitreObtenu",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EtatCivil",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NiveauDebutContrat",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NiveauFinContrat",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NombreEnfants",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ParcoursDepuisArriveeSuisse",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ParcoursFormationSuisse",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ParcoursProfessionnel",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PlusHautesEtudes",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PlusHautesEtudesCommentaire",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RechercheLogement",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RechercheLogementCommentaire",
                table: "AspNetUsers");
        }
    }
}
