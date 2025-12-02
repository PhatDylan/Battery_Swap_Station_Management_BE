namespace BusinessObject.Enums;

public enum BatteryStatus
{
    Available = 1,
    InUse = 2,
    Charging = 3,
    Maintenance = 4,
    QualityCheck = 4,    //đang kiểm tra chất lượng
    Damaged = 5
}