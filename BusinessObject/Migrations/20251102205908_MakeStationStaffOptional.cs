using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    /// <inheritdoc />
    public partial class MakeStationStaffOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "station_staff_id",
                table: "BatterySwap",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-001",
                column: "created_at",
                value: new DateTime(2025, 11, 2, 20, 59, 7, 894, DateTimeKind.Utc).AddTicks(9853));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-002",
                column: "created_at",
                value: new DateTime(2025, 11, 2, 20, 59, 7, 894, DateTimeKind.Utc).AddTicks(9882));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "user_id",
                keyValue: "admin-001",
                columns: new[] { "created_at", "password" },
                values: new object[] { new DateTime(2025, 11, 2, 20, 59, 7, 894, DateTimeKind.Utc).AddTicks(8857), "$2a$11$2fVlSSqGH8.0PsdwYf/GAuIkYWeKRbT2XN5QXr1XC.olyegRe30xa" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "station_staff_id",
                table: "BatterySwap",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-001",
                column: "created_at",
                value: new DateTime(2025, 10, 27, 15, 12, 44, 916, DateTimeKind.Utc).AddTicks(1955));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-002",
                column: "created_at",
                value: new DateTime(2025, 10, 27, 15, 12, 44, 916, DateTimeKind.Utc).AddTicks(1960));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "user_id",
                keyValue: "admin-001",
                columns: new[] { "created_at", "password" },
                values: new object[] { new DateTime(2025, 10, 27, 15, 12, 44, 916, DateTimeKind.Utc).AddTicks(845), "$2a$11$SVWVNCUFi3LxTkjJSnUjxuFgBGjim42SFaHTBuiT.Q74xL7AJQg5W" });
        }
    }
}
