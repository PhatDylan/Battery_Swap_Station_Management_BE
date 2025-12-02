// BusinessObject/DTOs/StationStaffDto.cs
namespace BusinessObject.DTOs
{
    public class AssignStaffRequest
    {
        public string StationId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty; // Staff user ID (must have Role = Staff)
    }

    public class StationStaffBaseResponse
    {
        public string StationStaffId { get; set; } = string.Empty;
        public string StationId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    public class StationStaffResponse
    {
        public string StationStaffId { get; set; } = string.Empty;
        public string StationId { get; set; } = string.Empty;
        public string StationName { get; set; } = string.Empty;
        public string StationAddress { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string StaffName { get; set; } = string.Empty;
        public string StaffEmail { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
    }

    public class StaffStationResponse
    {
        public string StationId { get; set; } = string.Empty;
        public string StationName { get; set; } = string.Empty;
        public string StationAddress { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsActive { get; set; }
        public DateTime AssignedAt { get; set; }
    }
}