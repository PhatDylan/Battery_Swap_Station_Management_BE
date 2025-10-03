using BusinessObject.Enums;

namespace BusinessObject.DTOs.Booking
{
    public class BookingSummaryDto
    {
        public string BookingId { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public BBRStatus Status { get; set; }
        public string StationName { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}  
