using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Dtos
{
    public class SupportTicketRequest
    {
        public string? TicketId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? StationId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Priority Priority { get; set; }
        public SupportTicketStatus Status { get; set; }
    }

    public class SupportTicketResponse
    {
        public string TicketId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? StationId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Priority Priority { get; set; }
        public SupportTicketStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Additional fields for detailed information
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? StationName { get; set; }
        public string? StationAddress { get; set; }
        public string PriorityText => Priority.ToString();
        public string StatusText => Status.ToString();
    }
}