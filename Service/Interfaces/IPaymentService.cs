using BusinessObject.DTOs;
using Net.payOS.Types;

namespace Service.Interfaces
{
    public interface IPaymentService
    {

        Task<CreatePaymentResponse> CreatePaymentAsync(CreatePaymentRequest request);
        
        Task<PaymentResponse> GetPaymentDetailAsync(string paymentId);
        
        Task<List<PaymentResponse>> GetPaymentDetailByBookingIdAsync(string bookingId);
        bool ValidatePayOsSignature(WebhookType payload);
        Task ProcessPayOsPaymentAsync(WebhookData data, bool isSuccess);
        Task<PaymentResponse> GetPaymentDetailByDriverAsync(string userId);
        Task<List<PaymentResponse>> GetMyPaymentsAsync();
    }
}
