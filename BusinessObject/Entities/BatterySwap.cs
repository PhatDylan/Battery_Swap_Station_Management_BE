using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Enums;

namespace BusinessObject.Entities;

[Table("BatterySwap")]
public class BatterySwap
{
    [Key] [Column("swap_id")] public string SwapId { get; set; } = Guid.NewGuid().ToString();

    [Required] [Column("vehicle_id")] public string VehicleId { get; set; } = string.Empty;
    
    [Column("station_staff_id")]
    public string? StationStaffId { get; set; } = string.Empty;

    [Required] [Column("user_id")] public string UserId { get; set; } = string.Empty;

    [Required] [Column("station_id")] public string StationId { get; set; } = string.Empty;

    [Required] [Column("battery_id")] public string BatteryId { get; set; } = string.Empty;

    [Required] [Column("to_battery_id")] public string ToBatteryId { get; set; } = string.Empty;
    
    [Column("reason")] public string? Reason { get; set; } = string.Empty;
    
    [Required] [Column("payment_id")] public string PaymentId { get; set; }

    [Column("status")] public BBRStatus Status { get; set; } = BBRStatus.Pending;

    [Column("swapped_at")] public DateTime SwappedAt { get; set; } = DateTime.UtcNow;

    [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign key navigation properties
    [ForeignKey("VehicleId")] public virtual Vehicle Vehicle { get; set; } = null!;

    [ForeignKey("StationStaffId")] public virtual StationStaff? StationStaff { get; set; } = null!;

    [ForeignKey("UserId")] public virtual User User { get; set; } = null!;

    [ForeignKey("StationId")] public virtual Station Station { get; set; } = null!;

    [ForeignKey("BatteryId")] public virtual Battery Battery { get; set; } = null!;

    [ForeignKey("ToBatteryId")] public virtual Battery ToBattery { get; set; } = null!;
    
    [ForeignKey("PaymentId")] public virtual Payment Payment { get; set; } = null!;
    
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}