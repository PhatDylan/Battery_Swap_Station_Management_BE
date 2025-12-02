using System.ComponentModel.DataAnnotations;

namespace BusinessObject.Dtos
{
    public class SupportTicketEmergencyRequest
    {
        public string? StationId { get; set; }

        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = string.Empty;
    }
}