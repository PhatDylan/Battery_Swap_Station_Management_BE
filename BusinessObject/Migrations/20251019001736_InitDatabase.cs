using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BusinessObject.Migrations
{
    /// <inheritdoc />
    public partial class InitDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BatteryType",
                columns: table => new
                {
                    battery_type_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    battery_type_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatteryType", x => x.battery_type_id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlan",
                columns: table => new
                {
                    plan_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    monthly_fee = table.Column<double>(type: "float", nullable: false),
                    swaps_included = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    swap_amount = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlan", x => x.plan_id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    full_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    role = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    avatar_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetToken",
                columns: table => new
                {
                    reset_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    otp_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    consumed = table.Column<bool>(type: "bit", nullable: false),
                    attempt_count = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetToken", x => x.reset_id);
                    table.ForeignKey(
                        name: "FK_PasswordResetToken_User_user_id",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffAbsense",
                columns: table => new
                {
                    staff_absence_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffAbsense", x => x.staff_absence_id);
                    table.ForeignKey(
                        name: "FK_StaffAbsense_User_user_id",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Station",
                columns: table => new
                {
                    station_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    latitude = table.Column<double>(type: "float", nullable: false),
                    longitude = table.Column<double>(type: "float", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Station", x => x.station_id);
                    table.ForeignKey(
                        name: "FK_Station_User_user_id",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Subscription",
                columns: table => new
                {
                    subscription_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    plan_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    start_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    number_of_swaps = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscription", x => x.subscription_id);
                    table.ForeignKey(
                        name: "FK_Subscription_SubscriptionPlan_plan_id",
                        column: x => x.plan_id,
                        principalTable: "SubscriptionPlan",
                        principalColumn: "plan_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subscription_User_user_id",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StationInventory",
                columns: table => new
                {
                    station_inventory_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    station_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    maintenance_count = table.Column<int>(type: "int", nullable: false),
                    full_count = table.Column<int>(type: "int", nullable: false),
                    charging_count = table.Column<int>(type: "int", nullable: false),
                    last_update = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                name: "StationStaff",
                columns: table => new
                {
                    station_staff_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    station_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StationStaff", x => x.station_staff_id);
                    table.ForeignKey(
                        name: "FK_StationStaff_Station_station_id",
                        column: x => x.station_id,
                        principalTable: "Station",
                        principalColumn: "station_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StationStaff_User_user_id",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StationStaffOverride",
                columns: table => new
                {
                    station_staff_override_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    station_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    shift_id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StationStaffOverride", x => x.station_staff_override_id);
                    table.ForeignKey(
                        name: "FK_StationStaffOverride_Station_station_id",
                        column: x => x.station_id,
                        principalTable: "Station",
                        principalColumn: "station_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StationStaffOverride_User_user_id",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupportTicket",
                columns: table => new
                {
                    ticket_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    station_id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    priority = table.Column<int>(type: "int", nullable: false),
                    subject = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTicket", x => x.ticket_id);
                    table.ForeignKey(
                        name: "FK_SupportTicket_Station_station_id",
                        column: x => x.station_id,
                        principalTable: "Station",
                        principalColumn: "station_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupportTicket_User_user_id",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPayment",
                columns: table => new
                {
                    sub_pay_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    subscription_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    order_code = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    amount = table.Column<double>(type: "float", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    payment_method = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPayment", x => x.sub_pay_id);
                    table.ForeignKey(
                        name: "FK_SubscriptionPayment_Subscription_subscription_id",
                        column: x => x.subscription_id,
                        principalTable: "Subscription",
                        principalColumn: "subscription_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservation",
                columns: table => new
                {
                    reservation_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    station_inventory_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    reserved_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    expired_at = table.Column<DateTime>(type: "datetime2", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Battery",
                columns: table => new
                {
                    battery_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    station_id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    battery_type_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    reservation_id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    vehicle_id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    serial_no = table.Column<int>(type: "int", maxLength: 255, nullable: false),
                    owner = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    voltage = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    capacity_wh = table.Column<int>(type: "int", nullable: false),
                    current_capacity_wh = table.Column<int>(type: "int", nullable: false),
                    image_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Battery", x => x.battery_id);
                    table.ForeignKey(
                        name: "FK_Battery_BatteryType_battery_type_id",
                        column: x => x.battery_type_id,
                        principalTable: "BatteryType",
                        principalColumn: "battery_type_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Battery_Reservation_reservation_id",
                        column: x => x.reservation_id,
                        principalTable: "Reservation",
                        principalColumn: "reservation_id");
                    table.ForeignKey(
                        name: "FK_Battery_Station_station_id",
                        column: x => x.station_id,
                        principalTable: "Station",
                        principalColumn: "station_id");
                    table.ForeignKey(
                        name: "FK_Battery_User_user_id",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StationBatterySlot",
                columns: table => new
                {
                    station_slot_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    battery_id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    station_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    slot_no = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    last_updated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StationBatterySlot", x => x.station_slot_id);
                    table.ForeignKey(
                        name: "FK_StationBatterySlot_Battery_battery_id",
                        column: x => x.battery_id,
                        principalTable: "Battery",
                        principalColumn: "battery_id");
                    table.ForeignKey(
                        name: "FK_StationBatterySlot_Station_station_id",
                        column: x => x.station_id,
                        principalTable: "Station",
                        principalColumn: "station_id");
                });

            migrationBuilder.CreateTable(
                name: "Vehicle",
                columns: table => new
                {
                    vehicles_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    battery_type_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    v_brand = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    model = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    battery_capacity = table.Column<int>(type: "int", nullable: false),
                    license_plate = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BatteryId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicle", x => x.vehicles_id);
                    table.ForeignKey(
                        name: "FK_Vehicle_BatteryType_battery_type_id",
                        column: x => x.battery_type_id,
                        principalTable: "BatteryType",
                        principalColumn: "battery_type_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vehicle_Battery_BatteryId",
                        column: x => x.BatteryId,
                        principalTable: "Battery",
                        principalColumn: "battery_id");
                    table.ForeignKey(
                        name: "FK_Vehicle_User_user_id",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Booking",
                columns: table => new
                {
                    booking_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    station_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    vehicle_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BatteryId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    BatteryTypeId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Booking", x => x.booking_id);
                    table.ForeignKey(
                        name: "FK_Booking_BatteryType_BatteryTypeId",
                        column: x => x.BatteryTypeId,
                        principalTable: "BatteryType",
                        principalColumn: "battery_type_id");
                    table.ForeignKey(
                        name: "FK_Booking_Battery_BatteryId",
                        column: x => x.BatteryId,
                        principalTable: "Battery",
                        principalColumn: "battery_id");
                    table.ForeignKey(
                        name: "FK_Booking_Station_station_id",
                        column: x => x.station_id,
                        principalTable: "Station",
                        principalColumn: "station_id");
                    table.ForeignKey(
                        name: "FK_Booking_User_user_id",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_Booking_Vehicle_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "Vehicle",
                        principalColumn: "vehicles_id");
                });

            migrationBuilder.CreateTable(
                name: "BatteryBookingSlot",
                columns: table => new
                {
                    booking_slot_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    booking_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    battery_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    station_slot_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatteryBookingSlot", x => x.booking_slot_id);
                    table.ForeignKey(
                        name: "FK_BatteryBookingSlot_Battery_battery_id",
                        column: x => x.battery_id,
                        principalTable: "Battery",
                        principalColumn: "battery_id");
                    table.ForeignKey(
                        name: "FK_BatteryBookingSlot_Booking_booking_id",
                        column: x => x.booking_id,
                        principalTable: "Booking",
                        principalColumn: "booking_id");
                    table.ForeignKey(
                        name: "FK_BatteryBookingSlot_StationBatterySlot_station_slot_id",
                        column: x => x.station_slot_id,
                        principalTable: "StationBatterySlot",
                        principalColumn: "station_slot_id");
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                columns: table => new
                {
                    pay_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    order_code = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    amount = table.Column<double>(type: "float", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    booking_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    payment_method = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentPayId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.pay_id);
                    table.ForeignKey(
                        name: "FK_Payment_Booking_booking_id",
                        column: x => x.booking_id,
                        principalTable: "Booking",
                        principalColumn: "booking_id");
                    table.ForeignKey(
                        name: "FK_Payment_Payment_PaymentPayId",
                        column: x => x.PaymentPayId,
                        principalTable: "Payment",
                        principalColumn: "pay_id");
                    table.ForeignKey(
                        name: "FK_Payment_User_user_id",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "BatterySwap",
                columns: table => new
                {
                    swap_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    vehicle_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    station_staff_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    station_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    battery_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    to_battery_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    payment_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    swapped_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatterySwap", x => x.swap_id);
                    table.ForeignKey(
                        name: "FK_BatterySwap_Battery_battery_id",
                        column: x => x.battery_id,
                        principalTable: "Battery",
                        principalColumn: "battery_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BatterySwap_Battery_to_battery_id",
                        column: x => x.to_battery_id,
                        principalTable: "Battery",
                        principalColumn: "battery_id");
                    table.ForeignKey(
                        name: "FK_BatterySwap_Payment_payment_id",
                        column: x => x.payment_id,
                        principalTable: "Payment",
                        principalColumn: "pay_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BatterySwap_StationStaff_station_staff_id",
                        column: x => x.station_staff_id,
                        principalTable: "StationStaff",
                        principalColumn: "station_staff_id");
                    table.ForeignKey(
                        name: "FK_BatterySwap_Station_station_id",
                        column: x => x.station_id,
                        principalTable: "Station",
                        principalColumn: "station_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BatterySwap_User_user_id",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_BatterySwap_Vehicle_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "Vehicle",
                        principalColumn: "vehicles_id");
                });

            migrationBuilder.CreateTable(
                name: "Review",
                columns: table => new
                {
                    review_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    station_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    swap_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    rating = table.Column<int>(type: "int", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Review", x => x.review_id);
                    table.ForeignKey(
                        name: "FK_Review_BatterySwap_swap_id",
                        column: x => x.swap_id,
                        principalTable: "BatterySwap",
                        principalColumn: "swap_id");
                    table.ForeignKey(
                        name: "FK_Review_Station_station_id",
                        column: x => x.station_id,
                        principalTable: "Station",
                        principalColumn: "station_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Review_User_user_id",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.InsertData(
                table: "BatteryType",
                columns: new[] { "battery_type_id", "battery_type_name" },
                values: new object[,]
                {
                    { "type-001", "Standard Li-ion" },
                    { "type-002", "High Capacity Li-ion" },
                    { "type-003", "Fast Charge Li-ion" }
                });

            migrationBuilder.InsertData(
                table: "SubscriptionPlan",
                columns: new[] { "plan_id", "active", "created_at", "description", "monthly_fee", "name", "swap_amount", "swaps_included" },
                values: new object[,]
                {
                    { "plan-001", true, new DateTime(2025, 10, 19, 0, 17, 35, 659, DateTimeKind.Utc).AddTicks(3660), "Basic battery swap plan", 199000.0, "Basic Plan", 0, "10" },
                    { "plan-002", true, new DateTime(2025, 10, 19, 0, 17, 35, 659, DateTimeKind.Utc).AddTicks(3689), "Premium battery swap plan", 399000.0, "Premium Plan", 0, "25" }
                });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "user_id", "avatar_url", "created_at", "email", "full_name", "password", "phone", "role", "status" },
                values: new object[] { "admin-001", null, new DateTime(2025, 10, 19, 0, 17, 35, 659, DateTimeKind.Utc).AddTicks(2873), "admin@evdriver.com", "System Administrator", "$2a$11$iYvEciubrhVVfct4jVr07elpcB13hcBUZVNMQIlnokW0f/iidYNmq", "0123456789", 3, 1 });

            migrationBuilder.CreateIndex(
                name: "IX_Battery_battery_type_id",
                table: "Battery",
                column: "battery_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_Battery_reservation_id",
                table: "Battery",
                column: "reservation_id");

            migrationBuilder.CreateIndex(
                name: "IX_Battery_serial_no",
                table: "Battery",
                column: "serial_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Battery_station_id",
                table: "Battery",
                column: "station_id");

            migrationBuilder.CreateIndex(
                name: "IX_Battery_user_id",
                table: "Battery",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Battery_vehicle_id",
                table: "Battery",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "IX_BatteryBookingSlot_battery_id",
                table: "BatteryBookingSlot",
                column: "battery_id");

            migrationBuilder.CreateIndex(
                name: "IX_BatteryBookingSlot_booking_id",
                table: "BatteryBookingSlot",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_BatteryBookingSlot_station_slot_id",
                table: "BatteryBookingSlot",
                column: "station_slot_id");

            migrationBuilder.CreateIndex(
                name: "IX_BatterySwap_battery_id",
                table: "BatterySwap",
                column: "battery_id");

            migrationBuilder.CreateIndex(
                name: "IX_BatterySwap_payment_id",
                table: "BatterySwap",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "IX_BatterySwap_station_id",
                table: "BatterySwap",
                column: "station_id");

            migrationBuilder.CreateIndex(
                name: "IX_BatterySwap_station_staff_id",
                table: "BatterySwap",
                column: "station_staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_BatterySwap_to_battery_id",
                table: "BatterySwap",
                column: "to_battery_id");

            migrationBuilder.CreateIndex(
                name: "IX_BatterySwap_user_id",
                table: "BatterySwap",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_BatterySwap_vehicle_id",
                table: "BatterySwap",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_BatteryId",
                table: "Booking",
                column: "BatteryId");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_BatteryTypeId",
                table: "Booking",
                column: "BatteryTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_station_id",
                table: "Booking",
                column: "station_id");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_user_id",
                table: "Booking",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_vehicle_id",
                table: "Booking",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetToken_email_created_at",
                table: "PasswordResetToken",
                columns: new[] { "email", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetToken_user_id",
                table: "PasswordResetToken",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_booking_id",
                table: "Payment",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_PaymentPayId",
                table: "Payment",
                column: "PaymentPayId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_user_id",
                table: "Payment",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_station_inventory_id",
                table: "Reservation",
                column: "station_inventory_id");

            migrationBuilder.CreateIndex(
                name: "IX_Review_station_id",
                table: "Review",
                column: "station_id");

            migrationBuilder.CreateIndex(
                name: "IX_Review_swap_id",
                table: "Review",
                column: "swap_id");

            migrationBuilder.CreateIndex(
                name: "IX_Review_user_id",
                table: "Review",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_StaffAbsense_date",
                table: "StaffAbsense",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "IX_StaffAbsense_user_id_date",
                table: "StaffAbsense",
                columns: new[] { "user_id", "date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Station_user_id",
                table: "Station",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_StationBatterySlot_battery_id",
                table: "StationBatterySlot",
                column: "battery_id",
                unique: true,
                filter: "[battery_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StationBatterySlot_station_id",
                table: "StationBatterySlot",
                column: "station_id");

            migrationBuilder.CreateIndex(
                name: "IX_StationInventory_station_id",
                table: "StationInventory",
                column: "station_id");

            migrationBuilder.CreateIndex(
                name: "IX_StationStaff_station_id",
                table: "StationStaff",
                column: "station_id");

            migrationBuilder.CreateIndex(
                name: "IX_StationStaff_user_id",
                table: "StationStaff",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_StationStaffOverride_station_id_date",
                table: "StationStaffOverride",
                columns: new[] { "station_id", "date" });

            migrationBuilder.CreateIndex(
                name: "IX_StationStaffOverride_user_id_date",
                table: "StationStaffOverride",
                columns: new[] { "user_id", "date" },
                unique: true,
                filter: "[shift_id] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StationStaffOverride_user_id_date_shift_id",
                table: "StationStaffOverride",
                columns: new[] { "user_id", "date", "shift_id" },
                unique: true,
                filter: "[shift_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Subscription_plan_id",
                table: "Subscription",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_Subscription_user_id",
                table: "Subscription",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayment_subscription_id",
                table: "SubscriptionPayment",
                column: "subscription_id");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicket_station_id",
                table: "SupportTicket",
                column: "station_id");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicket_user_id",
                table: "SupportTicket",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_User_email",
                table: "User",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_battery_type_id",
                table: "Vehicle",
                column: "battery_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_BatteryId",
                table: "Vehicle",
                column: "BatteryId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_license_plate",
                table: "Vehicle",
                column: "license_plate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_user_id",
                table: "Vehicle",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Battery_Vehicle_vehicle_id",
                table: "Battery",
                column: "vehicle_id",
                principalTable: "Vehicle",
                principalColumn: "vehicles_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Battery_BatteryType_battery_type_id",
                table: "Battery");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicle_BatteryType_battery_type_id",
                table: "Vehicle");

            migrationBuilder.DropForeignKey(
                name: "FK_Battery_Reservation_reservation_id",
                table: "Battery");

            migrationBuilder.DropForeignKey(
                name: "FK_Battery_Station_station_id",
                table: "Battery");

            migrationBuilder.DropForeignKey(
                name: "FK_Battery_User_user_id",
                table: "Battery");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicle_User_user_id",
                table: "Vehicle");

            migrationBuilder.DropForeignKey(
                name: "FK_Battery_Vehicle_vehicle_id",
                table: "Battery");

            migrationBuilder.DropTable(
                name: "BatteryBookingSlot");

            migrationBuilder.DropTable(
                name: "PasswordResetToken");

            migrationBuilder.DropTable(
                name: "Review");

            migrationBuilder.DropTable(
                name: "StaffAbsense");

            migrationBuilder.DropTable(
                name: "StationStaffOverride");

            migrationBuilder.DropTable(
                name: "SubscriptionPayment");

            migrationBuilder.DropTable(
                name: "SupportTicket");

            migrationBuilder.DropTable(
                name: "StationBatterySlot");

            migrationBuilder.DropTable(
                name: "BatterySwap");

            migrationBuilder.DropTable(
                name: "Subscription");

            migrationBuilder.DropTable(
                name: "Payment");

            migrationBuilder.DropTable(
                name: "StationStaff");

            migrationBuilder.DropTable(
                name: "SubscriptionPlan");

            migrationBuilder.DropTable(
                name: "Booking");

            migrationBuilder.DropTable(
                name: "BatteryType");

            migrationBuilder.DropTable(
                name: "Reservation");

            migrationBuilder.DropTable(
                name: "StationInventory");

            migrationBuilder.DropTable(
                name: "Station");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Vehicle");

            migrationBuilder.DropTable(
                name: "Battery");
        }
    }
}
