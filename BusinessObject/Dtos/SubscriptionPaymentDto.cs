using BusinessObject.Enums;

namespace BusinessObject.Dtos
{
    // Request khi driver mua subscription plan
    public class CreateSubscriptionPaymentRequest
    {
        public string PlanId { get; set; } = string.Empty;
        public PayMethod PaymentMethod { get; set; } = PayMethod.Card; // Chỉ cho phép online payment
    }

    public class CreateSubscriptionPaymentResponse
    {
        public string SubPayId { get; set; } = string.Empty;
        public string SubscriptionId { get; set; } = string.Empty;
        public string OrderCode { get; set; } = string.Empty;
        public double Amount { get; set; }
        public PayMethod PaymentMethod { get; set; }
        public PayStatus Status { get; set; }
        public string? PaymentUrl { get; set; } // PayOS payment URL
        public DateTime CreatedAt { get; set; }
    }

    public class SubscriptionPaymentResponse
    {
        public string SubPayId { get; set; } = string.Empty;
        public string SubscriptionId { get; set; } = string.Empty;
        public string OrderCode { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public PayMethod PaymentMethod { get; set; }
        public PayStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        // Additional info
        public string? UserName { get; set; }
        public string? PlanName { get; set; }
    }
}
