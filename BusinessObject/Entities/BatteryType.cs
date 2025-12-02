using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Entities;

[Table("BatteryType")]
public class BatteryType
{
    [Key] [Column("battery_type_id")] public string BatteryTypeId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [Column("battery_type_name")]
    [StringLength(255)]
    public string BatteryTypeName { get; set; } = string.Empty;

    // Navigation properties.
    public virtual ICollection<Battery> Batteries { get; set; } = new List<Battery>();
    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}