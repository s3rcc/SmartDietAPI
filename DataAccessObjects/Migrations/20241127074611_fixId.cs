using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessObjects.Migrations
{
    /// <inheritdoc />
    public partial class fixId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DishIngredients_Dishes_DishId1",
                table: "DishIngredients");

            migrationBuilder.DropForeignKey(
                name: "FK_DishIngredients_Foods_FoodId1",
                table: "DishIngredients");

            migrationBuilder.DropForeignKey(
                name: "FK_Fridges_AspNetUsers_SmartDietUserId1",
                table: "Fridges");

            migrationBuilder.DropIndex(
                name: "IX_Fridges_SmartDietUserId1",
                table: "Fridges");

            migrationBuilder.DropIndex(
                name: "IX_DishIngredients_DishId1",
                table: "DishIngredients");

            migrationBuilder.DropIndex(
                name: "IX_DishIngredients_FoodId1",
                table: "DishIngredients");

            migrationBuilder.DropColumn(
                name: "SmartDietUserId1",
                table: "Fridges");

            migrationBuilder.DropColumn(
                name: "DishId1",
                table: "DishIngredients");

            migrationBuilder.DropColumn(
                name: "FoodId1",
                table: "DishIngredients");

            migrationBuilder.AlterColumn<string>(
                name: "SmartDietUserId",
                table: "Fridges",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "FoodId",
                table: "DishIngredients",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "DishId",
                table: "DishIngredients",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Fridges_SmartDietUserId",
                table: "Fridges",
                column: "SmartDietUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DishIngredients_DishId",
                table: "DishIngredients",
                column: "DishId");

            migrationBuilder.CreateIndex(
                name: "IX_DishIngredients_FoodId",
                table: "DishIngredients",
                column: "FoodId");

            migrationBuilder.AddForeignKey(
                name: "FK_DishIngredients_Dishes_DishId",
                table: "DishIngredients",
                column: "DishId",
                principalTable: "Dishes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DishIngredients_Foods_FoodId",
                table: "DishIngredients",
                column: "FoodId",
                principalTable: "Foods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Fridges_AspNetUsers_SmartDietUserId",
                table: "Fridges",
                column: "SmartDietUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DishIngredients_Dishes_DishId",
                table: "DishIngredients");

            migrationBuilder.DropForeignKey(
                name: "FK_DishIngredients_Foods_FoodId",
                table: "DishIngredients");

            migrationBuilder.DropForeignKey(
                name: "FK_Fridges_AspNetUsers_SmartDietUserId",
                table: "Fridges");

            migrationBuilder.DropIndex(
                name: "IX_Fridges_SmartDietUserId",
                table: "Fridges");

            migrationBuilder.DropIndex(
                name: "IX_DishIngredients_DishId",
                table: "DishIngredients");

            migrationBuilder.DropIndex(
                name: "IX_DishIngredients_FoodId",
                table: "DishIngredients");

            migrationBuilder.AlterColumn<int>(
                name: "SmartDietUserId",
                table: "Fridges",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "SmartDietUserId1",
                table: "Fridges",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FoodId",
                table: "DishIngredients",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<int>(
                name: "DishId",
                table: "DishIngredients",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "DishId1",
                table: "DishIngredients",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FoodId1",
                table: "DishIngredients",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fridges_SmartDietUserId1",
                table: "Fridges",
                column: "SmartDietUserId1");

            migrationBuilder.CreateIndex(
                name: "IX_DishIngredients_DishId1",
                table: "DishIngredients",
                column: "DishId1");

            migrationBuilder.CreateIndex(
                name: "IX_DishIngredients_FoodId1",
                table: "DishIngredients",
                column: "FoodId1");

            migrationBuilder.AddForeignKey(
                name: "FK_DishIngredients_Dishes_DishId1",
                table: "DishIngredients",
                column: "DishId1",
                principalTable: "Dishes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DishIngredients_Foods_FoodId1",
                table: "DishIngredients",
                column: "FoodId1",
                principalTable: "Foods",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Fridges_AspNetUsers_SmartDietUserId1",
                table: "Fridges",
                column: "SmartDietUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
