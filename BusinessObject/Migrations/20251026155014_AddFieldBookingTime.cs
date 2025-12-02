using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldBookingTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "booking_time",
                table: "Booking",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "booking_time",
                table: "Booking");

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
    }
}
