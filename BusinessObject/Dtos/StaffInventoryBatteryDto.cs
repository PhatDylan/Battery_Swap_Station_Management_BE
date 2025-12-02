using BusinessObject.Enums;

namespace BusinessObject.DTOs;


//tóm tắt tồn kho (đếm số lượng pin theo trạng thái)
public class BatteryInventorySummaryRequest
{
    public string StationId { get; set; } = string.Empty;
}

//liệt kê tồn kho có lọc/phân loại
public class BatteryInventorySearchRequest
{
    public string StationId { get; set; } = string.Empty;

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    // Bộ lọc
    public BatteryStatus? Status { get; set; }           
    public string? Model { get; set; }                    // Vehicle.Model
    public string? BatteryTypeId { get; set; }            // Loại pin
    public int? CapacityMinWh { get; set; }               // Lọc theo dung lượng tối thiểu
    public int? CapacityMaxWh { get; set; }               // Lọc theo dung lượng tối đa
}

// Tóm tắt số lượng pin theo trạng thái
public class BatteryInventorySummaryResponse
{
    public int Total { get; set; }
    public int Available { get; set; }
    public int Charging { get; set; }
    public int Maintenance { get; set; }
}

// Item chi tiết trong danh sách tồn kho
public class BatteryInventoryItemResponse
{
    public string BatteryId { get; set; } = string.Empty;
    public int SerialNo { get; set; }
    public string StationId { get; set; } = string.Empty;

    public string BatteryTypeId { get; set; } = string.Empty;
    public string BatteryTypeName { get; set; } = string.Empty;

    public int CapacityWh { get; set; }
    public int CurrentCapacityWh { get; set; }
    public double SoCPercent => CapacityWh == 0 ? 0 : Math.Round((double)CurrentCapacityWh / CapacityWh * 100, 1);

    public string? VehicleModel { get; set; }
    public BatteryStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}