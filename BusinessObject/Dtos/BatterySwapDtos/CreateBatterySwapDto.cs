using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTOs.BatterySwap
{
    public class CreateBatterySwapDto
    {
        [Required]
        public string NewBatteryId { get; set; } = string.Empty;

    }
}
