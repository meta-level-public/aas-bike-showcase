using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AasDemoapp.Migrations
{
    /// <inheritdoc />
    public partial class SupplierSecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "value",
                table: "Settings",
                newName: "Value");

            migrationBuilder.AddColumn<string>(
                name: "SecuritySetting",
                table: "Suppliers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecuritySetting",
                table: "Suppliers");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "Settings",
                newName: "value");
        }
    }
}
