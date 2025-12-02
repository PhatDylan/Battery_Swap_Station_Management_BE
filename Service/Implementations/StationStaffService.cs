// Service/Implementations/StationStaffService.cs

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
    public class StationStaffService(ApplicationDbContext context, IHttpContextAccessor accessor) : IStationStaffService
    {
        public async Task<StationStaffResponse> AssignStaffToStationAsync(AssignStaffRequest request)
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
            if (admin is not { Role: UserRole.Admin })
                throw new ValidationException
                {
                    ErrorMessage = "Only admin can assign staff to stations",
                    Code = "403",
                    StatusCode = HttpStatusCode.Forbidden
                };

            // Validate station belongs to admin
            var station = await context.Stations
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StationId == request.StationId && s.UserId == adminUserId);

            if (station == null)
                throw new ValidationException
                {
                    ErrorMessage = "Station not found or you don't have permission to manage this station",
                    Code = "404",
                    StatusCode = HttpStatusCode.NotFound
                };

            // Validate staff user exists and has Staff role
            var staffUser = await context.Users.FindAsync(request.UserId);
            if (staffUser == null || staffUser.Role != UserRole.Staff)
                throw new ValidationException
                {
                    ErrorMessage = "User not found or user is not a staff member",
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };

            // Check if staff is already assigned to this station
            var existingAssignment = await context.StationStaffs
                .FirstOrDefaultAsync(ss => ss.StationId == request.StationId && ss.UserId == request.UserId);

            if (existingAssignment != null)
                throw new ValidationException
                {
                    ErrorMessage = "Staff is already assigned to this station",
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };

            // Create new station staff assignment
            var stationStaff = new StationStaff
            {
                StationId = request.StationId,
                UserId = request.UserId,
                AssignedAt = DateTime.UtcNow
            };

            try
            {
                context.StationStaffs.Add(stationStaff);
                await context.SaveChangesAsync();

                return new StationStaffResponse
                {
                    StationStaffId = stationStaff.StationStaffId,
                    StationId = stationStaff.StationId,
                    StationName = station.Name,
                    StationAddress = station.Address,
                    UserId = stationStaff.UserId,
                    StaffName = staffUser.FullName,
                    StaffEmail = staffUser.Email,
                    AssignedAt = stationStaff.AssignedAt
                };
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

        public async Task<StationStaffResponse> RemoveStaffFromStationAsync(string stationStaffId)
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
            if (admin is not { Role: UserRole.Admin })
                throw new ValidationException
                {
                    ErrorMessage = "Only admin can remove staff from stations",
                    Code = "403",
                    StatusCode = HttpStatusCode.Forbidden
                };

            // Find station staff assignment
            var stationStaff = await context.StationStaffs
                .Include(ss => ss.Station)
                .Include(ss => ss.User)
                .FirstOrDefaultAsync(ss => ss.StationStaffId == stationStaffId);

            if (stationStaff == null)
                throw new ValidationException
                {
                    ErrorMessage = "Station staff assignment not found",
                    Code = "404",
                    StatusCode = HttpStatusCode.NotFound
                };

            // Validate station belongs to admin
            if (stationStaff.Station.UserId != adminUserId)
                throw new ValidationException
                {
                    ErrorMessage = "You don't have permission to manage staff for this station",
                    Code = "403",
                    StatusCode = HttpStatusCode.Forbidden
                };

            var response = new StationStaffResponse
            {
                StationStaffId = stationStaff.StationStaffId,
                StationId = stationStaff.StationId,
                StationName = stationStaff.Station.Name,
                StationAddress = stationStaff.Station.Address,
                UserId = stationStaff.UserId,
                StaffName = stationStaff.User.FullName,
                StaffEmail = stationStaff.User.Email,
                AssignedAt = stationStaff.AssignedAt
            };

            try
            {
                context.StationStaffs.Remove(stationStaff);
                await context.SaveChangesAsync();
                return response;
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

        public async Task<StationStaffBaseResponse> GetStationStaffBaseAsync(string userId)
        {
            var stationStaff = await context.StationStaffs
                .Include(ss => ss.Station)
                .Include(ss => ss.User)
                .FirstOrDefaultAsync(ss => ss.UserId == userId);
            if (stationStaff is null)
            {
                throw new ValidationException
                {
                    ErrorMessage = "Station staff not found",
                    Code = "404",
                    StatusCode = HttpStatusCode.NotFound
                };
            }

            return new StationStaffBaseResponse
            {
                StationId = stationStaff.StationId,
                StationStaffId = stationStaff.StationStaffId,
                UserId = userId
            };
        }

        public async Task<PaginationWrapper<List<StationStaffResponse>, StationStaffResponse>> GetStationStaffsAsync(string stationId, int page, int pageSize, string? search)
        {
            var adminUserId = JwtUtils.GetUserId(accessor);
            if (string.IsNullOrEmpty(adminUserId))
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    ErrorMessage = "Unauthorized",
                    Code = "401"
                };

            // Validate station belongs to admin
            var station = await context.Stations
                .FirstOrDefaultAsync(s => s.StationId == stationId && s.UserId == adminUserId);

            if (station == null)
                throw new ValidationException
                {
                    ErrorMessage = "Station not found or you don't have permission",
                    Code = "404",
                    StatusCode = HttpStatusCode.NotFound
                };

            var query = context.StationStaffs
                .Include(ss => ss.Station)
                .Include(ss => ss.User)
                .Where(ss => ss.StationId == stationId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(ss => ss.User.FullName.Contains(search) || ss.User.Email.Contains(search));
            }

            var totalItems = await query.CountAsync();
            var stationStaffs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderByDescending(ss => ss.AssignedAt)
                .Select(ss => new StationStaffResponse
                {
                    StationStaffId = ss.StationStaffId,
                    StationId = ss.StationId,
                    StationName = ss.Station.Name,
                    StationAddress = ss.Station.Address,
                    UserId = ss.UserId,
                    StaffName = ss.User.FullName,
                    StaffEmail = ss.User.Email,
                    AssignedAt = ss.AssignedAt
                })
                .ToListAsync();

            return new PaginationWrapper<List<StationStaffResponse>, StationStaffResponse>(stationStaffs, totalItems, page, pageSize);
        }

        public async Task<PaginationWrapper<List<StaffStationResponse>, StaffStationResponse>> GetStaffStationsAsync(string staffUserId, int page, int pageSize)
        {
            var currentUserId = JwtUtils.GetUserId(accessor);
            if (string.IsNullOrEmpty(currentUserId))
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    ErrorMessage = "Unauthorized",
                    Code = "401"
                };

            // Admin can view any staff's stations, Staff can only view their own
            var currentUser = await context.Users.FindAsync(currentUserId);
            if (currentUser == null)
                throw new ValidationException
                {
                    ErrorMessage = "User not found",
                    Code = "404",
                    StatusCode = HttpStatusCode.NotFound
                };

            if (currentUser.Role != UserRole.Admin && currentUserId != staffUserId)
                throw new ValidationException
                {
                    ErrorMessage = "You can only view your own station assignments",
                    Code = "403",
                    StatusCode = HttpStatusCode.Forbidden
                };

            var query = context.StationStaffs
                .Include(ss => ss.Station)
                .Where(ss => ss.UserId == staffUserId);

            var totalItems = await query.CountAsync();
            var staffStations = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderByDescending(ss => ss.AssignedAt)
                .Select(ss => new StaffStationResponse
                {
                    StationId = ss.StationId,
                    StationName = ss.Station.Name,
                    StationAddress = ss.Station.Address,
                    Latitude = ss.Station.Latitude,
                    Longitude = ss.Station.Longitude,
                    IsActive = ss.Station.IsActive,
                    AssignedAt = ss.AssignedAt
                })
                .ToListAsync();

            return new PaginationWrapper<List<StaffStationResponse>, StaffStationResponse>(staffStations, totalItems, page, pageSize);
        }

        public async Task<StationStaffResponse> GetStationStaffAsync(string stationStaffId)
        {
            var currentUserId = JwtUtils.GetUserId(accessor);
            if (string.IsNullOrEmpty(currentUserId))
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    ErrorMessage = "Unauthorized",
                    Code = "401"
                };

            var stationStaff = await context.StationStaffs
                .Include(ss => ss.Station)
                .Include(ss => ss.User)
                .FirstOrDefaultAsync(ss => ss.StationStaffId == stationStaffId);

            if (stationStaff == null)
                throw new ValidationException
                {
                    ErrorMessage = "Station staff assignment not found",
                    Code = "404",
                    StatusCode = HttpStatusCode.NotFound
                };

            var currentUser = await context.Users.FindAsync(currentUserId);

            // Only admin of the station or the staff themselves can view
            if (currentUser?.Role != UserRole.Admin ||
                (stationStaff.Station.UserId != currentUserId && stationStaff.UserId != currentUserId))
                throw new ValidationException
                {
                    ErrorMessage = "You don't have permission to view this assignment",
                    Code = "403",
                    StatusCode = HttpStatusCode.Forbidden
                };

            return new StationStaffResponse
            {
                StationStaffId = stationStaff.StationStaffId,
                StationId = stationStaff.StationId,
                StationName = stationStaff.Station.Name,
                StationAddress = stationStaff.Station.Address,
                UserId = stationStaff.UserId,
                StaffName = stationStaff.User.FullName,
                StaffEmail = stationStaff.User.Email,
                AssignedAt = stationStaff.AssignedAt
            };
        }

        public async Task<List<StationStaffResponse>> GetAllStaffsByStationAsync(string stationId)
        {
            var adminUserId = JwtUtils.GetUserId(accessor);
            if (string.IsNullOrEmpty(adminUserId))
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    ErrorMessage = "Unauthorized",
                    Code = "401"
                };

            // Validate station belongs to admin
            var station = await context.Stations
                .FirstOrDefaultAsync(s => s.StationId == stationId && s.UserId == adminUserId);

            if (station == null)
                throw new ValidationException
                {
                    ErrorMessage = "Station not found or you don't have permission",
                    Code = "404",
                    StatusCode = HttpStatusCode.NotFound
                };

            return await context.StationStaffs
                .Include(ss => ss.Station)
                .Include(ss => ss.User)
                .Where(ss => ss.StationId == stationId)
                .Select(ss => new StationStaffResponse
                {
                    StationStaffId = ss.StationStaffId,
                    StationId = ss.StationId,
                    StationName = ss.Station.Name,
                    StationAddress = ss.Station.Address,
                    UserId = ss.UserId,
                    StaffName = ss.User.FullName,
                    StaffEmail = ss.User.Email,
                    AssignedAt = ss.AssignedAt
                })
                .ToListAsync();
        }
    }
}