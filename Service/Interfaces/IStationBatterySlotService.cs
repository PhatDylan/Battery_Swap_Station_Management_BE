using BusinessObject.Dtos;
using BusinessObject.DTOs;

namespace Service.Interfaces
{
    public interface IStationBatterySlotService
    {
        Task<PaginationWrapper<List<StationBatterySlotResponse>, StationBatterySlotResponse>> GetAllStationSlotAsync(int page, int pageSize, string? search);
        Task<StationBatterySlotResponse?> GetByIdAsync(string id);

        Task<PaginationWrapper<List<StationBatterySlotResponse>, StationBatterySlotResponse>> GetByStationAsync(
            string stationId, int page, int pageSize, string? search);
        Task<PaginationWrapper<List<StationBatterySlotResponse>, StationBatterySlotResponse>> GetValidSlotByStationAsync(string stationId, int page, int pageSize, string? search);
        Task AddAsync(StationBatterySlotRequest request);
        Task UpdateAsync(StationBatterySlotRequest request);
        Task DeleteAsync(string id);
        Task ResetStationBatterySlot();
        Task<List<StationBatterySlotResponse>> GetStationBatterySlotDetailAsync();
    }
}
