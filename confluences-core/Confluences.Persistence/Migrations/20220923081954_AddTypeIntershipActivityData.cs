using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class AddTypeIntershipActivityData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO public.\"TypeIntershipActivities\"(\"Nom\") VALUES('Stage Découverte')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
