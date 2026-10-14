using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class AjustementsOctobre : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Marrainages",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "QuestionnaireSaisiLe",
                table: "Marrainages",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuestionnaireSaisiPar",
                table: "Marrainages",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "Marrainages",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Marrainages",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateEvaluation",
                table: "Candidatures",
                type: "timestamp without time zone",
                nullable: true);

            // Oui/Non → Oui/Non/À vérifier : "oui" reste "oui", tout le reste (non ou vide) devient "non".
            migrationBuilder.Sql(@"ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""RechercheLogement"" TYPE text USING CASE WHEN ""RechercheLogement"" THEN 'oui' ELSE 'non' END;");

            // Oui/Non → Oui/Non/À vérifier : "oui" reste "oui", tout le reste (non ou vide) devient "non".
            migrationBuilder.Sql(@"ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""BesoinsSouhaitsStages"" TYPE text USING CASE WHEN ""BesoinsSouhaitsStages"" THEN 'oui' ELSE 'non' END;");

            // Oui/Non → Oui/Non/À vérifier : "oui" reste "oui", tout le reste (non ou vide) devient "non".
            migrationBuilder.Sql(@"ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""BesoinsSouhaitsMarrainage"" TYPE text USING CASE WHEN ""BesoinsSouhaitsMarrainage"" THEN 'oui' ELSE 'non' END;");

            // Oui/Non → Oui/Non/À vérifier : "oui" reste "oui", tout le reste (non ou vide) devient "non".
            migrationBuilder.Sql(@"ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""BesoinsSouhaitsFormation"" TYPE text USING CASE WHEN ""BesoinsSouhaitsFormation"" THEN 'oui' ELSE 'non' END;");

            // Oui/Non → Oui/Non/À vérifier : "oui" reste "oui", tout le reste (non ou vide) devient "non".
            migrationBuilder.Sql(@"ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""BesoinsSouhaitsEmploi"" TYPE text USING CASE WHEN ""BesoinsSouhaitsEmploi"" THEN 'oui' ELSE 'non' END;");

            // Oui/Non → Oui/Non/À vérifier : "oui" reste "oui", tout le reste (non ou vide) devient "non".
            migrationBuilder.Sql(@"ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""BesoinsSouhaitsBenevolat"" TYPE text USING CASE WHEN ""BesoinsSouhaitsBenevolat"" THEN 'oui' ELSE 'non' END;");

            migrationBuilder.AddColumn<int>(
                name: "AnneeObtentionPermis",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BesoinsSouhaitsSport",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Disponibilites",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProjetProfessionnel",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SouhaitAgeMarrainage",
                table: "AspNetUsers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SouhaitGenreMarrainage",
                table: "AspNetUsers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ParticipantComments",
                columns: table => new
                {
                    ParticipantCommentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<string>(type: "text", nullable: false),
                    Rubrique = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AuthorId = table.Column<string>(type: "text", nullable: true),
                    AuthorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantComments", x => x.ParticipantCommentId);
                    table.ForeignKey(
                        name: "FK_ParticipantComments_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantComments_StudentId",
                table: "ParticipantComments",
                column: "StudentId");
        

            // Permis : l'année est reprise de la date déjà saisie.
            migrationBuilder.Sql(@"UPDATE ""AspNetUsers"" SET ""AnneeObtentionPermis"" = EXTRACT(YEAR FROM ""DateObtentionPermis"")::int WHERE ""DateObtentionPermis"" IS NOT NULL;");

            // Marrainages existants : le suivi ("Nouveau" pendant un mois, "Prendre des nouvelles"
            // après trois mois) démarre à la date de mise en ligne.
            migrationBuilder.Sql(@"UPDATE ""Marrainages"" SET ""CreatedAt"" = now(), ""UpdatedAt"" = now();");

            // Les anciens champs "commentaire" du suivi formatrices deviennent la première entrée
            // datée de leur rubrique (date et nom = dernière mise à jour de la fiche). Les colonnes
            // d'origine sont conservées.
            migrationBuilder.Sql(@"
INSERT INTO ""ParticipantComments"" (""StudentId"", ""Rubrique"", ""Text"", ""CreatedAt"", ""AuthorName"")
SELECT ""Id"", 'parcours_suisse', ""ParcoursDepuisArriveeSuisse"", COALESCE(""ProfilUpdatedAt"", now()), ""ProfilUpdatedByName"" FROM ""AspNetUsers"" WHERE COALESCE(TRIM(""ParcoursDepuisArriveeSuisse""), '') <> '';
INSERT INTO ""ParticipantComments"" (""StudentId"", ""Rubrique"", ""Text"", ""CreatedAt"", ""AuthorName"")
SELECT ""Id"", 'formation_origine', ""PlusHautesEtudesCommentaire"", COALESCE(""ProfilUpdatedAt"", now()), ""ProfilUpdatedByName"" FROM ""AspNetUsers"" WHERE COALESCE(TRIM(""PlusHautesEtudesCommentaire""), '') <> '';
INSERT INTO ""ParticipantComments"" (""StudentId"", ""Rubrique"", ""Text"", ""CreatedAt"", ""AuthorName"")
SELECT ""Id"", 'activite_origine', ""ParcoursProfessionnel"", COALESCE(""ProfilUpdatedAt"", now()), ""ProfilUpdatedByName"" FROM ""AspNetUsers"" WHERE COALESCE(TRIM(""ParcoursProfessionnel""), '') <> '';
INSERT INTO ""ParticipantComments"" (""StudentId"", ""Rubrique"", ""Text"", ""CreatedAt"", ""AuthorName"")
SELECT ""Id"", 'activite_suisse', ""ActiviteProfSuisseCommentaire"", COALESCE(""ProfilUpdatedAt"", now()), ""ProfilUpdatedByName"" FROM ""AspNetUsers"" WHERE COALESCE(TRIM(""ActiviteProfSuisseCommentaire""), '') <> '';
INSERT INTO ""ParticipantComments"" (""StudentId"", ""Rubrique"", ""Text"", ""CreatedAt"", ""AuthorName"")
SELECT ""Id"", 'francais', ""BesoinsSouhaitsFrancais"", COALESCE(""ProfilUpdatedAt"", now()), ""ProfilUpdatedByName"" FROM ""AspNetUsers"" WHERE COALESCE(TRIM(""BesoinsSouhaitsFrancais""), '') <> '';
INSERT INTO ""ParticipantComments"" (""StudentId"", ""Rubrique"", ""Text"", ""CreatedAt"", ""AuthorName"")
SELECT ""Id"", 'logement', ""RechercheLogementCommentaire"", COALESCE(""ProfilUpdatedAt"", now()), ""ProfilUpdatedByName"" FROM ""AspNetUsers"" WHERE COALESCE(TRIM(""RechercheLogementCommentaire""), '') <> '';
INSERT INTO ""ParticipantComments"" (""StudentId"", ""Rubrique"", ""Text"", ""CreatedAt"", ""AuthorName"")
SELECT ""Id"", 'stages', ""BesoinsSouhaitsStagesCommentaire"", COALESCE(""ProfilUpdatedAt"", now()), ""ProfilUpdatedByName"" FROM ""AspNetUsers"" WHERE COALESCE(TRIM(""BesoinsSouhaitsStagesCommentaire""), '') <> '';
INSERT INTO ""ParticipantComments"" (""StudentId"", ""Rubrique"", ""Text"", ""CreatedAt"", ""AuthorName"")
SELECT ""Id"", 'benevolat', ""BesoinsSouhaitsBenevoletCommentaire"", COALESCE(""ProfilUpdatedAt"", now()), ""ProfilUpdatedByName"" FROM ""AspNetUsers"" WHERE COALESCE(TRIM(""BesoinsSouhaitsBenevoletCommentaire""), '') <> '';
INSERT INTO ""ParticipantComments"" (""StudentId"", ""Rubrique"", ""Text"", ""CreatedAt"", ""AuthorName"")
SELECT ""Id"", 'marrainage', ""BesoinsSouhaitsMarrainageCommentaire"", COALESCE(""ProfilUpdatedAt"", now()), ""ProfilUpdatedByName"" FROM ""AspNetUsers"" WHERE COALESCE(TRIM(""BesoinsSouhaitsMarrainageCommentaire""), '') <> '';
INSERT INTO ""ParticipantComments"" (""StudentId"", ""Rubrique"", ""Text"", ""CreatedAt"", ""AuthorName"")
SELECT ""Id"", 'emploi', ""BesoinsSouhaitsEmploiCommentaire"", COALESCE(""ProfilUpdatedAt"", now()), ""ProfilUpdatedByName"" FROM ""AspNetUsers"" WHERE COALESCE(TRIM(""BesoinsSouhaitsEmploiCommentaire""), '') <> '';
INSERT INTO ""ParticipantComments"" (""StudentId"", ""Rubrique"", ""Text"", ""CreatedAt"", ""AuthorName"")
SELECT ""Id"", 'formation', ""BesoinsSouhaitsFormationCommentaire"", COALESCE(""ProfilUpdatedAt"", now()), ""ProfilUpdatedByName"" FROM ""AspNetUsers"" WHERE COALESCE(TRIM(""BesoinsSouhaitsFormationCommentaire""), '') <> '';
INSERT INTO ""ParticipantComments"" (""StudentId"", ""Rubrique"", ""Text"", ""CreatedAt"", ""AuthorName"")
SELECT ""Id"", 'autre', ""Autre"", COALESCE(""ProfilUpdatedAt"", now()), ""ProfilUpdatedByName"" FROM ""AspNetUsers"" WHERE COALESCE(TRIM(""Autre""), '') <> '';");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParticipantComments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Marrainages");

            migrationBuilder.DropColumn(
                name: "QuestionnaireSaisiLe",
                table: "Marrainages");

            migrationBuilder.DropColumn(
                name: "QuestionnaireSaisiPar",
                table: "Marrainages");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "Marrainages");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Marrainages");

            migrationBuilder.DropColumn(
                name: "DateEvaluation",
                table: "Candidatures");

            migrationBuilder.DropColumn(
                name: "AnneeObtentionPermis",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BesoinsSouhaitsSport",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Disponibilites",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProjetProfessionnel",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SouhaitAgeMarrainage",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SouhaitGenreMarrainage",
                table: "AspNetUsers");

            migrationBuilder.Sql(@"ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""RechercheLogement"" TYPE boolean USING (""RechercheLogement"" = 'oui');");

            migrationBuilder.Sql(@"ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""BesoinsSouhaitsStages"" TYPE boolean USING (""BesoinsSouhaitsStages"" = 'oui');");

            migrationBuilder.Sql(@"ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""BesoinsSouhaitsMarrainage"" TYPE boolean USING (""BesoinsSouhaitsMarrainage"" = 'oui');");

            migrationBuilder.Sql(@"ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""BesoinsSouhaitsFormation"" TYPE boolean USING (""BesoinsSouhaitsFormation"" = 'oui');");

            migrationBuilder.Sql(@"ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""BesoinsSouhaitsEmploi"" TYPE boolean USING (""BesoinsSouhaitsEmploi"" = 'oui');");

            migrationBuilder.Sql(@"ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""BesoinsSouhaitsBenevolat"" TYPE boolean USING (""BesoinsSouhaitsBenevolat"" = 'oui');");
        }
    }
}
