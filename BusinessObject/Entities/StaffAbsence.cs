using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Entities;

[Table("StaffAbsence")]
public class StaffAbsence
{
    [Key]
    [Column("staff_absence_id")]
    public string StaffAbsenceId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [Column("date")]
    public DateTime Date { get; set; }

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
}