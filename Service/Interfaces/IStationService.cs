using BusinessObject.Dtos;
using BusinessObject.DTOs;

namespace Service.Interfaces
{
    public interface IStationService
    {
        Task<StationResponse> GetStationAsync(string stationId);
        Task<StationResponse> CreateStationAsync(StationRequest stationRequest);
        Task<StationResponse> UpdateStationAsync(string stationId, StationRequest stationRequest);
        Task<StationResponse> DeleteStationAsync(string stationId);
        Task<PaginationWrapper<List<StationResponse>, StationResponse>> GetAllStationsAsync(int page, int pageSize, string? search);
        Task<List<StationAvailabilityDto>> GetStationsWithBatteryTypeAvailableAsync(string batteryTypeId, bool excludeFullStations = true);
    }
}