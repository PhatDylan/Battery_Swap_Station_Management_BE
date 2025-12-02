using BusinessObject;
using BusinessObject.DTOs;
using BusinessObject.Entities;
using BusinessObject.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Service.Exceptions;
using Service.Interfaces;
using Service.Utils;
using System.Net;

namespace Service.Implementations;

public class VehicleService(ApplicationDbContext context, IHttpContextAccessor accessor) : IVehicleService
{
    // 1) Get single vehicle: include lookup for current battery
    public async Task<VehicleResponse> GetVehicleAsync(string vehicleId)
    {
        var vehicle = await context.Vehicles
            .Include(v => v.BatteryType)
            .Include(v => v.User)
            .FirstOrDefaultAsync(v => v.VehicleId == vehicleId);

        if (vehicle is null)
            throw new ValidationException
            {
                ErrorMessage = "Vehicle not found",
                Code = "400",
                StatusCode = HttpStatusCode.BadRequest
            };

        // Tìm battery đang InUse gắn vào vehicle (nếu có)
        var currentBatteryId = await context.Batteries
            .Where(b => b.VehicleId == vehicle.VehicleId && b.Status == BatteryStatus.InUse)
            .OrderByDescending(b => b.UpdatedAt ?? b.CreatedAt)
            .Select(b => b.BatteryId)
            .FirstOrDefaultAsync();

        return new VehicleResponse
        {
            VehicleId = vehicle.VehicleId,
            UserId = vehicle.UserId,
            UserName = vehicle.User.FullName,
            BatteryTypeId = vehicle.BatteryTypeId,
            BatteryTypeName = vehicle.BatteryType.BatteryTypeName,
            VBrand = vehicle.VBrand,
            Model = vehicle.Model,
            LicensePlate = vehicle.LicensePlate,
            BatteryId = currentBatteryId // <-- gán ở đây từ bảng Battery
        };
    }

    public async Task<VehicleResponse> CreateVehicleAsync(VehicleRequest vehicleRequest)
    {
        var userId = JwtUtils.GetUserId(accessor);
        if (string.IsNullOrEmpty(userId))
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.Unauthorized,
                ErrorMessage = "Unauthorized",
                Code = "401"
            };

        await ValidateVehicleRequest(vehicleRequest);

        var vehicleEntity = new Vehicle
        {
            VehicleId = Guid.NewGuid().ToString(),
            BatteryTypeId = vehicleRequest.BatteryTypeId,
            VBrand = vehicleRequest.VBrand,
            Model = vehicleRequest.Model,
            LicensePlate = vehicleRequest.LicensePlate,
            UserId = userId
        };

