using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AasDemoapp.Migrations
{
    /// <inheritdoc />
    public partial class ToolRepo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ToolRepos",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Logo = table.Column<string>(type: "TEXT", nullable: false),
                    RemoteAasRepositoryUrl = table.Column<string>(type: "TEXT", nullable: false),
                    RemoteSmRepositoryUrl = table.Column<string>(type: "TEXT", nullable: false),
                    RemoteAasRegistryUrl = table.Column<string>(type: "TEXT", nullable: false),
                    RemoteSmRegistryUrl = table.Column<string>(type: "TEXT", nullable: false),
                    RemoteDiscoveryUrl = table.Column<string>(type: "TEXT", nullable: false),
                    RemoteCdRepositoryUrl = table.Column<string>(type: "TEXT", nullable: false),
                    SecuritySetting = table.Column<string>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolRepos", x => x.Id);
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ToolRepos");
        }
    }
}
