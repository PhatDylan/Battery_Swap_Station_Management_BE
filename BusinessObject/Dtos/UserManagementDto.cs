using BusinessObject.Enums;

namespace BusinessObject.DTOs
{
    public class PromoteUserRequest
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class CreateStaffRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UserRoleHistoryResponse
    {
        public string UserId { get; set; } = string.Empty;
        public UserRole PreviousRole { get; set; }
        public UserRole NewRole { get; set; }
        public DateTime ChangedAt { get; set; }
        public string ChangedBy { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}