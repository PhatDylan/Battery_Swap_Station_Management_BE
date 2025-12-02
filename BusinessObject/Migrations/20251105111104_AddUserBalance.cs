using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    /// <inheritdoc />
    public partial class AddUserBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "balance",
                table: "User",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-001",
                column: "created_at",
                value: new DateTime(2025, 11, 5, 11, 11, 3, 499, DateTimeKind.Utc).AddTicks(1353));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-002",
                column: "created_at",
                value: new DateTime(2025, 11, 5, 11, 11, 3, 499, DateTimeKind.Utc).AddTicks(1377));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "user_id",
                keyValue: "admin-001",
                columns: new[] { "balance", "created_at", "password" },
                values: new object[] { 0.0, new DateTime(2025, 11, 5, 11, 11, 3, 499, DateTimeKind.Utc).AddTicks(256), "$2a$11$S5jyyNnh7ZlgZ3DK8JdvGOt9QXIw9UjeI2Irr3yQ0iOyeaPrEuAwu" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "balance",
                table: "User");

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
    }
}
