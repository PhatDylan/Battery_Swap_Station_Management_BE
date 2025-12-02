using System.Net;
using BusinessObject;
using BusinessObject.DTOs;
using BusinessObject.Entities;
using BusinessObject.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Service.Exceptions;
using Service.Interfaces;
using Service.Utils;

namespace Service.Implementations
{
    public class UserManagementService(ApplicationDbContext context, IHttpContextAccessor accessor) : IUserManagementService
    {
        public async Task<UserProfileResponse> PromoteUserToStaffAsync(string userId)
        {
            var adminUserId = JwtUtils.GetUserId(accessor);
            if (string.IsNullOrEmpty(adminUserId))
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    ErrorMessage = "Unauthorized",
                    Code = "401"
                };

            // Validate admin role
            var admin = await context.Users.FindAsync(adminUserId);
            if (admin?.Role != UserRole.Admin)
                throw new ValidationException
                {
                    ErrorMessage = "Only admin can promote users to staff",
                    Code = "403",
                    StatusCode = HttpStatusCode.Forbidden
                };

            // Find target user
            var user = await context.Users.FindAsync(userId);
            if (user == null)
                throw new ValidationException
                {
                    ErrorMessage = "User not found",
                    Code = "404",
                    StatusCode = HttpStatusCode.NotFound
                };

            // Check if user is already staff or admin
            if (user.Role == UserRole.Staff)
                throw new ValidationException
                {
                    ErrorMessage = "User is already a staff member",
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };

            if (user.Role == UserRole.Admin)
                throw new ValidationException
                {
                    ErrorMessage = "Cannot change admin role",
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };

            // Promote user to staff
            var previousRole = user.Role;
            user.Role = UserRole.Staff;

            await context.SaveChangesAsync();

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

        public async Task<UserProfileResponse> DemoteStaffToUserAsync(string userId)
        {
            var adminUserId = JwtUtils.GetUserId(accessor);
            if (string.IsNullOrEmpty(adminUserId))
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    ErrorMessage = "Unauthorized",
                    Code = "401"
                };

            var admin = await context.Users.FindAsync(adminUserId);
            if (admin?.Role != UserRole.Admin)
                throw new ValidationException
                {
                    ErrorMessage = "Only admin can demote staff",
                    Code = "403",
                    StatusCode = HttpStatusCode.Forbidden
                };

            var user = await context.Users.FindAsync(userId);
            if (user == null)
                throw new ValidationException
                {
                    ErrorMessage = "User not found",
                    Code = "404",
                    StatusCode = HttpStatusCode.NotFound
                };

            if (user.Role != UserRole.Staff)
                throw new ValidationException
                {
                    ErrorMessage = "User is not a staff member",
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };

            // Remove all station assignments before demoting
            var stationAssignments = await context.StationStaffs
                .Where(ss => ss.UserId == userId)
                .ToListAsync();

            context.StationStaffs.RemoveRange(stationAssignments);

            user.Role = UserRole.Driver;
            await context.SaveChangesAsync();

            return new UserProfileResponse
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl,
                Balance = user.Balance,
                Phone = user.Phone,
                Role = user.Role,
                Status = user.Status,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<UserProfileResponse> CreateStaffAccountAsync(CreateStaffRequest request)
        {
            var adminUserId = JwtUtils.GetUserId(accessor);
            if (string.IsNullOrEmpty(adminUserId))
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    ErrorMessage = "Unauthorized",
                    Code = "401"
                };

            var admin = await context.Users.FindAsync(adminUserId);
            if (admin?.Role != UserRole.Admin)
                throw new ValidationException
                {
                    ErrorMessage = "Only admin can create staff accounts",
                    Code = "403",
                    StatusCode = HttpStatusCode.Forbidden
                };

            // Check if email already exists
            var existingUser = await context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
                throw new ValidationException
                {
                    ErrorMessage = "Email already exists",
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };

            var newStaff = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = UserRole.Staff,
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(newStaff);
            await context.SaveChangesAsync();

            return new UserProfileResponse
            {
                UserId = newStaff.UserId,
                FullName = newStaff.FullName,
                Email = newStaff.Email,
                Phone = newStaff.Phone,
                AvatarUrl = newStaff.AvatarUrl,
                Balance = newStaff.Balance,
                Role = newStaff.Role,
                Status = newStaff.Status,
                CreatedAt = newStaff.CreatedAt
            };
        }

        public async Task<PaginationWrapper<List<UserProfileResponse>, UserProfileResponse>> GetAllUsersAsync(
            int page, int pageSize, string? search, UserRole? role = null)
        {
            var adminUserId = JwtUtils.GetUserId(accessor);
            if (string.IsNullOrEmpty(adminUserId))
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    ErrorMessage = "Unauthorized",
                    Code = "401"
                };

            var admin = await context.Users.FindAsync(adminUserId);
            if (admin?.Role != UserRole.Admin)
                throw new ValidationException
                {
                    ErrorMessage = "Only admin can view all users",
                    Code = "403",
                    StatusCode = HttpStatusCode.Forbidden
                };

            var query = context.Users.AsQueryable();

            if (role.HasValue)
                query = query.Where(u => u.Role == role.Value);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(u => u.FullName.Contains(search) ||
                                        u.Email.Contains(search));

            var totalItems = await query.CountAsync();
            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderByDescending(u => u.CreatedAt)
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
                })
                .ToListAsync();

            return new PaginationWrapper<List<UserProfileResponse>, UserProfileResponse>(
                users, totalItems, page, pageSize);
        }
    }
}