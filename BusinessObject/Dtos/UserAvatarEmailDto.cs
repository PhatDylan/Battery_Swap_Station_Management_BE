using System.ComponentModel.DataAnnotations;

namespace BusinessObject.Dtos
{
    public class UpdateAvatarEmailRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Url]
        public string? AvatarUrl { get; set; }
    }
}
