using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class OptionalFieldsOnContact : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_Entreprises_EntrepriseId",
                table: "Contacts");

            migrationBuilder.AlterColumn<string>(
                name: "Nom",
                table: "Contacts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "EntrepriseId",
                table: "Contacts",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_Entreprises_EntrepriseId",
                table: "Contacts",
                column: "EntrepriseId",
                principalTable: "Entreprises",
                principalColumn: "EntrepriseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_Entreprises_EntrepriseId",
                table: "Contacts");

            migrationBuilder.AlterColumn<string>(
                name: "Nom",
                table: "Contacts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EntrepriseId",
                table: "Contacts",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_Entreprises_EntrepriseId",
                table: "Contacts",
                column: "EntrepriseId",
                principalTable: "Entreprises",
                principalColumn: "EntrepriseId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
