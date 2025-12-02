using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    /// <inheritdoc />
    public partial class AddBattery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-001",
                column: "created_at",
                value: new DateTime(2025, 10, 19, 8, 17, 54, 913, DateTimeKind.Utc).AddTicks(6263));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-002",
                column: "created_at",
                value: new DateTime(2025, 10, 19, 8, 17, 54, 913, DateTimeKind.Utc).AddTicks(6267));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "user_id",
                keyValue: "admin-001",
                columns: new[] { "created_at", "password" },
                values: new object[] { new DateTime(2025, 10, 19, 8, 17, 54, 913, DateTimeKind.Utc).AddTicks(5628), "$2a$11$WW5WdEialmwNO1KHL1afceHGxYhGpDys2UKjlyBEV.b3bRuZPV./m" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-001",
                column: "created_at",
                value: new DateTime(2025, 10, 19, 0, 17, 35, 659, DateTimeKind.Utc).AddTicks(3660));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-002",
                column: "created_at",
                value: new DateTime(2025, 10, 19, 0, 17, 35, 659, DateTimeKind.Utc).AddTicks(3689));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "user_id",
                keyValue: "admin-001",
                columns: new[] { "created_at", "password" },
                values: new object[] { new DateTime(2025, 10, 19, 0, 17, 35, 659, DateTimeKind.Utc).AddTicks(2873), "$2a$11$iYvEciubrhVVfct4jVr07elpcB13hcBUZVNMQIlnokW0f/iidYNmq" });
        }
    }
}
