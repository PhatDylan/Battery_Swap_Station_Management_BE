using System.Net;
using BusinessObject;
using BusinessObject.Dtos;
using BusinessObject.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Service.Exceptions;
using Service.Interfaces;
using Service.Utils;

namespace Service.Implementations;

public class UserService(ApplicationDbContext context, IHttpContextAccessor accessor) : IUserService
{
    public async Task<PaginationWrapper<List<UserProfileResponse>, UserProfileResponse>> GetAllUsersAsync(int page,
        int pageSize, string? search)
    {
        var query = context.Users.AsQueryable();
        if (!string.IsNullOrEmpty(search))
            query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));
        var totalItem = await query.CountAsync();
        var users = await query.Skip((page - 1) * pageSize).Take(pageSize).OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserProfileResponse
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role,
                AvatarUrl = u.AvatarUrl,
                Balance = u.Balance,
                Status = u.Status,
                CreatedAt = u.CreatedAt
            }).ToListAsync();
        return new PaginationWrapper<List<UserProfileResponse>, UserProfileResponse>(users, page, totalItem, pageSize);
    }

    public async Task<UserProfileResponse> GetUserProfileAsync(string userId)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user is null)
            throw new ValidationException
            {
                ErrorMessage = "User not found",
                Code = "400",
                StatusCode = HttpStatusCode.BadRequest
            };
        return new UserProfileResponse
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            AvatarUrl = user.AvatarUrl,
            Balance = user.Balance,
            Role = user.Role,
            Status = user.Status,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserProfileResponse> GetMeProfileAsync()
    {
        var userId = JwtUtils.GetUserId(accessor);
        if (string.IsNullOrEmpty(userId))
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Code = "401",
                ErrorMessage = "Unauthorized"
            };
        return await GetUserProfileAsync(userId);
    }

    public async Task<UserProfileResponse?> UpdateUserProfileResponse(string id, UserProfileRequest userProfileDto)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (user is null)
            throw new ValidationException
            {
                ErrorMessage = "User not found",
                Code = "400",
                StatusCode = HttpStatusCode.BadRequest
            };
        user.FullName = userProfileDto.FullName;
        user.Email = userProfileDto.Email;
        user.Phone = userProfileDto.Phone;
        user.AvatarUrl = userProfileDto.AvatarUrl;
        try
        {
            await context.SaveChangesAsync();
            return await GetUserProfileAsync(id);
        }
        catch (Exception ex)
        {
            throw new ValidationException
            {
                ErrorMessage = ex.Message,
                Code = "500",
                StatusCode = HttpStatusCode.InternalServerError
            };
        }
    }

    public async Task<UserProfileResponse?> UpdateMeProfileAsync(UserProfileRequest userProfile)
    {
        var userId = JwtUtils.GetUserId(accessor);
        if (string.IsNullOrEmpty(userId))
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Code = "401",
                ErrorMessage = "Unauthorized"
            };
        return await UpdateUserProfileResponse(userId, userProfile);
    }

    public async Task UpdatePassword(ChangePasswordRequest request)
    {
        var id = JwtUtils.GetUserId(accessor);
        if (string.IsNullOrEmpty(id))
            throw new ValidationException
            {
                Code = "401",
                StatusCode = HttpStatusCode.Unauthorized,
                ErrorMessage = "Unauthorized"
            };
        var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (user is null)
            throw new ValidationException
            {
                ErrorMessage = "User not found",
                Code = "400",
                StatusCode = HttpStatusCode.BadRequest
            };
        var isOldPasswordCorrect = BCrypt.Net.BCrypt.Verify(request.OldPassword, user.Password);
        if (!isOldPasswordCorrect)
            throw new ValidationException
            {
                ErrorMessage = "Old password is incorrect",
                Code = "400",
                StatusCode = HttpStatusCode.BadRequest
            };

        if (!request.NewPassword.Equals(request.ConfirmPassword))
            throw new ValidationException
            {
                ErrorMessage = "New password and confirm password are not match",
                Code = "400",
                StatusCode = HttpStatusCode.BadRequest
            };
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
                StatusCode = HttpStatusCode.BadRequest
            };
        }
    }

    //Check Url
    private static bool IsValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true; 
        //Just http, https
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    //Update avatar, email

    public async Task<UserProfileResponse?> UpdateUserAvatarAndEmailAsync(string userId, UpdateAvatarEmailRequest request)
    {
        if (!IsValidUrl(request.AvatarUrl))
            throw new ValidationException
            {
                ErrorMessage = "Invalid AvatarUrl (requires http/https)",
                Code = "400",
                StatusCode = HttpStatusCode.BadRequest
            };

        var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user is null)
            throw new ValidationException
            {
                ErrorMessage = "User not found",
                Code = "400",
                StatusCode = HttpStatusCode.BadRequest
            };

        // Check email 
        var emailExists = await context.Users.AnyAsync(u => u.Email == request.Email && u.UserId != userId);
        if (emailExists)
            throw new ValidationException
            {
                ErrorMessage = "Email already in use",
                Code = "400",
                StatusCode = HttpStatusCode.BadRequest
            };

        user.Email = request.Email;
        user.AvatarUrl = request.AvatarUrl;

        await context.SaveChangesAsync();

        return new UserProfileResponse
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            AvatarUrl = user.AvatarUrl,
            Balance = user.Balance,
            Role = user.Role,
            Status = user.Status,
            CreatedAt = user.CreatedAt
        };
    }

    //Update for me
    public async Task<UserProfileResponse?> UpdateMeAvatarAndEmailAsync(UpdateAvatarEmailRequest request)
    {
        var meId = JwtUtils.GetUserId(accessor);
        if (string.IsNullOrEmpty(meId))
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Code = "401",
                ErrorMessage = "Unauthorized"
            };

        return await UpdateUserAvatarAndEmailAsync(meId, request);
    }
}