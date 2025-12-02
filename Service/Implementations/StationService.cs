using System.Net;
using BusinessObject;
using BusinessObject.Dtos;
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
    public class StationService(ApplicationDbContext context, IHttpContextAccessor accessor) : IStationService
    {
        public async Task<StationResponse> GetStationAsync(string stationId)
        {
            var station = await context.Stations
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StationId == stationId);

            if (station is null)
                throw new ValidationException
                {
                    ErrorMessage = "Station not found",
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };

            return new StationResponse
            {
                StationId = station.StationId,
                UserId = station.UserId,
                Name = station.Name,
                Address = station.Address,
                Latitude = station.Latitude,
                Longitude = station.Longitude,
                IsActive = station.IsActive,
                CreatedAt = station.CreatedAt,
                UpdatedAt = station.UpdatedAt
            };
        }

        public async Task<StationResponse> CreateStationAsync(StationRequest stationRequest)
        {
            var userId = JwtUtils.GetUserId(accessor);
            if (string.IsNullOrEmpty(userId))
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    ErrorMessage = "Unauthorized",
                    Code = "401"
                };

            await ValidateStationRequest(stationRequest);

            var stationEntity = new Station
            {
                Name = stationRequest.Name,
                Address = stationRequest.Address,
                Latitude = stationRequest.Latitude,
                Longitude = stationRequest.Longitude,
                IsActive = stationRequest.IsActive,
                UserId = userId
            };

           
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                context.Stations.Add(stationEntity);
                await context.SaveChangesAsync();
                
                await transaction.CommitAsync();
                
                await context.Entry(stationEntity)
                    .Reference(s => s.User)
                    .LoadAsync();

                return new StationResponse
                {
                    StationId = stationEntity.StationId,
                    UserId = stationEntity.UserId,
                    Name = stationEntity.Name,
                    Address = stationEntity.Address,
                    Latitude = stationEntity.Latitude,
                    Longitude = stationEntity.Longitude,
                    IsActive = stationEntity.IsActive,
                    CreatedAt = stationEntity.CreatedAt,
                    UpdatedAt = stationEntity.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new ValidationException
                {
                    ErrorMessage = ex.Message,
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }
        }

        public async Task<StationResponse> UpdateStationAsync(string stationId, StationRequest stationRequest)
        {
            var stationEntity = await context.Stations
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StationId == stationId);

            if (stationEntity is null)
                throw new ValidationException
                {
                    ErrorMessage = "Station not found",
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };

            await ValidateStationRequest(stationRequest, stationId);

            stationEntity.Name = stationRequest.Name;
            stationEntity.Address = stationRequest.Address;
            stationEntity.Latitude = stationRequest.Latitude;
            stationEntity.Longitude = stationRequest.Longitude;
            stationEntity.IsActive = stationRequest.IsActive;
            stationEntity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await context.SaveChangesAsync();
                return new StationResponse
                {
                    StationId = stationEntity.StationId,
                    UserId = stationEntity.UserId,
                    Name = stationEntity.Name,
                    Address = stationEntity.Address,
                    Latitude = stationEntity.Latitude,
                    Longitude = stationEntity.Longitude,
                    IsActive = stationEntity.IsActive,
                    CreatedAt = stationEntity.CreatedAt,
                    UpdatedAt = stationEntity.UpdatedAt
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

        public async Task<StationResponse> DeleteStationAsync(string stationId)
        {
            var stationEntity = await context.Stations
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StationId == stationId);

            if (stationEntity is null)
                throw new ValidationException
                {
                    ErrorMessage = "Station not found",
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };

            var response = new StationResponse
            {
                StationId = stationEntity.StationId,
                UserId = stationEntity.UserId,
                Name = stationEntity.Name,
                Address = stationEntity.Address,
                Latitude = stationEntity.Latitude,
                Longitude = stationEntity.Longitude,
                IsActive = stationEntity.IsActive,
                CreatedAt = stationEntity.CreatedAt,
                UpdatedAt = stationEntity.UpdatedAt
            };

            try
            {
                context.Stations.Remove(stationEntity);
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

        public async Task<PaginationWrapper<List<StationResponse>, StationResponse>> GetAllStationsAsync(int page, int pageSize, string? search)
        {
            var query = context.Stations.Include(s => s.User).AsQueryable();

            if (search is not null)
                query = query.Where(s => s.Name.Contains(search) || s.Address.Contains(search));

            var totalItems = await query.CountAsync();
            var stations = await query.Skip((page - 1) * pageSize).Take(pageSize).OrderByDescending(s => s.CreatedAt)
                .Select(s => new StationResponse
                {
                    StationId = s.StationId,
                    UserId = s.UserId,
                    Name = s.Name,
                    Address = s.Address,
                    Latitude = s.Latitude,
                    Longitude = s.Longitude,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                }).ToListAsync();

            return new PaginationWrapper<List<StationResponse>, StationResponse>(stations, totalItems, page, pageSize);
        }

        public async Task<List<StationAvailabilityDto>> GetStationsWithBatteryTypeAvailableAsync(string batteryTypeId, bool excludeFullStations = true)
        {
            if (string.IsNullOrWhiteSpace(batteryTypeId))
                return new List<StationAvailabilityDto>();

            // Query: đếm số battery available cùng loại trong station,
            // đếm total slots và available slots (dựa trên StationBatterySlot.Status)
            var query = from s in context.Stations
                        let totalSlots = s.StationBatterySlots.Count()
                        let availableSlots = s.StationBatterySlots.Count(ss => ss.Status == SBSStatus.Available)
                        let availableBatteries = context.Batteries.Count(b => b.StationId == s.StationId
                                                                                && b.BatteryTypeId == batteryTypeId
                                                                                && b.Status == BatteryStatus.Available)
                        select new StationAvailabilityDto
                        {
                            StationId = s.StationId,
                            StationName = s.Name,
                            Address = s.Address,
                            Latitude = s.Latitude,
                            Longitude = s.Longitude,
                            TotalSlots = totalSlots,
                            AvailableSlots = availableSlots,
                            AvailableBatteriesOfType = availableBatteries
                        };

            var list = await query.ToListAsync();

            // Lọc: phải có ít nhất 1 battery cùng loại available
            list = list.Where(x => x.AvailableBatteriesOfType > 0).ToList();

            // Nếu muốn loại trừ station đã full để tránh overlap, lọc thêm:
            if (excludeFullStations)
                list = list.Where(x => x.AvailableSlots > 0).ToList();

            return list;
        }

        private async Task ValidateStationRequest(StationRequest stationRequest, string? excludeStationId = null)
        {
            if (string.IsNullOrWhiteSpace(stationRequest.Name))
                throw new ValidationException
                {
                    ErrorMessage = "Station name is required",
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };

            if (string.IsNullOrWhiteSpace(stationRequest.Address))
                throw new ValidationException
                {
                    ErrorMessage = "Station address is required",
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };

            if (stationRequest.Latitude < -90 || stationRequest.Latitude > 90)
                throw new ValidationException
                {
                    ErrorMessage = "Latitude must be between -90 and 90",
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };

            if (stationRequest.Longitude < -180 || stationRequest.Longitude > 180)
                throw new ValidationException
                {
                    ErrorMessage = "Longitude must be between -180 and 180",
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };

            // Check for duplicate name and address (excluding current station for update)
            var duplicateQuery = context.Stations.Where(s => s.Name == stationRequest.Name && s.Address == stationRequest.Address);

            if (!string.IsNullOrEmpty(excludeStationId))
            {
                duplicateQuery = duplicateQuery.Where(s => s.StationId != excludeStationId);
            }

            if (await duplicateQuery.AnyAsync())
                throw new ValidationException
                {
                    ErrorMessage = "Station with same name and address already exists",
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };

            // Check for duplicate coordinates (excluding current station for update)
            var duplicateLocationQuery = context.Stations.Where(s => s.Latitude == stationRequest.Latitude && s.Longitude == stationRequest.Longitude);

            if (!string.IsNullOrEmpty(excludeStationId))
            {
                duplicateLocationQuery = duplicateLocationQuery.Where(s => s.StationId != excludeStationId);
            }

            if (await duplicateLocationQuery.AnyAsync())
                throw new ValidationException
                {
                    ErrorMessage = "Station with same coordinates already exists",
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };
        }
    }
}
