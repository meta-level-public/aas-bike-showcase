using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AasDemoapp.Migrations
{
    /// <inheritdoc />
    public partial class LocalAasId1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LocalAasId",
                table: "KatalogEintraege",
                type: "TEXT",
                nullable: false,
                defaultValue: ""
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "LocalAasId", table: "KatalogEintraege");
        }
    }
}
