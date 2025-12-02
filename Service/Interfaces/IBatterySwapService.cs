// Service/Interfaces/IBatterySwapService.cs
using BusinessObject.DTOs;

namespace Service.Interfaces
{
    public interface IBatterySwapService
    {
        Task<BatterySwapResponse> CreateBatterySwapFromBookingAsync(CreateBatterySwapRequest request);
        Task<BatterySwapResponse> UpdateBatterySwapStatusAsync(string swapId, UpdateBatterySwapStatusRequest request);
        Task<BatterySwapDetailResponse> GetBatterySwapDetailAsync(string swapId);
        Task<PaginationWrapper<List<BatterySwapResponse>, BatterySwapResponse>> GetStationBatterySwapsAsync(string stationId, int page, int pageSize, string? search);
        Task<PaginationWrapper<List<BatterySwapResponse>, BatterySwapResponse>> GetMyBatterySwapsAsync(int page, int pageSize, string? search);
        Task<List<BatterySwapResponse>> GetSwapsByBookingAsync(string bookingId);
        Task<PaginationWrapper<List<BatterySwapResponse>, BatterySwapResponse>> GetDriverSwapHistoryAsync(int page, int pageSize, string? search);
    }
}