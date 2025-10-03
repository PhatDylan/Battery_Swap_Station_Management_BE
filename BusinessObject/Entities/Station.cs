using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Entities
{
    [Table("Station")]
    public class Station
    {
        [Key]
        [Column("station_id")]
        public string StationId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("address")]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [Column("latitude")]
        public double Latitude { get; set; }

        [Required]
        [Column("longitude")]
        public double Longitude { get; set; }

        [Column("maximum_slot")]
        public int MaximumSlot { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Foreign key navigation property
        public virtual User User { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<Battery> Batteries { get; set; } = new List<Battery>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<StationStaff> StationStaffs { get; set; } = new List<StationStaff>();
        public virtual ICollection<BatterySwap> BatterySwaps { get; set; } = new List<BatterySwap>();
        public virtual ICollection<StationInventory> StationInventories { get; set; } = new List<StationInventory>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
    }
}
