using BusinessObject.DTOs;
using BusinessObject.Enums;

namespace Service.Interfaces
{
    public interface IStaffManagementBatteryService
    {
        Task<PaginationWrapper<List<StaffSwapListItemResponse>, StaffSwapListItemResponse>> GetMyStationSwapsAsync(
            int page, int pageSize, string? stationId, BBRStatus? status, string? search, CancellationToken ct = default);

        Task RejectSwapAsync(string swapId, StaffRejectSwapRequest? request, CancellationToken ct = default);

        Task ConfirmSwapAsync(string swapId, CancellationToken ct = default);
        Task CompleteSwapAsync(string swapId, CancellationToken ct = default);
    }
}