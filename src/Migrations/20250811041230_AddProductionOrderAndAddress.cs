using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AasDemoapp.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionOrderAndAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Vorname = table.Column<string>(type: "TEXT", nullable: true),
                    Strasse = table.Column<string>(type: "TEXT", nullable: true),
                    Plz = table.Column<string>(type: "TEXT", nullable: true),
                    Ort = table.Column<string>(type: "TEXT", nullable: true),
                    Land = table.Column<string>(type: "TEXT", nullable: true),
                    Lat = table.Column<double>(type: "REAL", nullable: true),
                    Long = table.Column<double>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductionOrders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConfiguredProductId = table.Column<long>(type: "INTEGER", nullable: false),
                    Anzahl = table.Column<int>(type: "INTEGER", nullable: false),
                    AddressId = table.Column<long>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionOrders_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrders_ConfiguredProducts_ConfiguredProductId",
                        column: x => x.ConfiguredProductId,
                        principalTable: "ConfiguredProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_AddressId",
                table: "ProductionOrders",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_ConfiguredProductId",
                table: "ProductionOrders",
                column: "ConfiguredProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductionOrders");

            migrationBuilder.DropTable(
                name: "Addresses");
        }
    }
}
