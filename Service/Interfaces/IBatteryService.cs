using BusinessObject.Dtos;
using BusinessObject.DTOs;

namespace Service.Interfaces
{
    public interface IBatteryService
    {
        Task<BatteryResponse> GetByBatteryAsync(string id);
        Task<BatteryResponse> GetBySerialAsync(int serialNo);

        Task<PaginationWrapper<List<BatteryResponse>, BatteryResponse>> GetByStationAsync(string stationId, int page,
            int pageSize, string? search);
        Task<BatteryResponse> GetAvailableAsync(string? stationId = null);
        Task AddAsync(BatteryRequest battery);
        Task AddBatteryToStation(IEnumerable<BatteryAddBulkStationRequest> requests);
        Task UpdateAsync(BatteryRequest battery);
        Task DeleteAsync(string id);
        Task<PaginationWrapper<List<BatteryResponse>, BatteryResponse>> GetAllBatteriesAsync(int page,
        int pageSize, string? search);

        Task<PaginationWrapper<List<BatteryResponse>, BatteryResponse>> GetAllBatteryInStorage(string stationId,
            int page, int pageSize, string? search);
        // Lấy tất cả battery không có vehicle chủ và không nằm trong station nào
        Task<List<BatteryResponse>> GetUnassignedAndNotInStationAsync(string? batteryTypeId);

        // Gắn battery vào vehicle (attach battery -> vehicle)
        Task AttachBatteryToVehicleAsync(BatteryAttachRequest request);

        Task<PaginationWrapper<List<BatteryResponse>, BatteryResponse>> GetBatteryAssignedByStationIdAsync(
            string stationId, int page, int pageSize, string? search);
    }
}
