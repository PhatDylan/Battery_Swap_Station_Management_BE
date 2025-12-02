namespace BusinessObject.Dtos;

public class RevenueReportDto
{
    public List<RevenueByPeriodDto> ByDay { get; set; } = new();
    public List<RevenueByPeriodDto> ByMonth { get; set; } = new();
    public List<RevenueByPeriodDto> ByYear { get; set; } = new();
}

public class RevenueByPeriodDto
{
    public string Time { get; set; } = string.Empty; // Format: "DD/MM/YYYY", "MM/YYYY", "YYYY"
    public double TotalRevenue { get; set; } // Tổng doanh thu
    public double SwapPaymentRevenue { get; set; } // Doanh thu từ swap trả tiền trực tiếp
    public double SubscriptionRevenue { get; set; } // Doanh thu từ gói subscription
    public int SwapPaymentCount { get; set; } // Số lượng giao dịch swap thanh toán
    public int SubscriptionCount { get; set; } // Số lượng gói đăng ký đã mua
}
