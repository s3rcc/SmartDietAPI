using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessObjects.Migrations
{
    /// <inheritdoc />
    public partial class fix1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AverageRating",
                table: "Meals",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "Difficulty",
                table: "Dishes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "Meals");

            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "Dishes");
        }
    }
}
