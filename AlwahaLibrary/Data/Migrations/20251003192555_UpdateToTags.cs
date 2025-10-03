using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlwahaLibrary.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateToTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ItemTags",
                keyColumn: "Icon",
                keyValue: null,
                column: "Icon",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                table: "ItemTags",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "ItemTags",
                keyColumn: "Colour",
                keyValue: null,
                column: "Colour",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Colour",
                table: "ItemTags",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ItemTags",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "ItemTags",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "ItemTags",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "ItemTags",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ItemTags",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "ItemTags",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "ItemTags",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ItemTags");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "ItemTags");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "ItemTags");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "ItemTags");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ItemTags");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ItemTags");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "ItemTags");

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                table: "ItemTags",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Colour",
                table: "ItemTags",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
