using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class EraseOldHomeworks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HomeworkStudents");

            migrationBuilder.DropTable(
                name: "Homework");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Homework",
                columns: table => new
                {
                    HomeworkId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClassLink = table.Column<string>(type: "text", nullable: true),
                    ClassName = table.Column<string>(type: "text", nullable: true),
                    ExerciceLink = table.Column<string>(type: "text", nullable: true),
                    ExerciceName = table.Column<string>(type: "text", nullable: true),
                    HomeworkDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    HomeworkTypeId = table.Column<int>(type: "integer", nullable: false),
                    IsHomeworkAdditionnal = table.Column<bool>(type: "boolean", nullable: false),
                    SessionId = table.Column<int>(type: "integer", nullable: false),
                    TeacherId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Homework", x => x.HomeworkId);
                    table.ForeignKey(
                        name: "FK_Homework_HomeworkTypes_HomeworkTypeId",
                        column: x => x.HomeworkTypeId,
                        principalTable: "HomeworkTypes",
                        principalColumn: "HomeworkTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Homework_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Homework_AspNetUsers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HomeworkStudents",
                columns: table => new
                {
                    HomeworkStudentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HomeworkFile = table.Column<string>(type: "text", nullable: false),
                    HomeworkFileTeacher = table.Column<string>(type: "text", nullable: true),
                    HomeworkId = table.Column<int>(type: "integer", nullable: false),
                    HomeworkStudentDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    StudentId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeworkStudents", x => x.HomeworkStudentId);
                    table.ForeignKey(
                        name: "FK_HomeworkStudents_Homework_HomeworkId",
                        column: x => x.HomeworkId,
                        principalTable: "Homework",
                        principalColumn: "HomeworkId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HomeworkStudents_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Homework_HomeworkTypeId",
                table: "Homework",
                column: "HomeworkTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Homework_SessionId",
                table: "Homework",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Homework_TeacherId",
                table: "Homework",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkStudents_HomeworkId",
                table: "HomeworkStudents",
                column: "HomeworkId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkStudents_StudentId",
                table: "HomeworkStudents",
                column: "StudentId");
        }
    }
}
