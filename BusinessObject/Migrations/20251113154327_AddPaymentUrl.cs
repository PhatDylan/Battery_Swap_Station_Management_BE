using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "payment_url",
                table: "SubscriptionPayment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_url",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-001",
                column: "created_at",
                value: new DateTime(2025, 11, 13, 15, 43, 27, 29, DateTimeKind.Utc).AddTicks(51));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-002",
                column: "created_at",
                value: new DateTime(2025, 11, 13, 15, 43, 27, 29, DateTimeKind.Utc).AddTicks(78));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "user_id",
                keyValue: "admin-001",
                columns: new[] { "created_at", "password" },
                values: new object[] { new DateTime(2025, 11, 13, 15, 43, 27, 28, DateTimeKind.Utc).AddTicks(7744), "$2a$11$Q0B.URCrt4J.71oYI/HU..oUADEA7P4v1GnXopWka6afofkXJbrbW" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payment_url",
                table: "SubscriptionPayment");

            migrationBuilder.DropColumn(
                name: "payment_url",
                table: "Payment");

            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-001",
                column: "created_at",
                value: new DateTime(2025, 11, 13, 14, 49, 3, 467, DateTimeKind.Utc).AddTicks(9290));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlan",
                keyColumn: "plan_id",
                keyValue: "plan-002",
                column: "created_at",
                value: new DateTime(2025, 11, 13, 14, 49, 3, 467, DateTimeKind.Utc).AddTicks(9313));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "user_id",
                keyValue: "admin-001",
                columns: new[] { "created_at", "password" },
                values: new object[] { new DateTime(2025, 11, 13, 14, 49, 3, 467, DateTimeKind.Utc).AddTicks(6590), "$2a$11$gL0CVamquWJmDHEq7Qsjxe4C7ioYAh3bt7BQP26hIOqyXQ1ZAgIQy" });
        }
    }
}
