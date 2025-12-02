using System.ComponentModel.DataAnnotations;
using BusinessObject.Enums;

namespace BusinessObject.Dtos
{
    public class BatteryRequest
    {
        [Required]
        public int SerialNo { get; set; }

        [Required]
        public BatteryOwner Owner { get; set; }

        [Required]
        public BatteryStatus Status { get; set; } = BatteryStatus.Available;

        [Required]
        public string Voltage { get; set; } = null!;

        [Required]
        public int CapacityWh { get; set; }

        public string? ImageUrl { get; set; }

        public string? StationId { get; set; }

        public string? BatteryTypeId { get; set; }
    }

    public class BatteryAddBulkStationRequest
    {
        public string StationId { get; set; } = null!;
        public IEnumerable<string> BatteryIds { get; set; } = null!;
    }
    public class BatteryResponse
    {
        public string BatteryId { get; set; } = null!;
        public int SerialNo { get; set; }
        public BatteryOwner Owner { get; set; }
        public BatteryStatus Status { get; set; }
        public string Voltage { get; set; } = null!;
        public int CapacityWh { get; set; }
        public int CurrentCapacityWh { get; set; }
        public string? ImageUrl { get; set; }
        public string? StationId { get; set; }
        public string? StationName { get; set; }
        public string? BatteryTypeId { get; set; }
        public string? BatteryTypeName { get; set; }
        public string? UserId { get; set; }
        public string? ReservationId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
    public class BatteryAttachRequest
    {
        public string BatteryId { get; set; } = string.Empty;
        public string VehicleId { get; set; } = string.Empty;

        // optional: ai thực hiện (staff/driver) để audit/log
        public string? PerformedByUserId { get; set; }
    }
}
