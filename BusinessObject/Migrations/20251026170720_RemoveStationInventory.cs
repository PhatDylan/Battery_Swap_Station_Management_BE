using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStationInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Battery_Reservation_reservation_id",
                table: "Battery");

            migrationBuilder.DropTable(
                name: "Reservation");

            migrationBuilder.DropTable(
                name: "StationInventory");

            migrationBuilder.DropIndex(
                name: "IX_Battery_reservation_id",
                table: "Battery");

            migrationBuilder.DropColumn(
                name: "reservation_id",
                table: "Battery");

            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-001",
                column: "created_at",
                value: new DateTime(2025, 10, 26, 17, 7, 20, 366, DateTimeKind.Utc).AddTicks(4369));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-002",
                column: "created_at",
                value: new DateTime(2025, 10, 26, 17, 7, 20, 366, DateTimeKind.Utc).AddTicks(4393));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "user_id",
                keyValue: "admin-001",
                columns: new[] { "created_at", "password" },
                values: new object[] { new DateTime(2025, 10, 26, 17, 7, 20, 366, DateTimeKind.Utc).AddTicks(3454), "$2a$11$otgFM3KBbnaTgyDzXClceeU4zAhckWIW4LOeKm/Gr7ijraSt1lxuq" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "reservation_id",
                table: "Battery",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StationInventory",
                columns: table => new
                {
                    station_inventory_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    station_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    available_count = table.Column<int>(type: "int", nullable: false),
                    charging_count = table.Column<int>(type: "int", nullable: false),
                    full_count = table.Column<int>(type: "int", nullable: false),
                    last_update = table.Column<DateTime>(type: "datetime2", nullable: false),
                    maintenance_count = table.Column<int>(type: "int", nullable: false),
                    quality_pending_count = table.Column<int>(type: "int", nullable: false),
                    total_count = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StationInventory", x => x.station_inventory_id);
                    table.ForeignKey(
                        name: "FK_StationInventory_Station_station_id",
                        column: x => x.station_id,
                        principalTable: "Station",
                        principalColumn: "station_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservation",
                columns: table => new
                {
                    reservation_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    station_inventory_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    expired_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    reserved_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservation", x => x.reservation_id);
                    table.ForeignKey(
                        name: "FK_Reservation_StationInventory_station_inventory_id",
                        column: x => x.station_inventory_id,
                        principalTable: "StationInventory",
                        principalColumn: "station_inventory_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-001",
                column: "created_at",
                value: new DateTime(2025, 10, 26, 15, 50, 13, 436, DateTimeKind.Utc).AddTicks(5501));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-002",
                column: "created_at",
                value: new DateTime(2025, 10, 26, 15, 50, 13, 436, DateTimeKind.Utc).AddTicks(5520));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "user_id",
                keyValue: "admin-001",
                columns: new[] { "created_at", "password" },
                values: new object[] { new DateTime(2025, 10, 26, 15, 50, 13, 436, DateTimeKind.Utc).AddTicks(4437), "$2a$11$YbvaSk4n76RvkCt7/KTUwu6rv/xEd5RQxRO0kesenU5iTszT.ohge" });

            migrationBuilder.CreateIndex(
                name: "IX_Battery_reservation_id",
                table: "Battery",
                column: "reservation_id");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_station_inventory_id",
                table: "Reservation",
                column: "station_inventory_id");

            migrationBuilder.CreateIndex(
                name: "IX_StationInventory_station_id",
                table: "StationInventory",
                column: "station_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Battery_Reservation_reservation_id",
                table: "Battery",
                column: "reservation_id",
                principalTable: "Reservation",
                principalColumn: "reservation_id");
        }
    }
}
