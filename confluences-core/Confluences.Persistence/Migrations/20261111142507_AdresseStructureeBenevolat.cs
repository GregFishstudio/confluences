using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    // Adresse structurée (rue + n°, NPA, localité) et intérêts pour le bénévolat (domaines, types
    // d'activité). Les adresses existantes de la forme « …, 1004 Lausanne » / « … 1004 Lausanne »
    // sont découpées automatiquement ; les autres restent dans Adresse, à compléter à la main.
    public partial class AdresseStructureeBenevolat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BenevolatDomaineAutre",
                table: "AspNetUsers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "BenevolatDomaines",
                table: "AspNetUsers",
                type: "text[]",
                nullable: false,
                defaultValueSql: "'{}'");

            migrationBuilder.AddColumn<List<string>>(
                name: "BenevolatTypesActivite",
                table: "AspNetUsers",
                type: "text[]",
                nullable: false,
                defaultValueSql: "'{}'");

            migrationBuilder.AddColumn<string>(
                name: "Localite",
                table: "AspNetUsers",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Npa",
                table: "AspNetUsers",
                type: "character varying(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rue",
                table: "AspNetUsers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE ""AspNetUsers"" u
SET ""Rue"" = NULLIF(btrim(left(s.m[1], 200)), ''),
    ""Npa"" = s.m[2],
    ""Localite"" = left(btrim(s.m[3]), 60)
FROM (
    SELECT ""Id"", regexp_match(btrim(""Adresse""), '^(?:(.*[^[:space:],])[[:space:],]+)?([0-9]{4})[[:space:]]+([^0-9,]+)$') AS m
    FROM ""AspNetUsers""
    WHERE ""Adresse"" IS NOT NULL AND btrim(""Adresse"") <> ''
) s
WHERE u.""Id"" = s.""Id"" AND s.m IS NOT NULL AND u.""Npa"" IS NULL;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BenevolatDomaineAutre",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BenevolatDomaines",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BenevolatTypesActivite",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Localite",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Npa",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Rue",
                table: "AspNetUsers");
        }
    }
}
