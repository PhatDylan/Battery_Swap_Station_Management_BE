namespace BusinessObject.DTOs;

public class PaymentManagementResponse
{
    public string PayId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public double Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string BookingId { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class StationPaymentResponse
{
    public string PayId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public double Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string BookingId { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string StationId { get; set; } = string.Empty;
    public string SwapId { get; set; } = string.Empty;
}