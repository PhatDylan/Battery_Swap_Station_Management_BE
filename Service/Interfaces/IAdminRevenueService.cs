using BusinessObject.Dtos;

namespace Service.Interfaces
{
    public interface IAdminRevenueService
    {
        Task<RevenueReportDto> GetRevenueReportAsync(CancellationToken cancellationToken = default);
    }
}