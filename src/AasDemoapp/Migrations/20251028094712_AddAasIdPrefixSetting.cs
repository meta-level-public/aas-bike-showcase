using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AasDemoapp.Migrations
{
    /// <inheritdoc />
    public partial class AddAasIdPrefixSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"INSERT INTO Settings (Name, Value) 
                  SELECT 'AasIdPrefix', 'https://oi4-nextbike.de' 
                  WHERE NOT EXISTS (SELECT 1 FROM Settings WHERE Name = 'AasIdPrefix');"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Settings WHERE Name = 'AasIdPrefix';");
        }
    }
}
