using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuiz_.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuizId = table.Column<int>(type: "int", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Option1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Option2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Option3 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Option4 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CorrectAnswer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2025, 4, 19, 8, 43, 20, 17, DateTimeKind.Utc).AddTicks(1103), "$2a$11$jMhN6Sf2YEihrHfk3aF0KOIgDV9EwEIw5YxtbpouhntVoGOGLwljK" });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_QuizId",
                table: "Questions",
                column: "QuizId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2025, 4, 19, 8, 14, 52, 916, DateTimeKind.Utc).AddTicks(2026), "$2a$11$pc7X3Dn1IbEhRO4Z3NWQcOe4Qwi9ckvFOf2ADRq0Z138Py5E/Kw32" });
        }
    }
}
