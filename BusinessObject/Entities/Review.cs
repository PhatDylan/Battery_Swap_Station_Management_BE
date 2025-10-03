using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Entities
{
    [Table("Review")]
    public class Review
    {
        [Key]
        [Column("review_id")]
        public string ReviewId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Column("station_id")]
        public string StationId { get; set; } = string.Empty;

        [Column("rating")]
        public int Rating { get; set; } // 1-5 stars

        [Column("comment")]
        [StringLength(1000)]
        public string? Comment { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key navigation properties
        public virtual User User { get; set; } = null!;

        public virtual Station Station { get; set; } = null!;
    }
}