using BusinessObject.Dtos;
using BusinessObject.DTOs;

namespace Service.Interfaces
{
    public interface ISubscriptionService
    {
        Task<PaginationWrapper<List<SubscriptionResponse>, SubscriptionResponse>> GetAllSubscriptionAsync(int page, int pageSize, string? search);
        Task<SubscriptionResponse?> GetBySubscriptionAsync(string id);
        Task<SubscriptionResponse> GetByUserAsync(string userId);
        Task AddAsync(SubscriptionRequest request);
        Task UpdateAsync(SubscriptionRequest request);
        Task DeleteAsync(string id);
        Task<List<SubscriptionResponse>> GetSubscriptionDetailAsync();
        Task ResetExpiredSubscriptionsAsync();
        Task<SubscriptionResponse> CancelSubscription(string subscriptionId);

        Task<PaginationWrapper<List<SubscriptionPurchaseResponse>, SubscriptionPurchaseResponse>> GetAllSubscriptionPurchasesAsync(
    int page, int pageSize, string? search = null);
    }
}