using BusinessObject.DTOs;

namespace Service.Interfaces;

public interface IStaffInventoryBatteryService
{
    Task<BatteryInventorySummaryResponse> GetSummaryAsync(BatteryInventorySummaryRequest request);

    Task<PaginationWrapper<List<BatteryInventoryItemResponse>, BatteryInventoryItemResponse>> SearchAsync(
        BatteryInventorySearchRequest request);
}