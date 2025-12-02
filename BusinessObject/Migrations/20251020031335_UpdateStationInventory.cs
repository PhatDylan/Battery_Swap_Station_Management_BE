using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStationInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-001",
                column: "created_at",
                value: new DateTime(2025, 10, 20, 3, 13, 34, 730, DateTimeKind.Utc).AddTicks(2946));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-002",
                column: "created_at",
                value: new DateTime(2025, 10, 20, 3, 13, 34, 730, DateTimeKind.Utc).AddTicks(2950));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "user_id",
                keyValue: "admin-001",
                columns: new[] { "created_at", "password" },
                values: new object[] { new DateTime(2025, 10, 20, 3, 13, 34, 730, DateTimeKind.Utc).AddTicks(2119), "$2a$11$MdR9of5i0ewrtpu.1JclcuR7FYtHTBzEpk.DPotqzQo8AXe8.C3ji" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-001",
                column: "created_at",
                value: new DateTime(2025, 10, 19, 13, 0, 31, 826, DateTimeKind.Utc).AddTicks(9005));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-002",
                column: "created_at",
                value: new DateTime(2025, 10, 19, 13, 0, 31, 826, DateTimeKind.Utc).AddTicks(9021));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "user_id",
                keyValue: "admin-001",
                columns: new[] { "created_at", "password" },
                values: new object[] { new DateTime(2025, 10, 19, 13, 0, 31, 826, DateTimeKind.Utc).AddTicks(8175), "$2a$11$yq/ty82RxZ2IK76td6iXIuCMAdoLBtoJuXBNdzDQluF3v/5kTBVBu" });
        }
    }
}
