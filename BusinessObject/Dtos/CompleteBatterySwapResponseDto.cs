namespace BusinessObject.DTOs.BatterySwap;

public class CompletedBatterySwapResponseDto
{
    public string SwapId { get; set; }
    public string VehicleId { get; set; }
    public string StationStaffId { get; set; }
    public string UserId { get; set; }
    public string StationId { get; set; }
    public string BatteryId { get; set; }
    public string ToBatteryId { get; set; }
    public string? Reason { get; set; }
    public string PaymentId { get; set; }
    public int Status { get; set; }
    public DateTime SwappedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Battery Information
    public BatteryInfoDto BatteryInfo { get; set; }
    public BatteryInfoDto ToBatteryInfo { get; set; }
}

public class BatteryInfoDto
{
    public string BatteryId { get; set; }
    public int SerialNo { get; set; }
    public string? Voltage { get; set; }
    public int CapacityWh { get; set; }
    public int CurrentCapacityWh { get; set; }
    public string? ImageUrl { get; set; }
    public int Status { get; set; }

    // Battery Type Information
    public string BatteryTypeId { get; set; }
    public string BatteryTypeName { get; set; }
}
