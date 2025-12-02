using BusinessObject.DTOs;
using BusinessObject.Enums;

namespace Service.Interfaces;

public interface IBookingService
{
    Task<BookingResponse> GetBookingAsync(string bookingId);
    Task CreateBooking(CreateBookingRequest request);
    Task<PaginationWrapper<List<BookingResponse>, BookingResponse>> GetAllBookingAsync(int page,
        int pageSize, string? search);

    Task<PaginationWrapper<List<BookingResponse>, BookingResponse>> GetAllMyBookingAsync(int page, int pageSize,
        string? search);
    
    Task<PaginationWrapper<List<BookingResponse>, BookingResponse>> GetAllMyStationBookingAsync(int page, int pageSize,
        string? search);
    
    Task<PaginationWrapper<List<BookingResponse>, BookingResponse>> GetAllStationBookingAsync(string stationId, int page, int pageSize,
        string? search);
    Task<PaginationWrapper<List<BookingResponse>, BookingResponse>> GetAllMyStationBookingAsync(int page, int pageSize,
    string? search, BBRStatus? status = null); // Thêm status parameter

    Task<PaginationWrapper<List<BookingResponse>, BookingResponse>> GetAllStationBookingAsync(string stationId, int page, int pageSize,
        string? search, BBRStatus? status = null); // Thêm status parameter
    Task RejectSwapAsync(string bookingId, StaffRejectSwapRequest? request, CancellationToken ct = default);

    Task ConfirmSwapAsync(string bookingId, CancellationToken ct = default);
    Task<EstimatePriceResponse> CalculateEstimatedPriceAsync(EstimatePriceRequest request);
}