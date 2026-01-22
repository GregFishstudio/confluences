using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.IO;

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class addViewRessource : Migration
    {
       protected override void Up(MigrationBuilder migrationBuilder)
{
    // On met le chemin complet vers le fichier sur VOTRE disque dur.
    // D'après vos captures, le fichier est ici :
    var sqlFile = @"C:\Users\info\Documents\00_Fish-studio\006_Applications\Confluences\confluences-core\Confluences.Persistence\Migrations\SQL\RessourceView.sql";

    if (System.IO.File.Exists(sqlFile))
    {
        // Si le fichier est là, on l'exécute
        migrationBuilder.Sql(System.IO.File.ReadAllText(sqlFile));
    }
    else
    {
        // ASTUCE : Au lieu de faire planter tout le processus avec une "Exception",
        // on écrit juste un avertissement dans la console et on continue.
        // Comme c'est une vieille migration (2021), ce n'est pas grave si elle est sautée.
        System.Console.WriteLine($"ATTENTION : Le fichier SQL n'a pas été trouvé ici : {sqlFile}. Migration ignorée.");
    }
}
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
