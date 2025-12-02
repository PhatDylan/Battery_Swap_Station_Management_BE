using BusinessObject.Enums;

namespace BusinessObject.DTOs
{
    public class StaffSwapListItemResponse
    {
        public string SwapId { get; set; } = string.Empty;
        public string StationId { get; set; } = string.Empty;

        public string? BookingId { get; set; }
        public DateTime? BookingTime { get; set; }

        public string UserId { get; set; } = string.Empty;
        public string VehicleId { get; set; } = string.Empty;

        public string BatteryId { get; set; } = string.Empty;   // Pin khách trả
        public string ToBatteryId { get; set; } = string.Empty; // Pin trạm cấp

        public BBRStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class StaffRejectSwapRequest
    {
        public string? Reason { get; set; }
    }
}