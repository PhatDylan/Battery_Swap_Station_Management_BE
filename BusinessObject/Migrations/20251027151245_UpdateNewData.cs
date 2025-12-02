using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNewData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
