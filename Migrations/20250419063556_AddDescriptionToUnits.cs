using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuiz_.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionToUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Topics");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Units",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2025, 4, 19, 6, 35, 54, 622, DateTimeKind.Utc).AddTicks(3620), "$2a$11$gbw6GkhNsTJQKORipETzYuklu345lqZIdx58Surn6D6PHFiouGfQS" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Units");

            migrationBuilder.CreateTable(
                name: "Topics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Topics = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Topics_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2025, 4, 19, 5, 46, 46, 479, DateTimeKind.Utc).AddTicks(784), "$2a$11$fs3fDD33ACMK50Pjzj3nIeIfwQD2hX1ROCqNLA63U4htlPzqrJRCO" });

            migrationBuilder.CreateIndex(
                name: "IX_Topics_UnitId",
                table: "Topics",
                column: "UnitId");
        }
    }
}
