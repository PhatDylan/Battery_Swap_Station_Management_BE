using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTOs;

public class RegisterDto
{
    [Required]
    [StringLength(255, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)] public string? Phone { get; set; }

    [Required]
    [StringLength(255, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}