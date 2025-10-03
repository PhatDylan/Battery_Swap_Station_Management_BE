using System.IdentityModel.Tokens.Jwt;
using System.Net;
using BusinessObject;
using BusinessObject.DTOs;
using BusinessObject.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Exceptions;
using Service.Interfaces;

namespace Service.Implementations;

public class UserService(ApplicationDbContext context, ILogger<UserService> logger) : IUserService
{

    public async Task<PaginationWrapper<List<UserProfileResponse>, UserProfileResponse>> GetAllUsersAsync(int page,
        int pageSize, string? search)
    {
        var query = context.Users.AsQueryable();
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));
        }
        var totalItem = await query.CountAsync();
        var users = await query.Skip((page - 1) * pageSize).Take(pageSize).OrderByDescending(u => u.CreatedAt).Select(u => new UserProfileResponse
        {
            UserId = u.UserId,
            FullName = u.FullName,
            Email = u.Email,
            Phone = u.Phone,
            Role = u.Role,
            Status = u.Status,
            CreatedAt = u.CreatedAt
        }).ToListAsync();
        return new PaginationWrapper<List<UserProfileResponse>, UserProfileResponse>(users, page, totalItem, pageSize);
    } 
    
    public async Task<UserProfileResponse?> GetUserProfileAsync(string userId)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user is not { Status: UserStatus.Active })
        {
            return null;
        }
        return new UserProfileResponse
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role,
            Status = user.Status,
            CreatedAt = user.CreatedAt
        };
    }
    
    public async Task<UserProfileResponse?> GetMeProfileAsync()
    {
        const string userId = JwtRegisteredClaimNames.Sub;
        if (string.IsNullOrEmpty(userId))
        {
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Code = "401",
                ErrorMessage = "Unauthorized"
            };
        }
        return await GetUserProfileAsync(userId);
    }

    public async Task<UserProfileResponse?> UpdateUserProfileResponse(string id, UserProfileRequest userProfileDto)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (user is null)
        {
            return null;
        }
        user.FullName = userProfileDto.FullName;
        user.Email = userProfileDto.Email;
        user.Phone = userProfileDto.Phone;
        try
        {
            await context.SaveChangesAsync();
            return await GetUserProfileAsync(id);
        } catch
        {
            logger.LogError("Error when try to update user profile");
            throw;
        }
    }
    
    public async Task<UserProfileResponse?> UpdateMeProfileAsync(UserProfileRequest userProfile)
    {
        const string userId = JwtRegisteredClaimNames.Sub;
        if (string.IsNullOrEmpty(userId))
        {
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Code = "401",
                ErrorMessage = "Unauthorized"
            };
        }
        return await UpdateUserProfileResponse(userId, userProfile);
    }

    public async Task UpdatePassword(ChangePasswordRequest request)
    {
        const string id = JwtRegisteredClaimNames.Sub;
        if (string.IsNullOrEmpty(id))
        {
            throw new ValidationException
            {
                Code = "401",
                StatusCode = HttpStatusCode.Unauthorized,
                ErrorMessage = "Unauthorized"
            };
        }
        var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (user is null)
        {
            throw new ValidationException
            {
                ErrorMessage = "User not found",
                Code = "400",
                StatusCode = HttpStatusCode.BadRequest,
            };
        }
        var isOldPasswordCorrect = BCrypt.Net.BCrypt.Verify(request.OldPassword, user.Password);
        if (!isOldPasswordCorrect)
        {
            throw new ValidationException
            {
                ErrorMessage = "Old password is incorrect",
                Code = "400",
                StatusCode = HttpStatusCode.BadRequest,
            };
        }

        if (!request.NewPassword.Equals(request.ConfirmPassword))
        {
            throw new ValidationException
            {
                ErrorMessage = "New password and confirm password are not match",
                Code = "400",
                StatusCode = HttpStatusCode.BadRequest,
            };
        }
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.Password = hashedPassword;
        try
        {
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new ValidationException
            {
                ErrorMessage = ex.Message,
                Code = "400",
                StatusCode = HttpStatusCode.BadRequest,
            };
        }

    }
}