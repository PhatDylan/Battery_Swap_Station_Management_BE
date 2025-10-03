using BusinessObject.Dtos.UserDtos;
using BusinessObject.Entities;


namespace Service.Interfaces;

public interface IUserService 
{
    User GetUserById(string userId);
    void UpdateUser(User user);
    void DeleteUser(string userId);
    void changePassword(string userId, string oldPassword, string newPassword);
    List<User> GetAllUsers();

    //Task<UserProfileResponse?> GetUserProfileAsync(string userId);
    //Task<UserProfileResponse?> GetMeProfileAsync();
    //Task<UserProfileResponse?> UpdateUserProfileResponse(string id, UserProfileRequest userProfileDto);
    //Task<UserProfileResponse?> UpdateMeProfileAsync(UserProfileRequest userProfile);
    //Task UpdatePassword(ChangePasswordRequest request);
    //Task<PaginationWrapper<List<UserProfileResponse>, UserProfileResponse>> GetAllUsersAsync(int page,
    //    int pageSize, string? search);


}