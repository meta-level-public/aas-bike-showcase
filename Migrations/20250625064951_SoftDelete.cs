using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AasDemoapp.Migrations
{
    /// <inheritdoc />
    public partial class SoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "UpdateableShells",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "UpdateableShells",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ProductParts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ProductParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ProducedProducts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ProducedProducts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "KatalogEintraege",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "KatalogEintraege",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ImportedShells",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ImportedShells",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ConfiguredProducts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ConfiguredProducts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "UpdateableShells");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "UpdateableShells");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ProductParts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ProductParts");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ProducedProducts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ProducedProducts");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "KatalogEintraege");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "KatalogEintraege");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ImportedShells");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ImportedShells");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ConfiguredProducts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ConfiguredProducts");
        }
    }
}
