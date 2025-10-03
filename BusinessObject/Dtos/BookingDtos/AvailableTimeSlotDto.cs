namespace BusinessObject.DTOs.Booking
{
    public class AvailableTimeSlotDto
    {
        public DateTime Date { get; set; }
        public string StationId { get; set; } = string.Empty;
        public string StationName { get; set; } = string.Empty;
        public string StationAddress { get; set; } = string.Empty;
        public List<TimeSlotDto> TimeSlots { get; set; } = new List<TimeSlotDto>();
        public int AvailableCount => TimeSlots.Count(t => t.IsAvailable);
        public int TotalSlots => TimeSlots.Count;
        public string OperatingHours { get; set; } = "08:00 - 20:00";
    }
}
