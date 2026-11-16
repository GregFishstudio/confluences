using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    // Une candidature peut viser plusieurs sessions : liste "2027-1", "2027-2"… reprise de
    // l'ancienne session visée unique (AnneeVisee + SessionNumberVisee, conservés).
    public partial class CandidatureSessionsVisees : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "SessionsVisees",
                table: "Candidatures",
                type: "text[]",
                nullable: false,
                defaultValueSql: "'{}'");

            migrationBuilder.Sql(@"
UPDATE ""Candidatures""
SET ""SessionsVisees"" = ARRAY[""AnneeVisee""::text || '-' || ""SessionNumberVisee""::text]
WHERE ""AnneeVisee"" IS NOT NULL AND ""SessionNumberVisee"" IS NOT NULL;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionsVisees",
                table: "Candidatures");
        }
    }
}
