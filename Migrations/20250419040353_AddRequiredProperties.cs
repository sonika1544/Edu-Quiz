using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuiz_.Migrations
{
    /// <inheritdoc />
    public partial class AddRequiredProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2025, 4, 19, 4, 3, 52, 50, DateTimeKind.Utc).AddTicks(7234), "$2a$11$WPRL8zw1p0qQ8xCPn9ZWM.rH0loUyYthoD0C0T4j6OgouVJqNkDzS" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2025, 4, 19, 3, 59, 20, 653, DateTimeKind.Utc).AddTicks(1096), "$2a$11$UAN3KqoYFq4PEL5QI0Kb2e4LJIqgl33dbUAaKxVzFshXiWgqymu7e" });
        }
    }
}
