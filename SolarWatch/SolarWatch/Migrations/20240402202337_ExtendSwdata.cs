using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarWatch.Migrations
{
    /// <inheritdoc />
    public partial class ExtendSwdata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "SolarWatchDataTable",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                table: "SolarWatchDataTable",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "SolarWatchDataTable");

            migrationBuilder.DropColumn(
                name: "TimeZone",
                table: "SolarWatchDataTable");
        }
    }
}
