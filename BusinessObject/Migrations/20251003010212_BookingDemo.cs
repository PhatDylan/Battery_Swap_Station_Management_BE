using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    /// <inheritdoc />
    public partial class BookingDemo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BatterySwap_Station_station_id",
                table: "BatterySwap");

            migrationBuilder.DropForeignKey(
                name: "FK_BatterySwap_User_user_id",
                table: "BatterySwap");

            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Vehicle_VehiclesId",
                table: "Booking");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_BatterySwap_swap_id",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_Station_station_id",
                table: "Review");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_User_user_id",
                table: "Review");

            migrationBuilder.DropForeignKey(
                name: "FK_Station_User_user_id",
                table: "Station");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscription_SubscriptionPlan_plan_id",
                table: "Subscription");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPayment_User_user_id",
                table: "SubscriptionPayment");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportTicket_Station_station_id",
                table: "SupportTicket");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportTicket_User_user_id",
                table: "SupportTicket");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicle_Battery_battery_id",
                table: "Vehicle");

            migrationBuilder.DropIndex(
                name: "IX_Subscription_plan_id",
                table: "Subscription");

            migrationBuilder.DropIndex(
                name: "IX_Payment_swap_id",
                table: "Payment");

            migrationBuilder.DropIndex(
                name: "IX_Booking_VehiclesId",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "VehiclesId",
                table: "Booking");

            migrationBuilder.RenameColumn(
                name: "date",
                table: "Booking",
                newName: "booking_date");

            migrationBuilder.AddColumn<string>(
                name: "BatteryId1",
                table: "Vehicle",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StationId1",
                table: "SupportTicket",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "SupportTicket",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "SubscriptionPayment",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "plan_id",
                table: "Subscription",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "SubscriptionPlanPlanId",
                table: "Subscription",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "swap_id",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "BatterySwapSwapId",
                table: "Payment",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "complete_at",
                table: "Booking",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "confirm_by",
                table: "Booking",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "time_slot",
                table: "Booking",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "Booking",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StationId1",
                table: "BatterySwap",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "BatterySwap",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-001",
                column: "created_at",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-002",
                column: "created_at",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "user_id",
                keyValue: "admin-001",
                columns: new[] { "created_at", "password" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "$2a$11$pDMVBuBf.cieLNUB/aCT1.FYbxwR/wEcQz22Qgl6r.SXBdq2zBxSO" });

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_BatteryId1",
                table: "Vehicle",
                column: "BatteryId1");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicket_StationId1",
                table: "SupportTicket",
                column: "StationId1");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicket_UserId1",
                table: "SupportTicket",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayment_UserId1",
                table: "SubscriptionPayment",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Subscription_SubscriptionPlanPlanId",
                table: "Subscription",
                column: "SubscriptionPlanPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_BatterySwapSwapId",
                table: "Payment",
                column: "BatterySwapSwapId");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_vehicle_id",
                table: "Booking",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "IX_BatterySwap_StationId1",
                table: "BatterySwap",
                column: "StationId1");

            migrationBuilder.CreateIndex(
                name: "IX_BatterySwap_UserId1",
                table: "BatterySwap",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_BatterySwap_Station_StationId1",
                table: "BatterySwap",
                column: "StationId1",
                principalTable: "Station",
                principalColumn: "station_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BatterySwap_Station_station_id",
                table: "BatterySwap",
                column: "station_id",
                principalTable: "Station",
                principalColumn: "station_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BatterySwap_User_UserId1",
                table: "BatterySwap",
                column: "UserId1",
                principalTable: "User",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BatterySwap_User_user_id",
                table: "BatterySwap",
                column: "user_id",
                principalTable: "User",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Vehicle_vehicle_id",
                table: "Booking",
                column: "vehicle_id",
                principalTable: "Vehicle",
                principalColumn: "vehicles_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_BatterySwap_BatterySwapSwapId",
                table: "Payment",
                column: "BatterySwapSwapId",
                principalTable: "BatterySwap",
                principalColumn: "swap_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Station_station_id",
                table: "Review",
                column: "station_id",
                principalTable: "Station",
                principalColumn: "station_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Review_User_user_id",
                table: "Review",
                column: "user_id",
                principalTable: "User",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Station_User_user_id",
                table: "Station",
                column: "user_id",
                principalTable: "User",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscription_SubscriptionPlan_SubscriptionPlanPlanId",
                table: "Subscription",
                column: "SubscriptionPlanPlanId",
                principalTable: "SubscriptionPlan",
                principalColumn: "plan_id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPayment_User_UserId1",
                table: "SubscriptionPayment",
                column: "UserId1",
                principalTable: "User",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPayment_User_user_id",
                table: "SubscriptionPayment",
                column: "user_id",
                principalTable: "User",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTicket_Station_StationId1",
                table: "SupportTicket",
                column: "StationId1",
                principalTable: "Station",
                principalColumn: "station_id");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTicket_Station_station_id",
                table: "SupportTicket",
                column: "station_id",
                principalTable: "Station",
                principalColumn: "station_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTicket_User_UserId1",
                table: "SupportTicket",
                column: "UserId1",
                principalTable: "User",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTicket_User_user_id",
                table: "SupportTicket",
                column: "user_id",
                principalTable: "User",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicle_Battery_BatteryId1",
                table: "Vehicle",
                column: "BatteryId1",
                principalTable: "Battery",
                principalColumn: "battery_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicle_Battery_battery_id",
                table: "Vehicle",
                column: "battery_id",
                principalTable: "Battery",
                principalColumn: "battery_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BatterySwap_Station_StationId1",
                table: "BatterySwap");

            migrationBuilder.DropForeignKey(
                name: "FK_BatterySwap_Station_station_id",
                table: "BatterySwap");

            migrationBuilder.DropForeignKey(
                name: "FK_BatterySwap_User_UserId1",
                table: "BatterySwap");

            migrationBuilder.DropForeignKey(
                name: "FK_BatterySwap_User_user_id",
                table: "BatterySwap");

            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Vehicle_vehicle_id",
                table: "Booking");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_BatterySwap_BatterySwapSwapId",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_Station_station_id",
                table: "Review");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_User_user_id",
                table: "Review");

            migrationBuilder.DropForeignKey(
                name: "FK_Station_User_user_id",
                table: "Station");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscription_SubscriptionPlan_SubscriptionPlanPlanId",
                table: "Subscription");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPayment_User_UserId1",
                table: "SubscriptionPayment");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPayment_User_user_id",
                table: "SubscriptionPayment");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportTicket_Station_StationId1",
                table: "SupportTicket");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportTicket_Station_station_id",
                table: "SupportTicket");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportTicket_User_UserId1",
                table: "SupportTicket");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportTicket_User_user_id",
                table: "SupportTicket");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicle_Battery_BatteryId1",
                table: "Vehicle");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicle_Battery_battery_id",
                table: "Vehicle");

            migrationBuilder.DropIndex(
                name: "IX_Vehicle_BatteryId1",
                table: "Vehicle");

            migrationBuilder.DropIndex(
                name: "IX_SupportTicket_StationId1",
                table: "SupportTicket");

            migrationBuilder.DropIndex(
                name: "IX_SupportTicket_UserId1",
                table: "SupportTicket");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPayment_UserId1",
                table: "SubscriptionPayment");

            migrationBuilder.DropIndex(
                name: "IX_Subscription_SubscriptionPlanPlanId",
                table: "Subscription");

            migrationBuilder.DropIndex(
                name: "IX_Payment_BatterySwapSwapId",
                table: "Payment");

            migrationBuilder.DropIndex(
                name: "IX_Booking_vehicle_id",
                table: "Booking");

            migrationBuilder.DropIndex(
                name: "IX_BatterySwap_StationId1",
                table: "BatterySwap");

            migrationBuilder.DropIndex(
                name: "IX_BatterySwap_UserId1",
                table: "BatterySwap");

            migrationBuilder.DropColumn(
                name: "BatteryId1",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "StationId1",
                table: "SupportTicket");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "SupportTicket");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "SubscriptionPayment");

            migrationBuilder.DropColumn(
                name: "SubscriptionPlanPlanId",
                table: "Subscription");

            migrationBuilder.DropColumn(
                name: "BatterySwapSwapId",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "complete_at",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "confirm_by",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "time_slot",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "StationId1",
                table: "BatterySwap");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "BatterySwap");

            migrationBuilder.RenameColumn(
                name: "booking_date",
                table: "Booking",
                newName: "date");

            migrationBuilder.AlterColumn<string>(
                name: "plan_id",
                table: "Subscription",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "swap_id",
                table: "Payment",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "VehiclesId",
                table: "Booking",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-001",
                column: "created_at",
                value: new DateTime(2025, 10, 1, 9, 10, 54, 245, DateTimeKind.Utc).AddTicks(2381));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-002",
                column: "created_at",
                value: new DateTime(2025, 10, 1, 9, 10, 54, 245, DateTimeKind.Utc).AddTicks(2409));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "user_id",
                keyValue: "admin-001",
                columns: new[] { "created_at", "password" },
                values: new object[] { new DateTime(2025, 10, 1, 9, 10, 54, 245, DateTimeKind.Utc).AddTicks(1374), "$2a$11$SymoTl0N6SBGODyfgObM/uuZfEiYKftoeeEhUKbp6i3HTD/9Jp8rS" });

            migrationBuilder.CreateIndex(
                name: "IX_Subscription_plan_id",
                table: "Subscription",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_swap_id",
                table: "Payment",
                column: "swap_id");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_VehiclesId",
                table: "Booking",
                column: "VehiclesId");

            migrationBuilder.AddForeignKey(
                name: "FK_BatterySwap_Station_station_id",
                table: "BatterySwap",
                column: "station_id",
                principalTable: "Station",
                principalColumn: "station_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BatterySwap_User_user_id",
                table: "BatterySwap",
                column: "user_id",
                principalTable: "User",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Vehicle_VehiclesId",
                table: "Booking",
                column: "VehiclesId",
                principalTable: "Vehicle",
                principalColumn: "vehicles_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_BatterySwap_swap_id",
                table: "Payment",
                column: "swap_id",
                principalTable: "BatterySwap",
                principalColumn: "swap_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Station_station_id",
                table: "Review",
                column: "station_id",
                principalTable: "Station",
                principalColumn: "station_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Review_User_user_id",
                table: "Review",
                column: "user_id",
                principalTable: "User",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Station_User_user_id",
                table: "Station",
                column: "user_id",
                principalTable: "User",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscription_SubscriptionPlan_plan_id",
                table: "Subscription",
                column: "plan_id",
                principalTable: "SubscriptionPlan",
                principalColumn: "plan_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPayment_User_user_id",
                table: "SubscriptionPayment",
                column: "user_id",
                principalTable: "User",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTicket_Station_station_id",
                table: "SupportTicket",
                column: "station_id",
                principalTable: "Station",
                principalColumn: "station_id");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTicket_User_user_id",
                table: "SupportTicket",
                column: "user_id",
                principalTable: "User",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicle_Battery_battery_id",
                table: "Vehicle",
                column: "battery_id",
                principalTable: "Battery",
                principalColumn: "battery_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
