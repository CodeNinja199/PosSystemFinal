using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StoreId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StoreId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StoreId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StoreId",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Stores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stores", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_StoreId",
                table: "Users",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_StoreId",
                table: "Products",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_StoreId",
                table: "Orders",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_StoreId",
                table: "Categories",
                column: "StoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Stores_StoreId",
                table: "Categories",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Stores_StoreId",
                table: "Orders",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Stores_StoreId",
                table: "Products",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Stores_StoreId",
                table: "Users",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Stores_StoreId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Stores_StoreId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Stores_StoreId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Stores_StoreId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Users_StoreId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Products_StoreId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Orders_StoreId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Categories_StoreId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Categories");
        }
    }
}