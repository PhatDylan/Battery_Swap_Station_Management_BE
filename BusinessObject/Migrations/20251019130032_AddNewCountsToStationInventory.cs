using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    /// <inheritdoc />
    public partial class AddNewCountsToStationInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Thêm available_count
            migrationBuilder.AddColumn<int>(
                name: "available_count",
                table: "StationInventory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Thêm total_count  
            migrationBuilder.AddColumn<int>(
                name: "total_count",
                table: "StationInventory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Thêm quality_pending_count
            migrationBuilder.AddColumn<int>(
                name: "quality_pending_count",
                table: "StationInventory",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "available_count", table: "StationInventory");
            migrationBuilder.DropColumn(name: "total_count", table: "StationInventory");
            migrationBuilder.DropColumn(name: "quality_pending_count", table: "StationInventory");
        }
    }
}