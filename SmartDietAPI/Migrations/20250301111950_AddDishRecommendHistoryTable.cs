using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartDietAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddDishRecommendHistoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SmartDietUserId",
                table: "UserFeedbacks",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "DishRecommendHistories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SmartDietUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DishId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RecommendationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishRecommendHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DishRecommendHistories_AspNetUsers_SmartDietUserId",
                        column: x => x.SmartDietUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DishRecommendHistories_Dishes_DishId",
                        column: x => x.DishId,
                        principalTable: "Dishes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserFeedbacks_SmartDietUserId",
                table: "UserFeedbacks",
                column: "SmartDietUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DishRecommendHistories_DishId",
                table: "DishRecommendHistories",
                column: "DishId");

            migrationBuilder.CreateIndex(
                name: "IX_DishRecommendHistories_SmartDietUserId",
                table: "DishRecommendHistories",
                column: "SmartDietUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFeedbacks_AspNetUsers_SmartDietUserId",
                table: "UserFeedbacks",
                column: "SmartDietUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFeedbacks_AspNetUsers_SmartDietUserId",
                table: "UserFeedbacks");

            migrationBuilder.DropTable(
                name: "DishRecommendHistories");

            migrationBuilder.DropIndex(
                name: "IX_UserFeedbacks_SmartDietUserId",
                table: "UserFeedbacks");

            migrationBuilder.AlterColumn<string>(
                name: "SmartDietUserId",
                table: "UserFeedbacks",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
