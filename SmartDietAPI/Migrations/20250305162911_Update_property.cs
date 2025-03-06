using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartDietAPI.Migrations
{
    /// <inheritdoc />
    public partial class Update_property : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Subcriptions");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Subcriptions");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "UserPayments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MonthOfSubcription",
                table: "Subcriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                table: "UserPayments");

            migrationBuilder.DropColumn(
                name: "MonthOfSubcription",
                table: "Subcriptions");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Subcriptions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Subcriptions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
