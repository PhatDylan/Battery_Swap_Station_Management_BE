using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Enums;

namespace BusinessObject.Entities;

[Table("SupportTicket")]
public class SupportTicket
{
    [Key] [Column("ticket_id")] public string TicketId { get; set; } = Guid.NewGuid().ToString();

    [Required] [Column("user_id")] public string UserId { get; set; } = string.Empty;

    [Column("station_id")] public string? StationId { get; set; }

    [Column("priority")] public Priority Priority { get; set; } = Priority.Low;

    [Required]
    [Column("subject")]
    [StringLength(255)]
    public string Subject { get; set; } = string.Empty;

    [Column("message")]
    [StringLength(2000)]
    public string? Message { get; set; }

    [Column("status")] public SupportTicketStatus Status { get; set; } = SupportTicketStatus.Open;

    [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")] public DateTime? UpdatedAt { get; set; }

    // Foreign key navigation properties
    [ForeignKey("UserId")] public virtual User User { get; set; } = null!;

    [ForeignKey("StationId")] public virtual Station? Station { get; set; }
}