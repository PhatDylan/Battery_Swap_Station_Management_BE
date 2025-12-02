using BusinessObject;
using BusinessObject.DTOs;
using BusinessObject.Entities;
using BusinessObject.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Exceptions;
using System.Net;

namespace Service.Implementations
{
    public partial class PaymentManagementService(ApplicationDbContext context, IHttpContextAccessor accessor) : IPaymentManagementService
    {
 
        public async Task<PaginationWrapper<List<PaymentManagementResponse>, PaymentManagementResponse>> GetAllPaymentsAsync(
            int page,
            int pageSize,
            string? search = null)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = context.Payments
                .Include(p => p.User)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(p =>
                    p.User.FullName.ToLower().Contains(term) ||
                    p.OrderCode.ToLower().Contains(term) ||
                    p.User.Email.ToLower().Contains(term) ||
                    p.PayId.ToLower().Contains(term));
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PaymentManagementResponse
                {
                    PayId = p.PayId,
                    UserId = p.UserId,
                    FullName = p.User.FullName,
                    AvatarUrl = p.User.AvatarUrl,
                    OrderCode = p.OrderCode,
                    Amount = p.Amount,
                    Currency = p.Currency,
                    BookingId = p.BookingId,
                    PaymentMethod = p.PaymentMethod.ToString(),
                    Status = p.Status.ToString(),
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return new PaginationWrapper<List<PaymentManagementResponse>, PaymentManagementResponse>(
                items, page, totalItems, pageSize);
        }

        /// <summary>
        /// Get payments by station (only completed battery swaps)
        /// </summary>
        public async Task<PaginationWrapper<List<StationPaymentResponse>, StationPaymentResponse>> GetPaymentsByStationAsync(
            string stationId,
            int page,
            int pageSize,
            string? search = null)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            // Validate station exists
            var stationExists = await context.Stations.AnyAsync(s => s.StationId == stationId);
            if (!stationExists)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Code = "404",
                    ErrorMessage = "Station not found"
                };

            // Get completed battery swaps for the station
            var query = context.BatterySwaps
                .Include(bs => bs.Payment)
                .ThenInclude(p => p.User)
                .Where(bs => bs.StationId == stationId && bs.Status == BBRStatus.Completed)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(bs =>
                    bs.Payment.User.FullName.ToLower().Contains(term) ||
                    bs.Payment.OrderCode.ToLower().Contains(term) ||
                    bs.Payment.User.Email.ToLower().Contains(term) ||
                    bs.Payment.PayId.ToLower().Contains(term) ||
                    bs.SwapId.ToLower().Contains(term));
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(bs => bs.Payment.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(bs => new StationPaymentResponse
                {
                    PayId = bs.Payment.PayId,
                    UserId = bs.Payment.UserId,
                    FullName = bs.Payment.User.FullName,
                    AvatarUrl = bs.Payment.User.AvatarUrl,
                    OrderCode = bs.Payment.OrderCode,
                    Amount = bs.Payment.Amount,
                    Currency = bs.Payment.Currency,
                    BookingId = bs.Payment.BookingId,
                    PaymentMethod = bs.Payment.PaymentMethod.ToString(),
                    Status = bs.Payment.Status.ToString(),
                    CreatedAt = bs.Payment.CreatedAt,
                    StationId = bs.StationId,
                    SwapId = bs.SwapId
                })
                .ToListAsync();

            return new PaginationWrapper<List<StationPaymentResponse>, StationPaymentResponse>(
                items, page, totalItems, pageSize);
        }
    }
}