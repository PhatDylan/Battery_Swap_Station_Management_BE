namespace BusinessObject.DTOs;

public class VehicleRequest
{
    public string BatteryTypeId { get; set; } = string.Empty;
    public string VBrand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
}

public class VehicleResponse
{
    public string VehicleId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string BatteryId { get; set; } = string.Empty;
    public string BatteryTypeId { get; set; } = string.Empty;
    public string BatteryTypeName { get; set; } = string.Empty;
    public string VBrand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
}