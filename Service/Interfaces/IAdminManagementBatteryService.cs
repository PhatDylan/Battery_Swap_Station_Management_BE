using BusinessObject.Dtos;

namespace Service.Interfaces
{
    public interface IAdminManagementBatteryService
    {
        Task<CountBatteryAdminDto> GetPeakSwapReportAsync(CancellationToken cancellationToken = default);
    }
}