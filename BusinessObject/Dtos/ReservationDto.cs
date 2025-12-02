using BusinessObject.Enums;
using System;

namespace BusinessObject.Dtos
{
    public class ReservationRequest
    {
        public string? ReservationId { get; set; }
        public string StationInventoryId { get; set; } = string.Empty;
        public BBRStatus Status { get; set; } = BBRStatus.Pending;
        public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiredAt { get; set; }
    }

    public class ReservationResponse
    {
        public string ReservationId { get; set; } = string.Empty;
        public string StationInventoryId { get; set; } = string.Empty;
        public BBRStatus Status { get; set; }
        public DateTime ReservedAt { get; set; }
        public DateTime ExpiredAt { get; set; }

        // Các trường bổ sung nếu muốn trả về thông tin chi tiết hơn
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? BatteryId { get; set; }
        public int? BatterySerialNo { get; set; }
        public string? StationId { get; set; }
        public string? StationName { get; set; }
    }
}