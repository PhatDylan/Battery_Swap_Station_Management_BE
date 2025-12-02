using BusinessObject.Dtos;
using BusinessObject.DTOs;

namespace Service.Interfaces
{
    public interface ISubscriptionPlanService
    {
        Task<SubscriptionPlanResponse> GetAllAsync();
        Task<SubscriptionPlanResponse?> GetByIdAsync(string id);
        Task AddAsync(SubscriptionPlanRequest request);
        Task UpdateAsync(SubscriptionPlanRequest request);
        Task DeleteAsync(string id);
        Task<PaginationWrapper<List<SubscriptionPlanResponse>, SubscriptionPlanResponse>> GetAllSubscriptionPlanAsync(int page, int pageSize, string? search);
    }
}