        try
        {
            context.Vehicles.Add(vehicleEntity);
            await context.SaveChangesAsync();
            var createdVehicle = await context.Vehicles
                .Include(v => v.User)
                .Include(v => v.BatteryType)
                .FirstAsync(v => v.VehicleId == vehicleEntity.VehicleId);

            return new VehicleResponse
            {
                VehicleId = createdVehicle.VehicleId,
                UserId = createdVehicle.UserId,
                UserName = createdVehicle.User.FullName,
                BatteryTypeId = createdVehicle.BatteryTypeId,
                BatteryTypeName = createdVehicle.BatteryType.BatteryTypeName,
                VBrand = createdVehicle.VBrand,
                Model = createdVehicle.Model,
                LicensePlate = createdVehicle.LicensePlate
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

    public async Task<VehicleResponse> UpdateVehicleAsync(string vehicleId, VehicleRequest vehicleRequest)
    {
        var vehicleEntity = await context.Vehicles.Include(v => v.BatteryType)
            .Include(v => v.User)
            .FirstOrDefaultAsync(v => v.VehicleId == vehicleId);
        if (vehicleEntity is null)
            throw new ValidationException
            {
                ErrorMessage = "Vehicle not found",
                Code = "400",
                StatusCode = HttpStatusCode.BadRequest
            };
        await ValidateVehicleRequest(vehicleRequest);
        vehicleEntity.BatteryTypeId = vehicleRequest.BatteryTypeId;
        vehicleEntity.VBrand = vehicleRequest.VBrand;
        vehicleEntity.Model = vehicleRequest.Model;
        vehicleEntity.LicensePlate = vehicleRequest.LicensePlate;
        try
        {
            await context.SaveChangesAsync();
            return new VehicleResponse
            {
                VehicleId = vehicleEntity.VehicleId,
                UserId = vehicleEntity.UserId,
                UserName = vehicleEntity.User.FullName,
                BatteryTypeId = vehicleEntity.BatteryTypeId,
                BatteryTypeName = vehicleEntity.BatteryType.BatteryTypeName,
                VBrand = vehicleEntity.VBrand,
                Model = vehicleEntity.Model,
                LicensePlate = vehicleEntity.LicensePlate
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

    public async Task<VehicleResponse> DeleteVehicleAsync(string vehicleId)
    {
        using var tx = await context.Database.BeginTransactionAsync();
        try
        {
            // 1) Tìm vehicle
            var vehicle = await context.Vehicles
                .Include(v => v.BatteryType)
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.VehicleId == vehicleId);

            if (vehicle is null)
                throw new ValidationException
                {
                    ErrorMessage = "Vehicle not found",
                    Code = "404",
                    StatusCode = HttpStatusCode.NotFound
                };

            // 2) Kiểm tra xem vehicle có booking đang active không
            var hasActiveBooking = await context.Bookings
                .AnyAsync(b => b.VehicleId == vehicleId &&
                              (b.Status == BBRStatus.Pending || b.Status == BBRStatus.Confirmed));

            if (hasActiveBooking)
                throw new InvalidOperationException("Cannot delete vehicle with active bookings (Pending or Confirmed)");

            // 3) Unlink tất cả batteries đang gắn với vehicle này
            var attachedBatteries = await context.Batteries
                .Where(b => b.VehicleId == vehicleId)
                .ToListAsync();

            foreach (var battery in attachedBatteries)
            {
                // Unlink battery khỏi vehicle
                battery.VehicleId = null;

                // Cập nhật status về Available nếu đang InUse
                if (battery.Status == BatteryStatus.InUse)
                {
                    battery.Status = BatteryStatus.Available;
                }

                // Xử lý theo Owner:
                if (battery.Owner == BatteryOwner.Driver)
                {
                    // Battery của Driver: Remove ownership, để orphan hoặc về depot
                    battery.UserId = null;
                    battery.StationId = null; // Hoặc gán về 1 depot/warehouse station nào đó

                    // Optional: Có thể chuyển ownership về Station
                    // battery.Owner = BatteryOwner.Station;
                }
                else // BatteryOwner.Station
                {
                    // Battery của Station: Giữ nguyên StationId, battery sẽ về lại station
                    // StationId đã có sẵn, không cần set lại
                    // UserId set về null vì không còn gắn với driver nào
                    battery.UserId = null;
                }

                battery.UpdatedAt = DateTime.UtcNow;
                context.Batteries.Update(battery);
            }

            // 4) Lưu thông tin vehicle trước khi xóa để return response
            var vehicleResponse = new VehicleResponse
            {
                VehicleId = vehicle.VehicleId,
                UserId = vehicle.UserId,
                UserName = vehicle.User.FullName,
                BatteryTypeId = vehicle.BatteryTypeId,
                BatteryTypeName = vehicle.BatteryType.BatteryTypeName,
                VBrand = vehicle.VBrand,
                Model = vehicle.Model,
                LicensePlate = vehicle.LicensePlate,
                BatteryId = string.Empty
            };

            // 5) Xóa vehicle
            context.Vehicles.Remove(vehicle);

            // 6) Save changes
            await context.SaveChangesAsync();
            await tx.CommitAsync();

            return vehicleResponse;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // 3) GetAllVehiclesAsync (list page) - same idea when projecting
    public async Task<PaginationWrapper<List<VehicleResponse>, VehicleResponse>> GetAllVehiclesAsync(int page, int pageSize, string? search)
    {
        var query = context.Vehicles.AsQueryable();
        if (search is not null) query = query.Where(v => v.Model.Contains(search) || v.LicensePlate.Contains(search));
        var totalItems = await query.CountAsync();

        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => new VehicleResponse
            {
                UserId = v.UserId,
                UserName = v.User.FullName,
                BatteryTypeId = v.BatteryTypeId,
                BatteryTypeName = v.BatteryType.BatteryTypeName,
                VBrand = v.VBrand,
                Model = v.Model,
                LicensePlate = v.LicensePlate,
                BatteryId = context.Batteries
                            .Where(b => b.VehicleId == v.VehicleId && b.Status == BatteryStatus.InUse)
                            .OrderByDescending(b => b.UpdatedAt ?? b.CreatedAt)
                            .Select(b => b.BatteryId)
                            .FirstOrDefault()
            }).ToListAsync();

        return new PaginationWrapper<List<VehicleResponse>, VehicleResponse>(users, totalItems, page, pageSize);
    }

    // GetVehiclesByUserAsync (projection với subquery để lấy BatteryId cho mỗi vehicle)
    // 2) Get vehicles by user (paged): use correlated subquery inside projection
    public async Task<PaginationWrapper<List<VehicleResponse>, VehicleResponse>> GetVehiclesByUserAsync(string userId, int page, int pageSize, string? search)
    {
        var query = context.Vehicles.AsNoTracking().AsQueryable();

        query = search is not null
            ? query.Where(v => v.UserId.Equals(userId) && (v.Model.Contains(search) || v.LicensePlate.Contains(search)))
            : query.Where(v => v.UserId.Equals(userId));

        var totalItems = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => new VehicleResponse
            {
                VehicleId = v.VehicleId,
                UserId = v.UserId,
                UserName = v.User.FullName,
                BatteryTypeId = v.BatteryTypeId,
                BatteryTypeName = v.BatteryType.BatteryTypeName,
                VBrand = v.VBrand,
                Model = v.Model,
                LicensePlate = v.LicensePlate,
                // correlated subquery -> EF sẽ chuyển thành efficient SQL subquery/LEFT JOIN
                BatteryId = context.Batteries
                            .Where(b => b.VehicleId == v.VehicleId && b.Status == BatteryStatus.InUse)
                            .OrderByDescending(b => b.UpdatedAt ?? b.CreatedAt)
                            .Select(b => b.BatteryId)
                            .FirstOrDefault()
            })
            .ToListAsync();

        return new PaginationWrapper<List<VehicleResponse>, VehicleResponse>(items, totalItems, page, pageSize);
    }
    private async Task ValidateVehicleRequest(VehicleRequest vehicleRequest)
    {
        if (!await context.BatteryTypes.AnyAsync(bt => bt.BatteryTypeId == vehicleRequest.BatteryTypeId))
            throw new ValidationException
            {
                ErrorMessage = "Battery type not found",
                Code = "400",
                StatusCode = HttpStatusCode.BadRequest
            };

        if (await context.Vehicles.AnyAsync(v => v.LicensePlate == vehicleRequest.LicensePlate))
            throw new ValidationException
            {
                ErrorMessage = "LicensePlate is already existed in our system",
                Code = "400",
                StatusCode = HttpStatusCode.BadRequest
            };
    }
}