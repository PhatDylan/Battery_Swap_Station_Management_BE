using BusinessObject.Enums;

namespace BusinessObject.DTOs;

public class PaginationMeta
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}

// Payments
public class MyPaymentsListRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public PayStatus? Status { get; set; }
}

public class MyPaymentsListResponse
{
    public List<PaymentResponse> Items { get; set; } = new();
    public PaginationMeta Pagination { get; set; } = new();
}

public class TransactionHistoryItem
{
    public string Id { get; set; } = string.Empty;
    public TransactionKind Kind { get; set; }
    public DateTime OccurredAt { get; set; }
    public double Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public string? ReferenceCode { get; set; } // OrderCode hoặc SwapId
    public string? BookingId { get; set; }
    public string? StationName { get; set; }
    public string? Status { get; set; }
}

public class TransactionHistoryQueryRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public TransactionKind? Kind { get; set; }
}

public class TransactionHistoryListResponse
{
    public List<TransactionHistoryItem> Items { get; set; } = new();
    public PaginationMeta Pagination { get; set; } = new();
}