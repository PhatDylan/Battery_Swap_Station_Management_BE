using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Entities;

[Table("Vehicle")]
public class Vehicle
{
    [Key] [Column("vehicles_id")] public string VehicleId { get; set; } = Guid.NewGuid().ToString();

    [Required] [Column("user_id")] public string UserId { get; set; } = string.Empty;

    [Required] [Column("battery_type_id")] public string BatteryTypeId { get; set; } = string.Empty;

    [Required]
    [Column("v_brand")]
    [StringLength(255)]
    public string VBrand { get; set; } = string.Empty;

    [Required]
    [Column("model")]
    [StringLength(255)]
    public string Model { get; set; } = string.Empty;

    [Required]
    [Column("battery_capacity")]
    public int BatteryCapacity { get; set; } = 1;

    [Required]
    [Column("license_plate")]
    [StringLength(50)]
    public string LicensePlate { get; set; } = string.Empty;

    [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [ForeignKey("UserId")] public virtual User User { get; set; } = null!;

    [ForeignKey("BatteryTypeId")] public virtual BatteryType BatteryType { get; set; } = null!;

    public virtual ICollection<Battery> Batteries { get; set; } = new List<Battery>();
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<BatterySwap> BatterySwaps { get; set; } = new List<BatterySwap>();
}