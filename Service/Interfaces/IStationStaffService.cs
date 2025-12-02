// Service/Interfaces/IStationStaffService.cs

using BusinessObject.DTOs;

namespace Service.Interfaces
{
    public interface IStationStaffService
    {
        Task<StationStaffResponse> AssignStaffToStationAsync(AssignStaffRequest request);
        Task<StationStaffResponse> RemoveStaffFromStationAsync(string stationStaffId);
        Task<PaginationWrapper<List<StationStaffResponse>, StationStaffResponse>> GetStationStaffsAsync(string stationId, int page, int pageSize, string? search);
        Task<PaginationWrapper<List<StaffStationResponse>, StaffStationResponse>> GetStaffStationsAsync(string staffUserId, int page, int pageSize);
        Task<StationStaffResponse> GetStationStaffAsync(string stationStaffId);
        Task<List<StationStaffResponse>> GetAllStaffsByStationAsync(string stationId);
        Task<StationStaffBaseResponse> GetStationStaffBaseAsync(string userId);
    }
}
