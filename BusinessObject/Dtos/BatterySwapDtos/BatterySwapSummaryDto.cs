using BusinessObject.Enums;

namespace BusinessObject.DTOs.BatterySwap
{
    public class BatterySwapSummaryDto
    {
        public string SwapId { get; set; } = string.Empty;
        public string VehicleId { get; set; } = string.Empty;
        public string VehicleLicensePlate { get; set; } = string.Empty;
        public string BatteryId { get; set; } = string.Empty;
        public string BatterySerialNo { get; set; } = string.Empty;
        public string StationId { get; set; } = string.Empty;
        public string StationName { get; set; } = string.Empty;
        public string StaffId { get; set; } = string.Empty;
        public string StaffName { get; set; } = string.Empty;
        public BBRStatus Status { get; set; }
        public DateTime SwappedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}