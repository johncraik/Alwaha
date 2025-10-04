using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlwahaLibrary.Data.Migrations
{
    /// <inheritdoc />
    public partial class StuffToSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemToTags_ItemTags_ItemTagTagId",
                table: "ItemToTags");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemToTags_MenuItems_MenuItemItemId",
                table: "ItemToTags");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_MenuItems_MenuItemItemId",
                table: "MenuItems");

            migrationBuilder.DropIndex(
                name: "IX_MenuItems_MenuItemItemId",
                table: "MenuItems");

            migrationBuilder.DropIndex(
                name: "IX_ItemToTags_ItemTagTagId",
                table: "ItemToTags");

            migrationBuilder.DropIndex(
                name: "IX_ItemToTags_MenuItemItemId",
                table: "ItemToTags");

            migrationBuilder.DropColumn(
                name: "MenuItemItemId",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "ItemTagTagId",
                table: "ItemToTags");

            migrationBuilder.DropColumn(
                name: "MenuItemItemId",
                table: "ItemToTags");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ItemToSets");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "ItemToSets");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "ItemToSets");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "ItemToSets");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ItemToSets");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ItemToSets");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "ItemToSets");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "MenuItemItemId",
                table: "MenuItems",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

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

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ItemToSets",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "ItemToSets",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "ItemToSets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "ItemToSets",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ItemToSets",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "ItemToSets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "ItemToSets",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_MenuItemItemId",
                table: "MenuItems",
                column: "MenuItemItemId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_MenuItems_MenuItemItemId",
                table: "MenuItems",
                column: "MenuItemItemId",
                principalTable: "MenuItems",
                principalColumn: "ItemId");
        }
    }
}
