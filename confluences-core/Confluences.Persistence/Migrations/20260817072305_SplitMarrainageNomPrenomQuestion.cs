using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class SplitMarrainageNomPrenomQuestion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // "Nom et prénom" devient "Prénom" ; une nouvelle question "Nom" est insérée juste après.
            // Idempotent : ne fait rien si la question a déjà été renommée (ex: rejouée deux fois).
            migrationBuilder.Sql(@"
                UPDATE ""MarrainageQuestions"" SET ""Label"" = 'Prénom' WHERE ""Label"" = 'Nom et prénom';

                UPDATE ""MarrainageQuestions""
                SET ""Order"" = ""Order"" + 1
                WHERE ""Order"" >= (SELECT ""Order"" + 1 FROM ""MarrainageQuestions"" WHERE ""Label"" = 'Prénom')
                  AND NOT EXISTS (SELECT 1 FROM ""MarrainageQuestions"" WHERE ""Label"" = 'Nom');

                INSERT INTO ""MarrainageQuestions"" (""Label"", ""Type"", ""Options"", ""Required"", ""Order"", ""Active"")
                SELECT 'Nom', 'text', NULL, true, ""Order"" + 1, true
                FROM ""MarrainageQuestions""
                WHERE ""Label"" = 'Prénom'
                  AND NOT EXISTS (SELECT 1 FROM ""MarrainageQuestions"" WHERE ""Label"" = 'Nom');
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM ""MarrainageQuestions"" WHERE ""Label"" = 'Nom';
                UPDATE ""MarrainageQuestions"" SET ""Label"" = 'Nom et prénom' WHERE ""Label"" = 'Prénom';
            ");
        }
    }
}
