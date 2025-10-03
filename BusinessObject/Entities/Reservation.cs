using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Enums;

namespace BusinessObject.Entities
{
    [Table("Reservation")]
    public class Reservation
    {
        [Key]
        [Column("reservation_id")]
        public string ReservationId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("station_inventory_id")]
        public string StationInventoryId { get; set; } = string.Empty;

        [Column("status")]
        public BBRStatus Status { get; set; } = BBRStatus.Pending;

        [Column("reserved_at")]
        public DateTime ReservedAt { get; set; } = DateTime.UtcNow;

        [Column("expired_at")]
        public DateTime ExpiredAt { get; set; }

        // Foreign key navigation properties
        public virtual StationInventory StationInventory { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<Battery> Batteries { get; set; } = new List<Battery>();
    }
}
