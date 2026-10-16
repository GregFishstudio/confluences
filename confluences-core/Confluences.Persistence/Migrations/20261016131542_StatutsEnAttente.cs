using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    // Statuts de liste d'attente des candidatures : "Sur liste d'attente A1/A2" devient
    // "En attente A1/A2 F&I" ou "En attente A1/A2 Télécours" selon la mesure visée.
    // Un statut "Sur liste d'attente" sans niveau est laissé tel quel (niveau inconnu).
    public partial class StatutsEnAttente : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE ""Candidatures""
SET ""Statut"" = 'En attente ' || RIGHT(TRIM(""Statut""), 2) || CASE WHEN ""MesureVisee"" = 'Télécours' THEN ' Télécours' ELSE ' F&I' END
WHERE TRIM(""Statut"") IN ('Sur liste d''attente A1', 'Sur liste d''attente A2');

UPDATE ""Candidatures""
SET ""MesureVisee"" = 'F&I'
WHERE ""Statut"" IN ('En attente A1 F&I', 'En attente A2 F&I') AND ""MesureVisee"" IS NULL;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE ""Candidatures""
SET ""Statut"" = 'Sur liste d''attente ' || SUBSTRING(""Statut"" FROM 'A[12]')
WHERE ""Statut"" LIKE 'En attente A_ %';");
        }
    }
}
