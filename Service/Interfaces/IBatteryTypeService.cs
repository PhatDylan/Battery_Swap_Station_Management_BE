using BusinessObject.Dtos;
using BusinessObject.DTOs;

namespace Service.Interfaces
{
    public interface IBatteryTypeService
    {
        Task<BatteryTypeResponse?> GetByIdAsync(string id);
        Task AddAsync(BatteryTypeRequest request);
        Task UpdateAsync(BatteryTypeRequest request);
        Task DeleteAsync(string id);
        Task<PaginationWrapper<List<BatteryTypeResponse>, BatteryTypeResponse>> GetAllBatteryTypeAsync(int page, int pageSize, string? search);
    }
}
