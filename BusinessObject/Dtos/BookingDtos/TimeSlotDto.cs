using BusinessObject.Enums;

namespace BusinessObject.DTOs.Booking
{
    public class TimeSlotDto
    {
        public string TimeSlot { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public string? BookedByUserId { get; set; }
        public string? BookedByUserName { get; set; }
        public BBRStatus? BookingStatus { get; set; }
        public string? BookingId { get; set; }
    }
}