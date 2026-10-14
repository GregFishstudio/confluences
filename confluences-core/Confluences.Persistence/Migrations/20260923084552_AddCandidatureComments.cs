using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class AddCandidatureComments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RemarquesAdministration",
                table: "Candidatures",
                newName: "RemarquesGenerales");

            migrationBuilder.CreateTable(
                name: "CandidatureComments",
                columns: table => new
                {
                    CandidatureCommentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CandidatureId = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AuthorId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidatureComments", x => x.CandidatureCommentId);
                    table.ForeignKey(
                        name: "FK_CandidatureComments_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CandidatureComments_Candidatures_CandidatureId",
                        column: x => x.CandidatureId,
                        principalTable: "Candidatures",
                        principalColumn: "CandidatureId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidatureComments_AuthorId",
                table: "CandidatureComments",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidatureComments_CandidatureId",
                table: "CandidatureComments",
                column: "CandidatureId");

            // Reprend les anciennes "Remarques AS" (texte unique) comme première remarque du fil,
            // avant de supprimer la colonne.
            migrationBuilder.Sql(@"
                INSERT INTO ""CandidatureComments"" (""CandidatureId"", ""Text"", ""CreatedAt"", ""AuthorId"")
                SELECT ""CandidatureId"", ""RemarquesAS"", ""UpdatedAt"", NULL
                FROM ""Candidatures""
                WHERE ""RemarquesAS"" IS NOT NULL AND btrim(""RemarquesAS"") <> '';");

            migrationBuilder.DropColumn(
                name: "RemarquesAS",
                table: "Candidatures");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidatureComments");

            migrationBuilder.RenameColumn(
                name: "RemarquesGenerales",
                table: "Candidatures",
                newName: "RemarquesAdministration");

            migrationBuilder.AddColumn<string>(
                name: "RemarquesAS",
                table: "Candidatures",
                type: "text",
                nullable: true);
        }
    }
}
