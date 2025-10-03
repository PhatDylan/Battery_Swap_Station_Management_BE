using BusinessObject.DTOs;

namespace Service.Interfaces;

public interface IUserService
{
    Task<UserProfileResponse?> GetUserProfileAsync(string userId);
    Task<UserProfileResponse?> GetMeProfileAsync();
    Task<UserProfileResponse?> UpdateUserProfileResponse(string id, UserProfileRequest userProfileDto);
    Task<UserProfileResponse?> UpdateMeProfileAsync(UserProfileRequest userProfile);
    Task UpdatePassword(ChangePasswordRequest request);
    Task<PaginationWrapper<List<UserProfileResponse>, UserProfileResponse>> GetAllUsersAsync(int page,
        int pageSize, string? search);
}