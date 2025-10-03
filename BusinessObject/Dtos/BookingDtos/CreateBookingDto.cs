using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTOs.Booking
{
    public class CreateBookingDto
    {
        [Required]
        public string VehicleId { get; set; } = string.Empty;

        [Required]
        public string StationId { get; set; } = string.Empty;

        [Required]
        public string BatteryTypeId { get; set; } = string.Empty;

        [Required]
        public DateTime BookingDate { get; set; }

        [Required]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Time slot must be in HH:mm format")]
        public string TimeSlot { get; set; } = string.Empty;
    }
}