using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarWatch.Migrations
{
    /// <inheritdoc />
    public partial class RenameTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CityDataTable",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    TimeZone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CityDataTable", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SolarWatchDataTable",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Sunrise = table.Column<TimeSpan>(type: "time", nullable: false),
                    Sunset = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolarWatchDataTable", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SolarWatchDataTable",
                table: "SolarWatchDataTable");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CityDataTable",
                table: "CityDataTable");

            migrationBuilder.RenameTable(
                name: "SolarWatchDataTable",
                newName: "SolarWatchDatas");

            migrationBuilder.RenameTable(
                name: "CityDataTable",
                newName: "CityDatas");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SolarWatchDatas",
                table: "SolarWatchDatas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CityDatas",
                table: "CityDatas",
                column: "Id");
        }
    }
}
