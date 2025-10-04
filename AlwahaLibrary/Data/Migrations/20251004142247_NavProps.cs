using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlwahaLibrary.Data.Migrations
{
    /// <inheritdoc />
    public partial class NavProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ItemTagTagId",
                table: "ItemToTags",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MenuItemItemId",
                table: "ItemToTags",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ItemToTags_ItemTagTagId",
                table: "ItemToTags",
                column: "ItemTagTagId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemToTags_MenuItemItemId",
                table: "ItemToTags",
                column: "MenuItemItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemToTags_ItemTags_ItemTagTagId",
                table: "ItemToTags",
                column: "ItemTagTagId",
                principalTable: "ItemTags",
                principalColumn: "TagId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemToTags_MenuItems_MenuItemItemId",
                table: "ItemToTags",
                column: "MenuItemItemId",
                principalTable: "MenuItems",
                principalColumn: "ItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemToTags_ItemTags_ItemTagTagId",
                table: "ItemToTags");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemToTags_MenuItems_MenuItemItemId",
                table: "ItemToTags");

            migrationBuilder.DropIndex(
                name: "IX_ItemToTags_ItemTagTagId",
                table: "ItemToTags");

            migrationBuilder.DropIndex(
                name: "IX_ItemToTags_MenuItemItemId",
                table: "ItemToTags");

            migrationBuilder.DropColumn(
                name: "ItemTagTagId",
                table: "ItemToTags");

            migrationBuilder.DropColumn(
                name: "MenuItemItemId",
                table: "ItemToTags");
        }
    }
}
