using BusinessObject.DTOs;
using BusinessObject.Enums;

namespace Service.Interfaces
{
    public interface IUserManagementService
    {
        Task<UserProfileResponse> PromoteUserToStaffAsync(string userId);
        Task<UserProfileResponse> DemoteStaffToUserAsync(string userId);
        Task<UserProfileResponse> CreateStaffAccountAsync(CreateStaffRequest request);
        Task<PaginationWrapper<List<UserProfileResponse>, UserProfileResponse>> GetAllUsersAsync(int page, int pageSize, string? search, UserRole? role = null);
    }
}