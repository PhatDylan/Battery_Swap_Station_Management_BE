using BusinessObject.DTOs;
using BusinessObject.DTOs.BatterySwap;

public interface IBatterySwapResponseService
{
    Task<PaginationWrapper<List<CompletedBatterySwapResponseDto>, CompletedBatterySwapResponseDto>>
        GetCompletedSwapsByStationStaffIdAsync(string stationStaffId, int page, int pageSize);

}
