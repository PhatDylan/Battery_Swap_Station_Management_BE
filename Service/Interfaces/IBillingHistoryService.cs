using BusinessObject.DTOs;

namespace Service.Interfaces;

public interface IBillingHistoryService
{
    // Payments
    Task<MyPaymentsListResponse> GetMyPaymentsAsync(MyPaymentsListRequest request);
    Task<PaymentResponse?> GetMyPaymentByIdAsync(string payId);

    // Transactions
    Task<TransactionHistoryListResponse> GetMyTransactionHistoryAsync(TransactionHistoryQueryRequest request);
}