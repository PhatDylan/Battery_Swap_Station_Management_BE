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

namespace Service.Implementations;

public class BatteryService(ApplicationDbContext context, IHttpContextAccessor accessor) : IBatteryService
{
    public async Task<BatteryResponse> GetByBatteryAsync(string id)
    {
        var battery = await context.Batteries
            .Include(b => b.Station)
            .Include(b => b.BatteryType)
            .FirstOrDefaultAsync(b => b.BatteryId == id);

        if (battery is null)
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.NotFound,
                Code = "404",
                ErrorMessage = "Battery not found."
            };
        return ToResponse(battery);
    }

    public async Task<BatteryResponse> GetBySerialAsync(int serialNo)
    {
        var battery = await context.Batteries
            .Include(b => b.Station)
            .Include(b => b.BatteryType)
            .FirstOrDefaultAsync(b => b.SerialNo == serialNo);

        if (battery is null)
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.NotFound,
                Code = "404",
                ErrorMessage = "Battery not found."
            };
        return ToResponse(battery);
    }


    public async Task<PaginationWrapper<List<BatteryResponse>, BatteryResponse>> GetByStationAsync(string stationId,
        int page, int pageSize, string? search)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;

        var query = context.Batteries
            .Include(b => b.Station)
            .Include(b => b.BatteryType)
            .AsQueryable();

        query = query.Where(b => b.StationId == stationId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(b =>
                b.Station != null && (b.SerialNo.ToString().Contains(term) ||
                                      b.Station.Name.Contains(term) ||
                                      b.BatteryType.BatteryTypeName.Contains(term))
            );
        }

        var totalItems = await query.CountAsync();

        var data = await query
            .OrderByDescending(b => b.UpdatedAt ?? b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var responses = data.Select(ToResponse).ToList();

        return new PaginationWrapper<List<BatteryResponse>, BatteryResponse>(responses, totalItems, page, pageSize);
    }

    public async Task<PaginationWrapper<List<BatteryResponse>, BatteryResponse>> GetAllBatteryInStorage(
        string stationId, int page, int pageSize, string? search)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;

        var query = context.Batteries
            .Include(b => b.Station)
            .Include(b => b.BatteryType)
            .AsQueryable();

        query = query.Where(b => b.StationId == stationId && b.StationBatterySlot == null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(b =>
                b.Station != null && (b.SerialNo.ToString().Contains(term) ||
                                      b.Station.Name.Contains(term) ||
                                      b.BatteryType.BatteryTypeName.Contains(term))
            );
        }

        var totalItems = await query.CountAsync();

        var data = await query
            .OrderByDescending(b => b.UpdatedAt ?? b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var responses = data.Select(ToResponse).ToList();

        return new PaginationWrapper<List<BatteryResponse>, BatteryResponse>(responses, totalItems, page, pageSize);
    }

    public async Task<BatteryResponse> GetAvailableAsync(string? stationId = null)
    {
        var query = context.Batteries
            .Include(b => b.Station)
            .Include(b => b.BatteryType)
            .Where(b => b.Status == BatteryStatus.Available);

        if (!string.IsNullOrEmpty(stationId))
            query = query.Where(b => b.StationId == stationId);

        var battery = await query
            .OrderByDescending(b => b.UpdatedAt ?? b.CreatedAt)
            .FirstOrDefaultAsync();

        if (battery == null)
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.NotFound,
                Code = "404",
                ErrorMessage = "No available battery found."
            };

        return ToResponse(battery);
    }

    public async Task AddAsync(BatteryRequest request)
    {
        var exists = await context.Batteries.AnyAsync(b => b.SerialNo == request.SerialNo);
        if (exists)
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.BadRequest,
                Code = "400",
                ErrorMessage = "Battery with the same SerialNo already exists."
            };

        var userId = JwtUtils.GetUserId(accessor);

        if (string.IsNullOrEmpty(userId))
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.Unauthorized,
                ErrorMessage = "Unauthorized",
                Code = "401"
            };

        if (!await context.Users.AnyAsync(u => u.UserId == userId && u.Role == UserRole.Admin))
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.Forbidden,
                ErrorMessage = "Only admin can create batteries",
                Code = "403"
            };


        var entity = new Battery
        {
            BatteryId = Guid.NewGuid().ToString(),
            SerialNo = request.SerialNo,
            Owner = request.Owner,
            Status = request.Status,
            Voltage = request.Voltage,
            CapacityWh = request.CapacityWh,
            CurrentCapacityWh = request.CapacityWh,
            ImageUrl = request.ImageUrl,
            StationId = request.StationId,
            BatteryTypeId = request.BatteryTypeId ?? string.Empty,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        context.Batteries.Add(entity);
        await context.SaveChangesAsync();
    }

    public async Task AddBatteryToStation(IEnumerable<BatteryAddBulkStationRequest> requests)
    {
        var userId = JwtUtils.GetUserId(accessor);
        if (string.IsNullOrEmpty(userId))
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.Unauthorized,
                ErrorMessage = "Unauthorized",
                Code = "401"
            };

        var batteryAddBulkStationRequests = requests.ToList();
        if (requests == null || batteryAddBulkStationRequests.Count == 0)
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.BadRequest,
                ErrorMessage = "Request list cannot be empty.",
                Code = "400"
            };

        var allBatteryIds = batteryAddBulkStationRequests.SelectMany(r => r.BatteryIds).Distinct().ToList();
        var allStationIds = batteryAddBulkStationRequests.Select(r => r.StationId).Distinct().ToList();
        
        var existingStationIds = await context.Stations
            .Where(s => allStationIds.Contains(s.StationId))
            .Select(s => s.StationId)
            .ToListAsync();

        var missingStations = allStationIds.Except(existingStationIds).ToList();
        if (missingStations.Count != 0)
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.NotFound,
                ErrorMessage = $"Stations not found: {string.Join(", ", missingStations)}",
                Code = "404"
            };

        var allBatteries = await context.Batteries
            .Where(b => allBatteryIds.Contains(b.BatteryId))
            .ToListAsync();

        var foundIds = allBatteries.Select(b => b.BatteryId).ToHashSet();
        var missingIds = allBatteryIds.Except(foundIds).ToList();
        if (missingIds.Count != 0)
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.NotFound,
                ErrorMessage = $"Batteries not found: {string.Join(", ", missingIds)}",
                Code = "404"
            };
        
        var notOwned = allBatteries.Where(b => b.UserId != userId).Select(b => b.BatteryId).ToList();
        if (notOwned.Count != 0)
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.Forbidden,
                ErrorMessage = $"You do not own these batteries: {string.Join(", ", notOwned)}",
                Code = "403"
            };
        
        var assignedElsewhere = allBatteries
            .Where(b => !string.IsNullOrEmpty(b.StationId) && !allStationIds.Contains(b.StationId))
            .Select(b => b.BatteryId)
            .ToList();

        if (assignedElsewhere.Count != 0)
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.Conflict,
                ErrorMessage = $"Already assigned to another station: {string.Join(", ", assignedElsewhere)}",
                Code = "409"
            };
        
        var stationMap = batteryAddBulkStationRequests.ToDictionary(r => r.StationId, r => r.BatteryIds.ToHashSet());
        var now = DateTime.UtcNow;

        foreach (var battery in allBatteries)
        foreach (var kvp in stationMap.Where(kvp => kvp.Value.Contains(battery.BatteryId)))
        {
            battery.StationId = kvp.Key;
            battery.Owner = BatteryOwner.Station;
            battery.Status = BatteryStatus.Available;
            battery.UpdatedAt = now;
            break;
        }

        context.Batteries.UpdateRange(allBatteries);
        await context.SaveChangesAsync();
    }


    public async Task UpdateAsync(BatteryRequest request)
    {
        var battery = await context.Batteries
            .FirstOrDefaultAsync(b => b.SerialNo == request.SerialNo);

        if (battery == null)
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.NotFound,
                Code = "404",
                ErrorMessage = "Battery not found."
            };
        var userId = JwtUtils.GetUserId(accessor);
        if (string.IsNullOrEmpty(userId))
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.Unauthorized,
                ErrorMessage = "Unauthorized",
                Code = "401"
            };

        if (!await context.Users.AnyAsync(u => u.UserId == userId && u.Role == UserRole.Admin))
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.Forbidden,
                ErrorMessage = "Only admin can create batteries",
                Code = "403"
            };
        battery.Owner = request.Owner;
        battery.Status = request.Status;
        battery.Voltage = request.Voltage;
        battery.CapacityWh = request.CapacityWh;
        battery.ImageUrl = request.ImageUrl;
        battery.StationId = request.StationId;
        battery.BatteryTypeId = request.BatteryTypeId ?? battery.BatteryTypeId;
        battery.UserId = userId;
        battery.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var battery = await context.Batteries.FirstOrDefaultAsync(b => b.BatteryId == id);
        if (battery == null)
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.NotFound,
                Code = "404",
                ErrorMessage = "Battery not found."
            };

        context.Batteries.Remove(battery);
        await context.SaveChangesAsync();
    }

    public async Task<PaginationWrapper<List<BatteryResponse>, BatteryResponse>> GetAllBatteriesAsync(
        int page, int pageSize, string? search)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;

        var query = context.Batteries
            .Include(b => b.Station)
            .Include(b => b.BatteryType)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(b =>
                b.Station != null && (b.SerialNo.ToString().Contains(term) ||
                                      b.Station.Name.Contains(term) ||
                                      b.BatteryType.BatteryTypeName.Contains(term))
            );
        }

        var totalItems = await query.CountAsync();

        var data = await query
            .OrderByDescending(b => b.UpdatedAt ?? b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var responses = data.Select(ToResponse).ToList();

        return new PaginationWrapper<List<BatteryResponse>, BatteryResponse>(responses, totalItems, page, pageSize);
    }

    public async Task<List<BatteryResponse>> GetUnassignedAndNotInStationAsync(string? batteryTypeId)
    {
        // Build query so we can add optional filters
        var query = context.Batteries
            .Include(b => b.BatteryType)   // <-- thêm include để navigation được load
            .Include(b => b.Station)       // optional: nếu muốn StationName không null
            .AsQueryable();

        // Unassigned: not attached to any vehicle AND not in any station
        query = query.Where(b => b.VehicleId == null && (b.StationId == null || b.StationId == ""));

        // Optional: filter by battery type if provided
        if (!string.IsNullOrWhiteSpace(batteryTypeId))
        {
            query = query.Where(b => b.BatteryTypeId == batteryTypeId);
        }

        var batteries = await query.ToListAsync();

        return batteries.Select(ToResponse).ToList();
    }
    
    public async Task<PaginationWrapper<List<BatteryResponse>, BatteryResponse>> GetBatteryAssignedByStationIdAsync(string stationId, int page, int pageSize, string? search)
    {
        var query = context.Batteries
            .Include(b => b.BatteryType)
            .Include(b => b.Station)
            .AsQueryable();
        
        query = query.Where(b => b.VehicleId == null && b.StationId == stationId);;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(b =>
                b.Station != null && (b.SerialNo.ToString().Contains(term) ||
                                      b.Station.Name.Contains(term) ||
                                      b.BatteryType.BatteryTypeName.Contains(term))
            );
        }

        var totalItems = await query.CountAsync();

        var data = await query
            .OrderByDescending(b => b.UpdatedAt ?? b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var responses = data.Select(ToResponse).ToList();

        return new PaginationWrapper<List<BatteryResponse>, BatteryResponse>(responses, totalItems, page, pageSize);
    }

    // 2) Gắn battery vào vehicle (attach)
    public async Task AttachBatteryToVehicleAsync(BatteryAttachRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.BatteryId))
            throw new ArgumentException("BatteryId is required");
        if (string.IsNullOrWhiteSpace(request.VehicleId))
            throw new ArgumentException("VehicleId is required");

        await using var tx = await context.Database.BeginTransactionAsync();
        try
        {
            var battery = await context.Batteries
                .FirstOrDefaultAsync(b => b.BatteryId == request.BatteryId);

            if (battery == null)
                throw new ArgumentException("Battery not found");

            // Kiểm tra battery không đang ở station
            if (!string.IsNullOrEmpty(battery.StationId))
                throw new InvalidOperationException("Battery is currently in a station and cannot be attached directly to vehicle");

            // Kiểm tra battery chưa có vehicle
            if (!string.IsNullOrEmpty(battery.VehicleId))
                throw new InvalidOperationException("Battery already attached to a vehicle");

            var vehicle = await context.Vehicles
                .FirstOrDefaultAsync(v => v.VehicleId == request.VehicleId);

            if (vehicle is null)
                throw new ArgumentException("Vehicle not found");

            // Kiểm tra vehicle hiện có battery đang InUse (để tránh gán 2 pin đang dùng cho 1 xe)
            var vehicleHasInUseBattery = await context.Batteries
                .AnyAsync(b => b.VehicleId == vehicle.VehicleId && b.Status == BatteryStatus.InUse);
            if (vehicleHasInUseBattery)
                throw new InvalidOperationException("Vehicle already has an in-use battery");

            // Kiểm tra loại pin tương thích
            if (!string.Equals(vehicle.BatteryTypeId, battery.BatteryTypeId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Battery type is not compatible with vehicle");

            // Thực hiện cập nhật trên Battery (không cập nhật Vehicle vì entity Vehicle không có field lưu id pin)
            battery.VehicleId = vehicle.VehicleId;
            battery.StationId = null;
            battery.Status = BatteryStatus.InUse;
            battery.UserId = vehicle.UserId; // nếu muốn chuyển quyền sở hữu về chủ xe
            battery.UpdatedAt = DateTime.UtcNow;
            
            context.Batteries.Update(battery);

            await context.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
    private static BatteryResponse ToResponse(Battery b)
    {
        return new BatteryResponse
        {
            BatteryId = b.BatteryId,
            SerialNo = b.SerialNo,
            Owner = b.Owner,
            Status = b.Status,
            Voltage = b.Voltage ?? "12.7 V",
            CapacityWh = b.CapacityWh,
            ImageUrl = b.ImageUrl,
            StationId = b.StationId,
            StationName = b.Station?.Name,
            BatteryTypeId = b.BatteryTypeId,
            BatteryTypeName = b.BatteryType?.BatteryTypeName,
            UserId = b.UserId,
            CreatedAt = b.CreatedAt,
            UpdatedAt = b.UpdatedAt
        };
    }
}