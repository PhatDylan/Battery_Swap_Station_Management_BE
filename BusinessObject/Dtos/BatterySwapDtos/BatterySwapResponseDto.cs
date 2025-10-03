namespace BusinessObject.DTOs.BatterySwap
{
    public class BatterySwapResponseDto : BatterySwapSummaryDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserPhone { get; set; } = string.Empty;
        public string VehicleBrand { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string BatteryTypeId { get; set; } = string.Empty;
        public string BatteryTypeName { get; set; } = string.Empty;
        public string? BatteryVoltage { get; set; }
        public string? BatteryCapacity { get; set; }
        public string StationAddress { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? Notes { get; set; }
    }
}