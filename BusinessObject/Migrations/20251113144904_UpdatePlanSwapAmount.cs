using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePlanSwapAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "swaps_included",
                table: "SubscriptionPlan");

            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-001",
                columns: new[] { "created_at", "swap_amount" },
                values: new object[] { new DateTime(2025, 11, 13, 14, 49, 3, 467, DateTimeKind.Utc).AddTicks(9290), 10 });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-002",
                columns: new[] { "created_at", "swap_amount" },
                values: new object[] { new DateTime(2025, 11, 13, 14, 49, 3, 467, DateTimeKind.Utc).AddTicks(9313), 20 });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "user_id",
                keyValue: "admin-001",
                columns: new[] { "created_at", "password" },
                values: new object[] { new DateTime(2025, 11, 13, 14, 49, 3, 467, DateTimeKind.Utc).AddTicks(6590), "$2a$11$gL0CVamquWJmDHEq7Qsjxe4C7ioYAh3bt7BQP26hIOqyXQ1ZAgIQy" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "swaps_included",
                table: "SubscriptionPlan",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-001",
                columns: new[] { "created_at", "swap_amount", "swaps_included" },
                values: new object[] { new DateTime(2025, 11, 5, 11, 11, 3, 499, DateTimeKind.Utc).AddTicks(1353), 0, "10" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-002",
                columns: new[] { "created_at", "swap_amount", "swaps_included" },
                values: new object[] { new DateTime(2025, 11, 5, 11, 11, 3, 499, DateTimeKind.Utc).AddTicks(1377), 0, "25" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "user_id",
                keyValue: "admin-001",
                columns: new[] { "created_at", "password" },
                values: new object[] { new DateTime(2025, 11, 5, 11, 11, 3, 499, DateTimeKind.Utc).AddTicks(256), "$2a$11$S5jyyNnh7ZlgZ3DK8JdvGOt9QXIw9UjeI2Irr3yQ0iOyeaPrEuAwu" });
        }
    }
}
