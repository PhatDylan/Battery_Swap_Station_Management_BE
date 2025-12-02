using System.ComponentModel.DataAnnotations;
using BusinessObject.Enums;

namespace BusinessObject.DTOs;

public class BookingBase
{
    public string BookingId { get; set; } = string.Empty;
    public string StationId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string VehicleId { get; set; } = string.Empty;
    public string BatteryId { get; set; } = string.Empty;
    public string BatteryTypeId { get; set; } = string.Empty;
    public string TimeSlot { get; set; } = string.Empty;
    public BBRStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BookingResponse : BookingBase
{
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserPhone { get; set; } = string.Empty;
    public string StationName { get; set; } = string.Empty;
    public string StationAddress { get; set; } = string.Empty;
    public string VehicleBrand { get; set; } = string.Empty;
    public string VehicleModel { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string BatteryTypeName { get; set; } = string.Empty;
    public string? ConfirmedByName { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool CanCancel { get; set; }
    public bool CanModify { get; set; }
    public int EstimatedPrice { get; set; }  // Giá ??c tính
    public string PriceBreakdown { get; set; } = string.Empty; // Chi ti?t tính giá
}

public class BookingSummaryResponse
{
    public string BookingId { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public string TimeSlot { get; set; } = string.Empty;
    public BBRStatus Status { get; set; }
    public string StationName { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class DailyBookingCountResponse
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
    public int CompletedCount { get; set; }
    public int CancelledCount { get; set; }
}

public class BookingStatisticsResponse
{
    public int TotalBookings { get; set; }
    public int PendingBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public double CompletionRate { get; set; }
    public double CancellationRate { get; set; }
    public List<DailyBookingCountResponse> DailyStats { get; set; } = new();
}

public class CreateBookingRequest
{
    [Required] public string VehicleId { get; set; } = string.Empty;

    [Required] public string StationId { get; set; } = string.Empty;

    [Required] public List<string> SlotIds { get; set; } = [];
    
    public DateTime? BookingDate { get; set; }
}

public class UpdateBookingStatusRequest
{
    [Required] public BBRStatus Status { get; set; }
}