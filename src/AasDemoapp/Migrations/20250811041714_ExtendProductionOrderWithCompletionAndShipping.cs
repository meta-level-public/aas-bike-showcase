using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AasDemoapp.Migrations
{
    /// <inheritdoc />
    public partial class ExtendProductionOrderWithCompletionAndShipping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FertigstellungsDatum",
                table: "ProductionOrders",
                type: "TEXT",
                nullable: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "ProduktionAbgeschlossen",
                table: "ProductionOrders",
                type: "INTEGER",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "VersandDatum",
                table: "ProductionOrders",
                type: "TEXT",
                nullable: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "Versandt",
                table: "ProductionOrders",
                type: "INTEGER",
                nullable: false,
                defaultValue: false
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "FertigstellungsDatum", table: "ProductionOrders");

            migrationBuilder.DropColumn(name: "ProduktionAbgeschlossen", table: "ProductionOrders");

            migrationBuilder.DropColumn(name: "VersandDatum", table: "ProductionOrders");

            migrationBuilder.DropColumn(name: "Versandt", table: "ProductionOrders");
        }
    }
}
