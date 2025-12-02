using BusinessObject;
using BusinessObject.DTOs;
using BusinessObject.Enums;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;

namespace Service.Implementations;

public class StaffInventoryBatteryService(ApplicationDbContext context) : IStaffInventoryBatteryService
{
    public async Task<BatteryInventorySummaryResponse> GetSummaryAsync(BatteryInventorySummaryRequest request)
    {
        var stationId = request.StationId;

        var baseQuery = context.Batteries.AsNoTracking().Where(b => b.StationId == stationId);

        var total = await baseQuery.CountAsync();
        var available = await baseQuery.CountAsync(b => b.Status == BatteryStatus.Available);
        var charging = await baseQuery.CountAsync(b => b.Status == BatteryStatus.Charging);
        var maintenance = await baseQuery.CountAsync(b => b.Status == BatteryStatus.Maintenance);

        return new BatteryInventorySummaryResponse
        {
            Total = total,
            Available = available,
            Charging = charging,
            Maintenance = maintenance
        };
    }

    public async Task<PaginationWrapper<List<BatteryInventoryItemResponse>, BatteryInventoryItemResponse>> SearchAsync(
        BatteryInventorySearchRequest request)
    {
        var query = context.Batteries
            .AsNoTracking()
            .Include(b => b.BatteryType)
            .Include(b => b.Vehicle)
            .Where(b => b.StationId == request.StationId);

        if (request.Status.HasValue)
            query = query.Where(b => b.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Model))
            query = query.Where(b => b.Vehicle != null && b.Vehicle.Model.Contains(request.Model));

        if (!string.IsNullOrWhiteSpace(request.BatteryTypeId))
            query = query.Where(b => b.BatteryTypeId == request.BatteryTypeId);

        if (request.CapacityMinWh.HasValue)
            query = query.Where(b => b.CapacityWh >= request.CapacityMinWh.Value);

        if (request.CapacityMaxWh.HasValue)
            query = query.Where(b => b.CapacityWh <= request.CapacityMaxWh.Value);

        query = query.OrderByDescending(b => b.UpdatedAt ?? b.CreatedAt);

        var totalItems = await query.CountAsync();

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => new BatteryInventoryItemResponse
            {
                BatteryId = b.BatteryId,
                SerialNo = b.SerialNo,
                StationId = b.StationId ?? string.Empty,
                BatteryTypeId = b.BatteryTypeId,
                BatteryTypeName = b.BatteryType.BatteryTypeName,
                CapacityWh = b.CapacityWh,
                CurrentCapacityWh = b.CurrentCapacityWh,
                VehicleModel = b.Vehicle != null ? b.Vehicle.Model : null,
                Status = b.Status,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            })
            .ToListAsync();

        return new PaginationWrapper<List<BatteryInventoryItemResponse>, BatteryInventoryItemResponse>(
            items, totalItems, request.Page, request.PageSize);
    }
}