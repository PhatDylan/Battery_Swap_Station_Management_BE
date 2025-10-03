using BusinessObject.Enums;

namespace BusinessObject.DTOs.Booking
{
    public class BookingFilterDto
    {
        public string? StationId { get; set; }
        public string? UserId { get; set; }
        public DateTime? BookingDate { get; set; }
        public BBRStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}