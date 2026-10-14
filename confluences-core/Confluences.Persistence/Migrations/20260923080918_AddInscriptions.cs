using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class AddInscriptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CommunicationAS",
                table: "SessionStudents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObjectifsAtteints",
                table: "SessionStudents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObjectifsCommentaire",
                table: "SessionStudents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrientationPrecisions",
                table: "SessionStudents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrientationPreconisee",
                table: "SessionStudents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatutBeneficiaire",
                table: "SessionStudents",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AssistantsSociaux",
                columns: table => new
                {
                    AssistantSocialId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Prenom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Nom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Telephone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Organisme = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Remarques = table.Column<string>(type: "text", nullable: true),
                    Actif = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssistantsSociaux", x => x.AssistantSocialId);
                });

            migrationBuilder.CreateTable(
                name: "Candidatures",
                columns: table => new
                {
                    CandidatureId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DateDemande = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Statut = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MesureVisee = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AnneeVisee = table.Column<int>(type: "integer", nullable: true),
                    SessionNumberVisee = table.Column<int>(type: "integer", nullable: true),
                    Priorite = table.Column<int>(type: "integer", nullable: true),
                    Civilite = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Prenom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Nom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DateNaissance = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Adresse = table.Column<string>(type: "text", nullable: true),
                    Telephone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Origine = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Mandant = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AssistantSocialId = table.Column<int>(type: "integer", nullable: true),
                    RemarquesAS = table.Column<string>(type: "text", nullable: true),
                    NotesRdv = table.Column<string>(type: "text", nullable: true),
                    EvaluationNiveau = table.Column<string>(type: "text", nullable: true),
                    RemarquesPedagogiques = table.Column<string>(type: "text", nullable: true),
                    RemarquesAdministration = table.Column<string>(type: "text", nullable: true),
                    InscriptionConfirmee = table.Column<bool>(type: "boolean", nullable: false),
                    TransmissionAS = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ParticipantId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidatures", x => x.CandidatureId);
                    table.ForeignKey(
                        name: "FK_Candidatures_AspNetUsers_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Candidatures_AssistantsSociaux_AssistantSocialId",
                        column: x => x.AssistantSocialId,
                        principalTable: "AssistantsSociaux",
                        principalColumn: "AssistantSocialId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssistantsSociaux_Email",
                table: "AssistantsSociaux",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Candidatures_AssistantSocialId",
                table: "Candidatures",
                column: "AssistantSocialId");

            migrationBuilder.CreateIndex(
                name: "IX_Candidatures_ParticipantId",
                table: "Candidatures",
                column: "ParticipantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Candidatures");

            migrationBuilder.DropTable(
                name: "AssistantsSociaux");

            migrationBuilder.DropColumn(
                name: "CommunicationAS",
                table: "SessionStudents");

            migrationBuilder.DropColumn(
                name: "ObjectifsAtteints",
                table: "SessionStudents");

            migrationBuilder.DropColumn(
                name: "ObjectifsCommentaire",
                table: "SessionStudents");

            migrationBuilder.DropColumn(
                name: "OrientationPrecisions",
                table: "SessionStudents");

            migrationBuilder.DropColumn(
                name: "OrientationPreconisee",
                table: "SessionStudents");

            migrationBuilder.DropColumn(
                name: "StatutBeneficiaire",
                table: "SessionStudents");
        }
    }
}
