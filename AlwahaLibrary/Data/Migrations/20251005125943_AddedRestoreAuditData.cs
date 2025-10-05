using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlwahaLibrary.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedRestoreAuditData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemToSets_MenuItems_MenuItemItemId",
                table: "ItemToSets");

            migrationBuilder.DropIndex(
                name: "IX_ItemToSets_MenuItemItemId",
                table: "ItemToSets");

            migrationBuilder.DropColumn(
                name: "MenuItemItemId",
                table: "ItemToSets");

            migrationBuilder.AddColumn<string>(
                name: "RestoredBy",
                table: "MenuItems",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "RestoredDate",
                table: "MenuItems",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RestoredBy",
                table: "ItemTypes",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "RestoredDate",
                table: "ItemTypes",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RestoredBy",
                table: "ItemTags",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "RestoredDate",
                table: "ItemTags",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RestoredBy",
                table: "BundleItems",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "RestoredDate",
                table: "BundleItems",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RestoredBy",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "RestoredDate",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "RestoredBy",
                table: "ItemTypes");

            migrationBuilder.DropColumn(
                name: "RestoredDate",
                table: "ItemTypes");

            migrationBuilder.DropColumn(
                name: "RestoredBy",
                table: "ItemTags");

            migrationBuilder.DropColumn(
                name: "RestoredDate",
                table: "ItemTags");

            migrationBuilder.DropColumn(
                name: "RestoredBy",
                table: "BundleItems");

            migrationBuilder.DropColumn(
                name: "RestoredDate",
                table: "BundleItems");

            migrationBuilder.AddColumn<string>(
                name: "MenuItemItemId",
                table: "ItemToSets",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ItemToSets_MenuItemItemId",
                table: "ItemToSets",
                column: "MenuItemItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemToSets_MenuItems_MenuItemItemId",
                table: "ItemToSets",
                column: "MenuItemItemId",
                principalTable: "MenuItems",
                principalColumn: "ItemId");
        }
    }
}
