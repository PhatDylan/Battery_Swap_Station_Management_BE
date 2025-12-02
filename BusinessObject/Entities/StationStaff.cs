using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Entities;

[Table("StationStaff")]
public class StationStaff
{
    [Key] [Column("station_staff_id")] public string StationStaffId { get; set; } = Guid.NewGuid().ToString();

    [Required] [Column("user_id")] public string UserId { get; set; } = string.Empty;

    [Required] [Column("station_id")] public string StationId { get; set; } = string.Empty;

    [Column("assigned_at")] public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Foreign key navigation properties
    [ForeignKey("UserId")] public virtual User User { get; set; } = null!;

    [ForeignKey("StationId")] public virtual Station Station { get; set; } = null!;

    // Navigation properties
    public virtual ICollection<BatterySwap> BatterySwaps { get; set; } = new List<BatterySwap>();
}