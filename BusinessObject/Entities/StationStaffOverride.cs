using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Entities;

[Table("StationStaffOverride")]
public class StationStaffOverride
{
    [Key]
    [Column("station_staff_override_id")]
    public string StationStaffOverrideId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [Column("station_id")]
    public string StationId { get; set; } = string.Empty;

    
    [Required]
    [Column("date")]
    public DateTime Date { get; set; }

    //neu muon them ca
    [Column("shift_id")]
    public string? ShiftId { get; set; }

    [Column("reason")]
    [StringLength(500)]
    public string? Reason { get; set; }

    [Required]
    [Column("created_by")]
    public string CreatedBy { get; set; } = string.Empty;

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("StationId")]
    public virtual Station Station { get; set; } = null!;
}