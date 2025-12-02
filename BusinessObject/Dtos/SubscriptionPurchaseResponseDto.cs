namespace BusinessObject.DTOs;

public class SubscriptionPurchaseResponse
{
    // Subscription Info
    public string SubscriptionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public string? UserAvatarUrl { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string PlanId { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int NumberOfSwaps { get; set; }
    public DateTime CreatedAt { get; set; }

    // SubscriptionPlan Info
    public SubscriptionPlanDto PlanInfo { get; set; } = new();

    // SubscriptionPayment Info (List)
    public List<SubscriptionPaymentDto> Payments { get; set; } = new();
}

public class SubscriptionPlanDto
{
    public string PlanId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double MonthlyFee { get; set; }
    public string SwapsIncluded { get; set; } = string.Empty;
    public bool Active { get; set; }
    public int SwapAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SubscriptionPaymentDto
{
    public string SubPayId { get; set; } = string.Empty;
    public string SubscriptionId { get; set; } = string.Empty;
    public string OrderCode { get; set; } = string.Empty;
    public double Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}