using BusinessObject;
using BusinessObject.Dtos;
using BusinessObject.Entities;
using BusinessObject.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;

namespace Service.Implementations;

public class AdminManagementBatteryService(ApplicationDbContext context, IHttpContextAccessor accessor) : IAdminManagementBatteryService
{
    public async Task<CountBatteryAdminDto> GetPeakSwapReportAsync(CancellationToken cancellationToken = default)
    {
        var baseQuery = context.Set<BatterySwap>().AsNoTracking()
            .Where(x => x.Status == BBRStatus.Completed);
        
        var byDayRaw = await baseQuery
            .GroupBy(s => new { s.SwappedAt.Year, s.SwappedAt.Month, s.SwappedAt.Day })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                g.Key.Day,
                Total = g.Count(),
                Peak = g.Count(s =>
                    (s.SwappedAt.Hour > 4 && s.SwappedAt.Hour < 8) ||
                    (s.SwappedAt.Hour == 4 && s.SwappedAt.Minute >= 30))
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.Day)
            .ToListAsync(cancellationToken);

        var byDay = byDayRaw.Select(x => new CountBatterySwapDto
        {
            Time = $"{x.Day:D2}/{x.Month:D2}/{x.Year}",
            TotalCountPerDay = x.Total,
            RushHourCount = x.Peak
        }).ToList();
        
        var byMonthRaw = await baseQuery
            .GroupBy(s => new { s.SwappedAt.Year, s.SwappedAt.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Total = g.Count(),
                Peak = g.Count(s =>
                    (s.SwappedAt.Hour > 4 && s.SwappedAt.Hour < 8) ||
                    (s.SwappedAt.Hour == 4 && s.SwappedAt.Minute >= 30))
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync(cancellationToken);

        var byMonth = byMonthRaw.Select(x => new CountBatterySwapDto
        {
            Time = $"{x.Month:D2}/{x.Year}",
            TotalCountPerDay = x.Total,
            RushHourCount = x.Peak
        }).ToList();
        
        var byYearRaw = await baseQuery
            .GroupBy(s => s.SwappedAt.Year)
            .Select(g => new
            {
                Year = g.Key,
                Total = g.Count(),
                Peak = g.Count(s =>
                    (s.SwappedAt.Hour > 4 && s.SwappedAt.Hour < 8) ||
                    (s.SwappedAt.Hour == 4 && s.SwappedAt.Minute >= 30))
            })
            .OrderBy(x => x.Year)
            .ToListAsync(cancellationToken);

        var byYear = byYearRaw.Select(x => new CountBatterySwapDto
        {
            Time = $"{x.Year}",
            TotalCountPerDay = x.Total,
            RushHourCount = x.Peak
        }).ToList();

        return new CountBatteryAdminDto
        {
            ByDay = byDay,
            ByMonth = byMonth,
            ByYear = byYear
        };
    }
}