using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Enums;

namespace BusinessObject.Entities;

[Table("Battery")]
public class Battery
{
    [Key] [Column("battery_id")] public string BatteryId { get; set; } = Guid.NewGuid().ToString();

    [Column("station_id")] public string? StationId { get; set; } = string.Empty;

    [Required] [Column("user_id")] public string UserId { get; set; } = string.Empty;

    [Required] [Column("battery_type_id")] public string BatteryTypeId { get; set; } = string.Empty;

    [Column("vehicle_id")] public string? VehicleId { get; set; }

    [Required]
    [Column("serial_no")]
    [StringLength(255)]
    public int SerialNo { get; set; }

    [Column("owner")] public BatteryOwner Owner { get; set; } = BatteryOwner.Station;

    [Column("status")] public BatteryStatus Status { get; set; } = BatteryStatus.Available;

    [Column("voltage")] [StringLength(50)] public string? Voltage { get; set; }

    [Required, Column("capacity_wh")] public int CapacityWh { get; set; } = 10000;

    [Required, Column("current_capacity_wh")]
    public int CurrentCapacityWh { get; set; } = 10000;

    [Column("image_url")]
    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")] public DateTime? UpdatedAt { get; set; }

    // Foreign key navigation properties
    [ForeignKey("StationId")] public virtual Station? Station { get; set; } = null!;

    [ForeignKey("UserId")] public virtual User User { get; set; } = null!;

    [ForeignKey("BatteryTypeId")] public virtual BatteryType BatteryType { get; set; } = null!;
    

    [ForeignKey("VehicleId")] public virtual Vehicle? Vehicle { get; set; }

    // Navigation properties
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<BatterySwap> BatterySwaps { get; set; } = new List<BatterySwap>();
    public virtual ICollection<BatterySwap> ToBatterySwaps { get; set; } = new List<BatterySwap>();
    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public virtual StationBatterySlot? StationBatterySlot { get; set; } = null!;
    public virtual ICollection<BatteryBookingSlot> BatteryBookingSlots { get; set; } = new List<BatteryBookingSlot>();
}