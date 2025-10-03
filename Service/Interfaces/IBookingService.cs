using BusinessObject.DTOs.Booking;
using BusinessObject.DTOs.BatterySwap;
using BusinessObject.Enums;

namespace Service.Interfaces
{
    public interface IBookingService
    {
        // Time Slots Management
        Task<AvailableTimeSlotDto?> GetAvailableTimeSlotsAsync(string stationId, DateTime date);
        List<string> GetAllTimeSlots();

        // Booking CRUD Operations
        Task<BookingResponseDto?> CreateBookingAsync(string userId, CreateBookingDto request);
        Task<BookingDetailDto?> GetBookingDetailAsync(string bookingId);
        Task<BookingResponseDto?> UpdateBookingStatusAsync(string bookingId, string staffId, UpdateBookingStatusDto request);
        Task<BookingResponseDto?> CancelBookingAsync(string bookingId, string userId);

        // Booking Queries
        Task<List<BookingSummaryDto>> GetUserBookingsAsync(string userId, BookingFilterDto? filter = null);
        Task<List<BookingResponseDto>> GetStationBookingsAsync(string stationId, BookingFilterDto? filter = null);
        Task<List<BookingResponseDto>> GetPendingBookingsForStaffAsync();
        Task<List<BookingResponseDto>> GetBookingsByStatusAsync(BBRStatus status, BookingFilterDto? filter = null);

        // Statistics
        Task<BookingStatisticsDto> GetBookingStatisticsAsync(string? stationId = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<BookingResponseDto>> GetTodayBookingsAsync(string? stationId = null);

        // Validation
        Task<bool> CanUserBookAsync(string userId, string vehicleId, DateTime bookingDate, string timeSlot);
        Task<bool> IsTimeSlotAvailableAsync(string stationId, DateTime bookingDate, string timeSlot);

        // Related Data
        Task<List<BatterySwapSummaryDto>> GetRelatedBatterySwapsAsync(string bookingId);
    }
}