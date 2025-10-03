namespace BusinessObject.DTOs.Booking
{
    public class BookingStatisticsDto
    {
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public double CompletionRate { get; set; }
        public double CancellationRate { get; set; }
        public List<DailyBookingCountDto> DailyStats { get; set; } = new List<DailyBookingCountDto>();
    }

    public class DailyBookingCountDto
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
    }
}
