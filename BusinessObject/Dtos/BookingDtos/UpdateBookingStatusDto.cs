using System.ComponentModel.DataAnnotations;
using BusinessObject.Enums;

namespace BusinessObject.DTOs.Booking
{
    public class UpdateBookingStatusDto
    {
        [Required]
        public BBRStatus Status { get; set; }

    }
}