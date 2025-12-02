using BusinessObject.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Entities;

[Table("BatteryBookingSlot")]
public class BatteryBookingSlot
{
    [Key][Column("booking_slot_id")] public string BookingSlotId { get; set; } 

    [Column("booking_id")] public string BookingId { get; set; }

    [Required] [Column("battery_id")] public string BatteryId { get; set; }

    [Required] [Column("station_slot_id")] public string StationSlotId { get; set; }

    [Required] [Column("status")] public SBSStatus Status { get; set; } = SBSStatus.Available;

    [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("BatteryId")] public virtual Battery Battery { get; set; } = null!;

    [ForeignKey("StationSlotId")] public virtual StationBatterySlot StationBatterySlot { get; set; } = null!;

    [ForeignKey("BookingId")] public virtual Booking Booking { get; set; } = null!;
}