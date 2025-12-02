using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Enums;
namespace BusinessObject.Entities;

[Table("StationBatterySlot")]
public class StationBatterySlot
{
    [Key] [Column("station_slot_id")] public string StationSlotId { get; set; }

    [Column("battery_id")] public string? BatteryId { get; set; }

    [Required] [Column("station_id")] public string StationId { get; set; }

    [Required] [Column("slot_no")] public int SlotNo { get; set; }

    [Required][Column("status")] public SBSStatus Status { get; set; } = SBSStatus.Available;

    [Column("last_updated")] public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    [ForeignKey("BatteryId")] public virtual Battery? Battery { get; set; } = null!;

    [ForeignKey("StationId")] public virtual Station Station { get; set; } = null!;

    public virtual ICollection<BatteryBookingSlot> BatteryBookingSlots { get; set; } = new List<BatteryBookingSlot>();
}