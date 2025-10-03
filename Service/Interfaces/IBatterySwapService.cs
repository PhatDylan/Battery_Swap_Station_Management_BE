using BusinessObject.DTOs.BatterySwap;

namespace Service.Interfaces
{
    public interface IBatterySwapService
    {
        Task<BatterySwapDetailDto?> CreateSwapFromBookingAsync(string bookingId, string staffId, CreateBatterySwapDto createSwapDto);
        Task<BatterySwapDetailDto?> GetBatterySwapDetailAsync(string swapId);
        Task<List<BatterySwapSummaryDto>> GetSwapsByBookingAsync(string bookingId);
    }
}