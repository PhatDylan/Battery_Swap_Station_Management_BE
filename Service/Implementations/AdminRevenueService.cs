using BusinessObject;
using BusinessObject.Dtos;
using BusinessObject.Enums;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;

namespace Service.Implementations
{
    public class AdminRevenueService(ApplicationDbContext context) : IAdminRevenueService
    {
        public async Task<RevenueReportDto> GetRevenueReportAsync(CancellationToken cancellationToken = default)
        {
            // ===== DOANH THU TỪ PAYMENT (người không có gói subscription - trả tiền khi swap) =====
            // Chỉ lấy Payment có Status = Completed và PaymentMethod != Subscription_Plan
            var swapPaymentsQuery = context.Payments
                .AsNoTracking()
                .Where(p => p.Status == PayStatus.Completed &&
                           p.PaymentMethod != PayMethod.Subscription_Plan);

            // ===== DOANH THU TỪ SUBSCRIPTION PAYMENT (người mua gói thẻ tháng) =====
            var subscriptionPaymentsQuery = context.SubscriptionPayments
                .AsNoTracking()
                .Where(sp => sp.Status == PayStatus.Completed);

            // ============= BÁO CÁO THEO NGÀY =============
            var revenueByDaySwap = await swapPaymentsQuery
                .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month, p.CreatedAt.Day })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    g.Key.Day,
                    Revenue = g.Sum(p => p.Amount),
                    Count = g.Count()
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.Day)
                .ToListAsync(cancellationToken);

            var revenueByDaySubscription = await subscriptionPaymentsQuery
                .GroupBy(sp => new { sp.CreatedAt.Year, sp.CreatedAt.Month, sp.CreatedAt.Day })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    g.Key.Day,
                    Revenue = g.Sum(sp => sp.Amount),
                    Count = g.Count()
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.Day)
                .ToListAsync(cancellationToken);

            // Kết hợp dữ liệu theo ngày
            var allDays = revenueByDaySwap.Select(x => new { x.Year, x.Month, x.Day })
                .Union(revenueByDaySubscription.Select(x => new { x.Year, x.Month, x.Day }))
                .Distinct()
                .OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.Day);

            var byDay = allDays.Select(date =>
            {
                var swapData = revenueByDaySwap.FirstOrDefault(x =>
                    x.Year == date.Year && x.Month == date.Month && x.Day == date.Day);
                var subData = revenueByDaySubscription.FirstOrDefault(x =>
                    x.Year == date.Year && x.Month == date.Month && x.Day == date.Day);

                var swapRevenue = swapData?.Revenue ?? 0;
                var subRevenue = subData?.Revenue ?? 0;

                return new RevenueByPeriodDto
                {
                    Time = $"{date.Day:D2}/{date.Month:D2}/{date.Year}",
                    SwapPaymentRevenue = swapRevenue,
                    SubscriptionRevenue = subRevenue,
                    TotalRevenue = swapRevenue + subRevenue,
                    SwapPaymentCount = swapData?.Count ?? 0,
                    SubscriptionCount = subData?.Count ?? 0
                };
            }).ToList();

            // ============= BÁO CÁO THEO THÁNG =============
            var revenueByMonthSwap = await swapPaymentsQuery
                .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Revenue = g.Sum(p => p.Amount),
                    Count = g.Count()
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync(cancellationToken);

            var revenueByMonthSubscription = await subscriptionPaymentsQuery
                .GroupBy(sp => new { sp.CreatedAt.Year, sp.CreatedAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Revenue = g.Sum(sp => sp.Amount),
                    Count = g.Count()
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync(cancellationToken);

            var allMonths = revenueByMonthSwap.Select(x => new { x.Year, x.Month })
                .Union(revenueByMonthSubscription.Select(x => new { x.Year, x.Month }))
                .Distinct()
                .OrderBy(x => x.Year).ThenBy(x => x.Month);

            var byMonth = allMonths.Select(date =>
            {
                var swapData = revenueByMonthSwap.FirstOrDefault(x =>
                    x.Year == date.Year && x.Month == date.Month);
                var subData = revenueByMonthSubscription.FirstOrDefault(x =>
                    x.Year == date.Year && x.Month == date.Month);

                var swapRevenue = swapData?.Revenue ?? 0;
                var subRevenue = subData?.Revenue ?? 0;

                return new RevenueByPeriodDto
                {
                    Time = $"{date.Month:D2}/{date.Year}",
                    SwapPaymentRevenue = swapRevenue,
                    SubscriptionRevenue = subRevenue,
                    TotalRevenue = swapRevenue + subRevenue,
                    SwapPaymentCount = swapData?.Count ?? 0,
                    SubscriptionCount = subData?.Count ?? 0
                };
            }).ToList();

            // ============= BÁO CÁO THEO NĂM =============
            var revenueByYearSwap = await swapPaymentsQuery
                .GroupBy(p => p.CreatedAt.Year)
                .Select(g => new
                {
                    Year = g.Key,
                    Revenue = g.Sum(p => p.Amount),
                    Count = g.Count()
                })
                .OrderBy(x => x.Year)
                .ToListAsync(cancellationToken);

            var revenueByYearSubscription = await subscriptionPaymentsQuery
                .GroupBy(sp => sp.CreatedAt.Year)
                .Select(g => new
                {
                    Year = g.Key,
                    Revenue = g.Sum(sp => sp.Amount),
                    Count = g.Count()
                })
                .OrderBy(x => x.Year)
                .ToListAsync(cancellationToken);

            var allYears = revenueByYearSwap.Select(x => x.Year)
                .Union(revenueByYearSubscription.Select(x => x.Year))
                .Distinct()
                .OrderBy(x => x);

            var byYear = allYears.Select(year =>
            {
                var swapData = revenueByYearSwap.FirstOrDefault(x => x.Year == year);
                var subData = revenueByYearSubscription.FirstOrDefault(x => x.Year == year);

                var swapRevenue = swapData?.Revenue ?? 0;
                var subRevenue = subData?.Revenue ?? 0;

                return new RevenueByPeriodDto
                {
                    Time = $"{year}",
                    SwapPaymentRevenue = swapRevenue,
                    SubscriptionRevenue = subRevenue,
                    TotalRevenue = swapRevenue + subRevenue,
                    SwapPaymentCount = swapData?.Count ?? 0,
                    SubscriptionCount = subData?.Count ?? 0
                };
            }).ToList();

            return new RevenueReportDto
            {
                ByDay = byDay,
                ByMonth = byMonth,
                ByYear = byYear
            };
        }
    }
}