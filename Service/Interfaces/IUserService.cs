using BusinessObject.DTOs;
using BusinessObject.Dtos;

namespace Service.Interfaces;

public interface IUserService
{
    Task<UserProfileResponse> GetUserProfileAsync(string userId);
    Task<UserProfileResponse> GetMeProfileAsync();
    Task<UserProfileResponse?> UpdateUserProfileResponse(string id, UserProfileRequest userProfileDto);
    Task<UserProfileResponse?> UpdateMeProfileAsync(UserProfileRequest userProfile);
    Task UpdatePassword(ChangePasswordRequest request);

    Task<PaginationWrapper<List<UserProfileResponse>, UserProfileResponse>> GetAllUsersAsync(int page,
        int pageSize, string? search);

    //Update avatar, email
    Task<UserProfileResponse?> UpdateUserAvatarAndEmailAsync(string userId, UpdateAvatarEmailRequest request);
    Task<UserProfileResponse?> UpdateMeAvatarAndEmailAsync(UpdateAvatarEmailRequest request);
}