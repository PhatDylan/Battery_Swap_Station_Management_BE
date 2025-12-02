using BusinessObject.Enums;

namespace BusinessObject.Dtos
{
    public class SubscriptionRequest
    {
        public string? SubscriptionId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public SubscriptionStatus Status { get; set; }
        public int NumberOfSwap { get; set; }
    }

    public class SubscriptionResponse
    {
        public string SubscriptionId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public SubscriptionStatus Status { get; set; }
        public int NumberOfSwap { get; set; }
        public DateTime CreatedAt { get; set; }

        // Additional fields for detailed information
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? PlanName { get; set; }
        public string? PlanDescription { get; set; }
        public decimal? MonthlyFee { get; set; }
        public bool IsExpired => DateTime.UtcNow > EndDate;
        public int DaysRemaining => Status == SubscriptionStatus.Active ?
            Math.Max(0, (EndDate - DateTime.UtcNow).Days) : 0;
    }
}