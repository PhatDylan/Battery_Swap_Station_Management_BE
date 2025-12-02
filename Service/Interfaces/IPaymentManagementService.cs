using BusinessObject.DTOs;

namespace Service.Interfaces;

public interface IPaymentManagementService
{
  

    Task<PaginationWrapper<List<PaymentManagementResponse>, PaymentManagementResponse>> GetAllPaymentsAsync(
        int page, int pageSize, string? search = null);

    Task<PaginationWrapper<List<StationPaymentResponse>, StationPaymentResponse>> GetPaymentsByStationAsync(
        string stationId, int page, int pageSize, string? search = null);
}