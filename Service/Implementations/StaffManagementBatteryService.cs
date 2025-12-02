using System.Net;
using BusinessObject;
using BusinessObject.DTOs;
using BusinessObject.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Service.Exceptions;
using Service.Interfaces;
using Service.Utils;

namespace Service.Implementations
{
    public class StaffManagementBatteryService(ApplicationDbContext context, IHttpContextAccessor accessor) : IStaffManagementBatteryService
    {
        public async Task<PaginationWrapper<List<StaffSwapListItemResponse>, StaffSwapListItemResponse>> GetMyStationSwapsAsync(
            int page, int pageSize, string? stationId, BBRStatus? status, string? search, CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var staffUserId = JwtUtils.GetUserId(accessor);
            if (string.IsNullOrEmpty(staffUserId))
            {
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Code = "401",
                    ErrorMessage = "Unauthorized"
                };
            }

            // Lấy danh sách trạm mà staff đang thuộc
            var myStationIds = await context.StationStaffs
                .Where(ss => ss.UserId == staffUserId)
                .Select(ss => ss.StationId)
                .Distinct()
                .ToListAsync(ct);

            if (myStationIds.Count == 0)
            {
                return new PaginationWrapper<List<StaffSwapListItemResponse>, StaffSwapListItemResponse>(
                    new List<StaffSwapListItemResponse>(), 0, page, pageSize);
            }

            // Nếu truyền stationId thì phải nằm trong myStationIds
            if (!string.IsNullOrEmpty(stationId) && !myStationIds.Contains(stationId))
            {
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    Code = "403",
                    ErrorMessage = "You are not assigned to this station."
                };
            }

            var scope = string.IsNullOrEmpty(stationId) ? myStationIds : new List<string> { stationId };

            var query = context.BatterySwaps
                .AsNoTracking()
                .Where(bs => scope.Contains(bs.StationId));

            if (status.HasValue)
                query = query.Where(bs => bs.Status == status.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(bs =>
                    bs.SwapId.Contains(term) ||
                    bs.BatteryId.Contains(term) ||
                    bs.ToBatteryId.Contains(term));
            }

            var totalItems = await query.CountAsync(ct);

            var data = await query
                .OrderByDescending(bs => bs.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(bs => new
                {
                    bs.SwapId,
                    bs.StationId,
                    bs.UserId,
                    bs.VehicleId,
                    bs.BatteryId,
                    bs.ToBatteryId,
                    bs.Status,
                    bs.CreatedAt
                })
                .ToListAsync(ct);

            // Tìm booking liên quan: cùng StationId + UserId + VehicleId, cùng ngày, gần CreatedAt nhất
            var stationIds = data.Select(d => d.StationId).Distinct().ToList();
            var userIds = data.Select(d => d.UserId).Distinct().ToList();
            var vehicleIds = data.Select(d => d.VehicleId).Distinct().ToList();

            var candidateBookings = await context.Bookings
                .AsNoTracking()
                .Where(b => stationIds.Contains(b.StationId)
                            && userIds.Contains(b.UserId)
                            && vehicleIds.Contains(b.VehicleId))
                .Select(b => new { b.BookingId, b.StationId, b.UserId, b.VehicleId, b.BookingTime })
                .ToListAsync(ct);

            var responses = (from s in data
                let booking = candidateBookings.Where(b => b.StationId == s.StationId && b.UserId == s.UserId && b.VehicleId == s.VehicleId && b.BookingTime.Date == s.CreatedAt.Date)
                    .OrderBy(b => Math.Abs((b.BookingTime - s.CreatedAt).TotalMinutes))
                    .FirstOrDefault()
                select new StaffSwapListItemResponse
                {
                    SwapId = s.SwapId,
                    StationId = s.StationId,
                    BookingId = booking?.BookingId,
                    BookingTime = booking?.BookingTime,
                    UserId = s.UserId,
                    VehicleId = s.VehicleId,
                    BatteryId = s.BatteryId,
                    ToBatteryId = s.ToBatteryId,
                    Status = s.Status,
                    CreatedAt = s.CreatedAt
                }).ToList();

            return new PaginationWrapper<List<StaffSwapListItemResponse>, StaffSwapListItemResponse>(
                responses, totalItems, page, pageSize);
        }

