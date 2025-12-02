namespace BusinessObject.DTOs
{
    public class RebalanceRequest
    {
        public string BatteryTypeId { get; set; } = string.Empty;
        // Số pin mục tiêu sẵn sàng ở mỗi trạm (target). Nếu null -> phân bổ theo trung bình.
        public int? TargetReadyPerStation { get; set; }
        // Giới hạn số pin chuyển trong 1 cặp from->to (0 hoặc null = không giới hạn)
        public int? MaxTransferPerPair { get; set; }
        // Có tính khoảng cách để ưu tiên gần hơn không
        public bool PreferNearest { get; set; } = false;
        // Chỉ điều phối trong các trạm thuộc admin hiện tại
        public bool RestrictToMyStations { get; set; } = true;
        // Tùy chọn: danh sách station được phép tham gia; nếu có sẽ filter theo danh sách này
        public List<string>? StationIds { get; set; }
    }

    public class DispatchPlanItem
    {
        public string FromStationId { get; set; } = string.Empty;
        public string ToStationId { get; set; } = string.Empty;
        public string BatteryTypeId { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public class DispatchPlanResponse
    {
        public string BatteryTypeId { get; set; } = string.Empty;
        public List<DispatchPlanItem> Items { get; set; } = new();
        public int TotalMoves => Items.Sum(x => x.Quantity);
        // Thống kê nhanh
        public Dictionary<string, int> StationReadyAfterPlan { get; set; } = new();
    }

    // Thực thi: có thể để service tự chọn pin theo trạng thái Available,
    // hoặc truyền chính xác các BatteryId để dịch chuyển.
    public class ExecuteDispatchRequest
    {
        public List<ExecuteDispatchMove> Moves { get; set; } = new();
        // Nếu BatteryIds rỗng, service sẽ tự pick theo Available
    }

    public class ExecuteDispatchMove
    {
        public string FromStationId { get; set; } = string.Empty;
        public string ToStationId { get; set; } = string.Empty;
        public string BatteryTypeId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public List<string>? BatteryIds { get; set; } // optional
    }

    public class ExecuteDispatchResult
    {
        public int Moved { get; set; }
        public List<string> AffectedBatteryIds { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}