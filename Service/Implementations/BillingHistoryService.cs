using BusinessObject;
using BusinessObject.DTOs;
using BusinessObject.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Service.Exceptions;
using Service.Interfaces;
using Service.Utils;
using System.Net;

namespace Service.Implementations;

public class BillingHistoryService(ApplicationDbContext context, IHttpContextAccessor accessor) : IBillingHistoryService
{
    // Payments
    public async Task<MyPaymentsListResponse> GetMyPaymentsAsync(MyPaymentsListRequest request)
    {
        var userId = JwtUtils.GetUserId(accessor);
        if (string.IsNullOrEmpty(userId))
            throw new ValidationException
            {
                ErrorMessage = "Unauthorized",
                StatusCode = HttpStatusCode.Unauthorized,
                Code = "401"
            };

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 || request.PageSize > 100 ? 10 : request.PageSize;

        var query = context.Payments
            .AsNoTracking()
            .Include(p => p.User)
            .Where(p => p.UserId == userId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim();
            query = query.Where(p =>
                p.OrderCode.Contains(s) ||
                p.BookingId.Contains(s) ||
                (p.User != null && p.User.FullName.Contains(s)));
        }

        if (request.Status.HasValue)
        {
            var st = request.Status.Value;
            query = query.Where(p => p.Status == st);
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PaymentResponse
            {
                PayId = p.PayId,
                UserId = p.UserId,
                UserName = p.User != null ? p.User.FullName : string.Empty,
                BookingId = p.BookingId,
                OrderCode = p.OrderCode,
                Amount = p.Amount,
                Currency = p.Currency,
                PaymentMethod = p.PaymentMethod,
                Status = p.Status,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return new MyPaymentsListResponse
        {
            Items = items,
            Pagination = new PaginationMeta
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                TotalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling((double)total / pageSize)
            }
        };
    }

    public async Task<PaymentResponse?> GetMyPaymentByIdAsync(string payId)
    {
        var userId = JwtUtils.GetUserId(accessor);
        if (string.IsNullOrEmpty(userId))
            throw new ValidationException
            {
                ErrorMessage = "Unauthorized",
                StatusCode = HttpStatusCode.Unauthorized,
                Code = "401"
            };

        var payment = await context.Payments
            .AsNoTracking()
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.PayId == payId && p.UserId == userId);

        if (payment == null) return null;

        return new PaymentResponse
        {
            PayId = payment.PayId,
            UserId = payment.UserId,
            UserName = payment.User != null ? payment.User.FullName : string.Empty,
            BookingId = payment.BookingId,
            OrderCode = payment.OrderCode,
            Amount = payment.Amount,
            Currency = payment.Currency,
            PaymentMethod = payment.PaymentMethod,
            Status = payment.Status,
            CreatedAt = payment.CreatedAt
        };
    }

    // Transaction history
    public async Task<TransactionHistoryListResponse> GetMyTransactionHistoryAsync(TransactionHistoryQueryRequest request)
    {
        var userId = JwtUtils.GetUserId(accessor);
        if (string.IsNullOrEmpty(userId))
            throw new ValidationException
            {
                ErrorMessage = "Unauthorized",
                StatusCode = HttpStatusCode.Unauthorized,
                Code = "401"
            };

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 || request.PageSize > 100 ? 10 : request.PageSize;

        // Payments của tôi
        var paymentQ = context.Payments
            .AsNoTracking()
            .Where(p => p.UserId == userId);

        //Battery swaps của tôi (kèm Payment để lấy BookingId/Amount/Currency, kèm Station để lấy tên)
        var swapQ = context.BatterySwaps
            .AsNoTracking()
            .Include(bs => bs.Station)
            .Include(bs => bs.Payment)
            .Where(bs => bs.UserId == userId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim();
            paymentQ = paymentQ.Where(p => p.OrderCode.Contains(s) || p.BookingId.Contains(s));
            swapQ = swapQ.Where(bs =>
                (bs.SwapId != null && bs.SwapId.Contains(s)) ||
                (bs.Station != null && bs.Station.Name.Contains(s)) ||
                (bs.Payment != null && bs.Payment.OrderCode.Contains(s)));
        }

        var payments = await paymentQ
            .Select(p => new TransactionHistoryItem
            {
                Id = p.PayId,
                Kind = TransactionKind.Payment,
                OccurredAt = p.CreatedAt,
                Amount = p.Amount,
                Currency = p.Currency,
                ReferenceCode = p.OrderCode,
                BookingId = p.BookingId,
                StationName = null,
                Status = p.Status.ToString()
            })
            .ToListAsync();

        var swaps = await swapQ
            .Select(bs => new TransactionHistoryItem
            {
                Id = bs.SwapId,
                Kind = TransactionKind.BatterySwap,
                OccurredAt = bs.SwappedAt,
                Amount = bs.Payment != null ? bs.Payment.Amount : 0,
                Currency = bs.Payment != null ? bs.Payment.Currency : "VND",
                ReferenceCode = bs.SwapId,
                BookingId = bs.Payment != null ? bs.Payment.BookingId : null,
                StationName = bs.Station != null ? bs.Station.Name : null,
                Status = bs.Status.ToString()
            })
            .ToListAsync();

        var combined = (request.Kind switch
        {
            TransactionKind.Payment => payments,
            TransactionKind.BatterySwap => swaps,
            _ => payments.Concat(swaps).ToList()
        }).OrderByDescending(x => x.OccurredAt).ToList();

        var total = combined.Count;
        var items = combined.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new TransactionHistoryListResponse
        {
            Items = items,
            Pagination = new PaginationMeta
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                TotalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling((double)total / pageSize)
            }
        };
    }
}