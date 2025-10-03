using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Enums;

namespace BusinessObject.Entities
{
    [Table("Payment")]
    public class Payment
    {
        [Key]
        [Column("pay_id")]
        public string PayId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Column("swap_id")]
        public string SwapId { get; set; } = string.Empty;

        [Required]
        [Column("order_code")]
        [StringLength(255)]
        public string OrderCode { get; set; } = string.Empty;

        [Column("amount")]
        public double Amount { get; set; }

        [Column("currency")]
        [StringLength(10)]
        public string Currency { get; set; } = "VND";

        [Column("payment_method")]
        public PayMethod PaymentMethod { get; set; } = PayMethod.Card;

        [Column("status")]
        public PayStatus Status { get; set; } = PayStatus.Pending;  

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key navigation properties
        public virtual User User { get; set; } = null!;

        public virtual BatterySwap BatterySwap { get; set; } = null!;
    }
}