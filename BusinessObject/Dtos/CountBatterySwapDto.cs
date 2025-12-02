namespace BusinessObject.Dtos;

public class CountBatterySwapDto
{
    public CountBatterySwapDto() { }

    // Mốc thời gian (theo từng loại group):
    // - ByDay: "dd/MM/yyyy"
    // - ByMonth: "MM/yyyy"
    // - ByYear: "yyyy"
    public string Time { get; set; } = string.Empty;

    // Số lượt đổi pin trong khung giờ cao điểm (4:30 - 8:00)
    public int RushHourCount { get; set; }

    // Tổng số lượt đổi pin trong ngày/tháng/năm tương ứng
    public int TotalCountPerDay { get; set; }
}