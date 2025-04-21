using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuiz_.Migrations
{
    /// <inheritdoc />
    public partial class AddDurationMinutesToQuiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "Quizzes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2025, 4, 19, 9, 52, 58, 748, DateTimeKind.Utc).AddTicks(1523), "$2a$11$JSC/uVNO68d8WF85FWfW9ezSdkmnW5vEsKhncycEgzfCaIfxyfYGm" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "Quizzes");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2025, 4, 19, 8, 43, 20, 17, DateTimeKind.Utc).AddTicks(1103), "$2a$11$jMhN6Sf2YEihrHfk3aF0KOIgDV9EwEIw5YxtbpouhntVoGOGLwljK" });
        }
    }
}
