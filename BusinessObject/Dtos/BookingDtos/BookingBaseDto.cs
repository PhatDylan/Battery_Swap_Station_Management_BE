using BusinessObject.Enums;

namespace BusinessObject.DTOs.Booking
{
    public class BookingBaseDto
    {
        public string BookingId { get; set; } = string.Empty;
        public string StationId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string VehicleId { get; set; } = string.Empty;
        public string BatteryId { get; set; } = string.Empty;
        public string BatteryTypeId { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public BBRStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
