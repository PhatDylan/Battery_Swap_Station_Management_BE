using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Entities
{
    [Table("StationInventory")]
    public class StationInventory
    {
        [Key]
        [Column("station_inventory_id")]
        public string StationInventoryId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("station_id")]
        public string StationId { get; set; } = string.Empty;

        [Column("maintenance_count")]
        public string MaintenanceCount { get; set; }

        [Column("full_count")]
        public string FullCount { get; set; }

        [Column("charging_count")]
        public string ChargingCount { get; set; }

        [Column("last_update")]
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;

        // Foreign key navigation properties
        public virtual Station Station { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
