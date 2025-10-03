using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Enums;

namespace BusinessObject.Entities
{
    [Table("Subscription")]
    public class Subscription
    {
        [Key]
        [Column("subscription_id")]
        public string SubscriptionId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Column("plan_id")]
        public string PlanId { get; set; } = string.Empty;

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Column("status")]
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

        [Column("number_of_swaps")]
        public int NumberOfSwaps { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key navigation properties
        public virtual User User { get; set; } = null!;

        public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<SubscriptionPayment> SubscriptionPayments { get; set; } = new List<SubscriptionPayment>();
    }
}
