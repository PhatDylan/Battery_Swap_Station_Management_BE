namespace BusinessObject.DTOs;

public class EstimatePriceRequest
{
    public string VehicleId { get; set; } = string.Empty;
    public string StationId { get; set; } = string.Empty;
}

public class EstimatePriceResponse
{
    public string VehicleId { get; set; } = string.Empty;
    public string BatteryId { get; set; } = string.Empty;
    public string BatteryTypeId { get; set; } = string.Empty;
    public string BatteryTypeName { get; set; } = string.Empty;
    public double EstimatedPrice { get; set; }
    public string PriceBreakdown { get; set; } = string.Empty;
    public int BatteryCapacity { get; set; }
    public int CurrentCapacity { get; set; }
    public double BatteryPercentage { get; set; }
    public string StationName { get; set; } = string.Empty;
    public int ElectricityRate { get; set; }
}
