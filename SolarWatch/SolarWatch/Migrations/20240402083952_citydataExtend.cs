using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarWatch.Migrations
{
    /// <inheritdoc />
    public partial class citydataExtend : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "CityDatas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                table: "CityDatas",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "CityDatas");

            migrationBuilder.DropColumn(
                name: "TimeZone",
                table: "CityDatas");
        }
    }
}
