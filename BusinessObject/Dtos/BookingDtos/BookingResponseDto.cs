using BusinessObject.Enums;

namespace BusinessObject.DTOs.Booking
{
    public class BookingResponseDto : BookingBaseDto
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
    }
}