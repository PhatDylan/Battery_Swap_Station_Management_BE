using BusinessObject.Enums;
using System;

namespace BusinessObject.Dtos
{
    public class StationBatterySlotRequest
    {
        public string? StationSlotId { get; set; }
        public string StationId { get; set; } = string.Empty;
        public string? BatteryId { get; set; }
        public int SlotNo { get; set; }
        public SBSStatus Status { get; set; }
    }

    public class StationBatterySlotResponse
    {
        public string StationSlotId { get; set; } = string.Empty;
        public string StationId { get; set; } = string.Empty;
        public string? BatteryId { get; set; }
        public int SlotNo { get; set; }
        public SBSStatus Status { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}