using BusinessObject.Dtos;

namespace Service.Interfaces
{
    public interface ISubscriptionPaymentService
    {
        // Driver mua subscription plan
        Task<CreateSubscriptionPaymentResponse> CreateSubscriptionPaymentAsync(CreateSubscriptionPaymentRequest request);

        // Get payment detail
        Task<SubscriptionPaymentResponse> GetSubscriptionPaymentDetailAsync(string subPayId);

        // Get payment history by user
        Task<List<SubscriptionPaymentResponse>> GetMySubscriptionPaymentsAsync();
    }
}
