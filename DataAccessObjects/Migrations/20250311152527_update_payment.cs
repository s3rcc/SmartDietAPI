using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessObjects.Migrations
{
    /// <inheritdoc />
    public partial class update_payment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubcriptionId",
                table: "UserPayments",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_UserPayments_SubcriptionId",
                table: "UserPayments",
                column: "SubcriptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPayments_Subcriptions_SubcriptionId",
                table: "UserPayments",
                column: "SubcriptionId",
                principalTable: "Subcriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPayments_Subcriptions_SubcriptionId",
                table: "UserPayments");

            migrationBuilder.DropIndex(
                name: "IX_UserPayments_SubcriptionId",
                table: "UserPayments");

            migrationBuilder.DropColumn(
                name: "SubcriptionId",
                table: "UserPayments");
        }
    }
}
