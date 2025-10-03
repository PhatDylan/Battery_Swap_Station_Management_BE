using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Enums;

namespace BusinessObject.Entities
{
    [Table("Booking")]
    public class Booking
    {
        [Key]
        [Column("booking_id")]
        public string BookingId { get; set; } = Guid.NewGuid().ToString();

        [Key]
        [Column("station_id")]
        public string StationId { get; set; } = string.Empty;

        [Key]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Key]
        [Column("vehicle_id")]
        public string VehicleId { get; set; } = string.Empty;

        [Key]
        [Column("battery_id")]
        public string BatteryId { get; set; } = string.Empty;

        [Key]
        [Column("battery_type_id")]
        public string BatteryTypeId { get; set; } = string.Empty;

        [Required]
        [Column("booking_date")]
        public DateTime BookingDate { get; set; }

        [Required]
        [Column("time_slot")]
        public TimeSpan TimeSlot { get; set; }

        [Required]
        [Column("status")]
        public BBRStatus Status { get; set; } = BBRStatus.Pending;

        [Column("confirm_by")]
        public string? ConfirmBy { get; set; } = string.Empty;

        [Column("complete_at")]
        public DateTime? CompleteAt { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Foreign key navigation properties
        public virtual Station Station { get; set; } = null!;

        public virtual User User { get; set; } = null!;

        public virtual Vehicle Vehicle { get; set; } = null!;

        public virtual Battery Battery { get; set; } = null!;

        public virtual BatteryType BatteryType { get; set; } = null!;
    }
}