        public async Task RejectSwapAsync(string swapId, StaffRejectSwapRequest? request, CancellationToken ct = default)
        {
            // var staffUserId = JwtUtils.GetUserId(accessor);
            // if (string.IsNullOrEmpty(staffUserId))
            //     throw new ValidationException
            //     {
            //         StatusCode = HttpStatusCode.Unauthorized,
            //         Code = "401",
            //         ErrorMessage = "Unauthorized"
            //     };

            var swap = await context.BatterySwaps.FirstOrDefaultAsync(bs => bs.SwapId == swapId, ct);
            if (swap == null)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Code = "404",
                    ErrorMessage = "Battery swap not found"
                };

            // var stationAssigned = await context.StationStaffs
            //     .AnyAsync(ss => ss.UserId == staffUserId && ss.StationId == swap.StationId, ct);
            // if (!stationAssigned)
            //     throw new ValidationException
            //     {
            //         StatusCode = HttpStatusCode.Forbidden,
            //         Code = "403",
            //         ErrorMessage = "You are not assigned to this station."
            //     };

            if (swap.Status != BBRStatus.Pending)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400",
                    ErrorMessage = "Only pending swaps can be rejected"
                };

            swap.Status = BBRStatus.Cancelled; // 3
            if (!string.IsNullOrWhiteSpace(request?.Reason))
                swap.Reason = request!.Reason;

            await context.SaveChangesAsync(ct);
        }

        public async Task ConfirmSwapAsync(string swapId, CancellationToken ct = default)
        {
            // var staffUserId = JwtUtils.GetUserId(accessor);
            // if (string.IsNullOrEmpty(staffUserId))
            //     throw new ValidationException
            //     {
            //         StatusCode = HttpStatusCode.Unauthorized,
            //         Code = "401",
            //         ErrorMessage = "Unauthorized"
            //     };

            var swap = await context.BatterySwaps.FirstOrDefaultAsync(bs => bs.SwapId == swapId, ct);
            if (swap == null)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Code = "404",
                    ErrorMessage = "Battery swap not found"
                };

            // var stationAssigned = await context.StationStaffs
            //     .AnyAsync(ss => ss.UserId == staffUserId && ss.StationId == swap.StationId, ct);
            // if (!stationAssigned)
            //     throw new ValidationException
            //     {
            //         StatusCode = HttpStatusCode.Forbidden,
            //         Code = "403",
            //         ErrorMessage = "You are not assigned to this station."
            //     };

            if (swap.Status != BBRStatus.Pending)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400",
                    ErrorMessage = "Only pending swaps can be confirmed"
                };

            swap.Status = BBRStatus.Confirmed; // 2
            await context.SaveChangesAsync(ct);
        }
        
        public async Task CompleteSwapAsync(string swapId, CancellationToken ct = default)
        {
            // var staffUserId = JwtUtils.GetUserId(accessor);
            // if (string.IsNullOrEmpty(staffUserId))
            //     throw new ValidationException
            //     {
            //         StatusCode = HttpStatusCode.Unauthorized,
            //         Code = "401",
            //         ErrorMessage = "Unauthorized"
            //     };

            var swap = await context.BatterySwaps.FirstOrDefaultAsync(bs => bs.SwapId == swapId, ct);
            if (swap == null)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Code = "404",
                    ErrorMessage = "Battery swap not found"
                };

            // var stationAssigned = await context.StationStaffs
            //     .AnyAsync(ss => ss.UserId == staffUserId && ss.StationId == swap.StationId, ct);
            // if (!stationAssigned)
            //     throw new ValidationException
            //     {
            //         StatusCode = HttpStatusCode.Forbidden,
            //         Code = "403",
            //         ErrorMessage = "You are not assigned to this station."
            //     };

            if (swap.Status != BBRStatus.Confirmed)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400",
                    ErrorMessage = "Only pending swaps can be confirmed"
                };

            swap.Status = BBRStatus.Completed;
            await context.SaveChangesAsync(ct);
        }
    }
}