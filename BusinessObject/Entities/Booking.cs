using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Enums;

namespace BusinessObject.Entities;

[Table("Booking")]
public class Booking
{
    [Key] [Column("booking_id")] public string BookingId { get; set; } = Guid.NewGuid().ToString();

    [Required] [Column("station_id")] public string StationId { get; set; } = string.Empty;

    [Required] [Column("user_id")] public string UserId { get; set; } = string.Empty;

    [Required] [Column("vehicle_id")] public string VehicleId { get; set; } = string.Empty;

    [Required] [Column("status")] public BBRStatus Status { get; set; } = BBRStatus.Pending;
    
    [Required] [Column("booking_time")] public DateTime BookingTime { get; set; }

    [Required] [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("StationId")] public virtual Station Station { get; set; } = null!;

    [ForeignKey("UserId")] public virtual User User { get; set; } = null!;

    [ForeignKey("VehicleId")] public virtual Vehicle Vehicle { get; set; } = null!;
    
    public virtual ICollection<Payment> Payments { get; set; } = null!;

    public virtual ICollection<BatteryBookingSlot> BatteryBookingSlots { get; set; } = new List<BatteryBookingSlot>();
}