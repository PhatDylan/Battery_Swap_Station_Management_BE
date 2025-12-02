using BusinessObject;
using BusinessObject.DTOs;
using BusinessObject.Entities;
using BusinessObject.Enums;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using System.Globalization;

namespace Service.Implementations;

public class AvailabilityService(ApplicationDbContext context) : IAvailabilityService
{
    public async Task<List<AvailableStaffResponse>> GetAvailableStaffAsync(AvailabilityQuery query)
    {
        var date = DateTime.SpecifyKind(DateTime.ParseExact(query.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture), DateTimeKind.Utc).Date;

        var absentIds = await context.Set<StaffAbsence>()
            .Where(a => a.Date == date)
            .Select(a => a.UserId)
            .ToListAsync();

        var overrideUserIds = await context.Set<StationStaffOverride>()
            .Where(o => o.Date == date && (query.ShiftId == null || o.ShiftId == query.ShiftId))
            .Select(o => o.UserId)
            .ToListAsync();

        var q = context.Users
            .Where(u => u.Role == UserRole.Staff &&
                        !absentIds.Contains(u.UserId) &&
                        !overrideUserIds.Contains(u.UserId));

        if (!string.IsNullOrEmpty(query.StationId))
        {
  
            q = q.Where(u => context.StationStaffs.Any(ss => ss.UserId == u.UserId && ss.StationId == query.StationId));
        }

        return await q
            .Select(u => new AvailableStaffResponse
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email
            })
            .ToListAsync();
    }
}