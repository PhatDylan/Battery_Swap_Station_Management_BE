// BusinessObject/DTOs/BatterySwapDto.cs
using BusinessObject.Enums;

namespace BusinessObject.DTOs
{
    public class CreateBatterySwapRequest
    {
        public string BookingId { get; set; } = string.Empty;
        public string ToBatteryId { get; set; } = string.Empty; // New battery to give to customer
    }

    public class BatterySwapResponse
    {
        public string SwapId { get; set; } = string.Empty;
        public string VehicleId { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string VehicleBrand { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string StationStaffId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserPhone { get; set; } = string.Empty;
        public string StationId { get; set; } = string.Empty;
        public string StationName { get; set; } = string.Empty;
        public string BatteryId { get; set; } = string.Empty; // Old battery from vehicle
        public int BatterySerial { get; set; }
        public string ToBatteryId { get; set; } = string.Empty; // New battery to give
        public int? ToBatterySerial { get; set; } 
        public BBRStatus Status { get; set; }
        public DateTime SwappedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool HasPayment { get; set; }
        public string PaymentId { get; set; }
    }

    public class UpdateBatterySwapStatusRequest
    {
        public BBRStatus Status { get; set; }
    }

    public class BatterySwapDetailResponse : BatterySwapResponse
    {
        public PaymentResponse? Payment { get; set; }
    }
}