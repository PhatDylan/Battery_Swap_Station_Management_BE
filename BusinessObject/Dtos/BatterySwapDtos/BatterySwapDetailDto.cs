using BusinessObject.Enums;

namespace BusinessObject.DTOs.BatterySwap
{
    public class BatterySwapDetailDto : BatterySwapSummaryDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserPhone { get; set; } = string.Empty;
        public string VehicleBrand { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string BatteryTypeName { get; set; } = string.Empty;
        public string BatteryVoltage { get; set; } = string.Empty;
        public string BatteryCapacity { get; set; } = string.Empty;
        public string StationAddress { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
