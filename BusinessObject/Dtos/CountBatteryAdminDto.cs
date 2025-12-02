namespace BusinessObject.Dtos;

public class CountBatteryAdminDto
{
    public List<CountBatterySwapDto> ByDay { get; set; } = new();
    public List<CountBatterySwapDto> ByMonth { get; set; } = new();
    public List<CountBatterySwapDto> ByYear { get; set; } = new();
}