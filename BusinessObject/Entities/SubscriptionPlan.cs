using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Entities
{
    [Table("SubscriptionPlan")]
    public class SubscriptionPlan
    {
        [Key]
        [Column("plan_id")]
        public string PlanId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        [StringLength(1000)]
        public string? Description { get; set; }

        [Column("monthly_fee")]
        public double MonthlyFee { get; set; }

        [Column("swaps_included")]
        public string SwapsIncluded { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
