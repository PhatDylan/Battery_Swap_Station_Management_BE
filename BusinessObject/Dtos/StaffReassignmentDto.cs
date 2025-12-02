using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTOs;

public class MarkAbsenceRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string Date { get; set; } = string.Empty;

    public string? Reason { get; set; }
}

public class ReassignStaffRequest
{
    [Required]
    public string AbsentUserId { get; set; } = string.Empty;

    [Required]
    public string ReplacementUserId { get; set; } = string.Empty;

    [Required]
    public string StationId { get; set; } = string.Empty;

    
    [Required]
    public string Date { get; set; } = string.Empty;

    public string? ShiftId { get; set; }

    public string? Reason { get; set; }
}

public class AvailabilityQuery
{
    
    [Required]
    public string Date { get; set; } = string.Empty;

    public string? StationId { get; set; }
    public string? ShiftId { get; set; }
}

public class AvailableStaffResponse
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}