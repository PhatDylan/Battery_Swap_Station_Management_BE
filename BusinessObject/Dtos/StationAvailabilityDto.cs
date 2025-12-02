namespace BusinessObject.Dtos
{
    public class StationAvailabilityDto
    {
        public string StationId { get; set; } = string.Empty;
        public string StationName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // Thống kê để client biết trạng thái
        public int AvailableSlots { get; set; }
        public int TotalSlots { get; set; }
        public int AvailableBatteriesOfType { get; set; }

        // true nếu station không còn slot available (dùng để loại trừ nếu cần)
        public bool IsFullForSwap => AvailableSlots == 0;
    }
}