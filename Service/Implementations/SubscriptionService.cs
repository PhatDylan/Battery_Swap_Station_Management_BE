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
    // Implement đúng theo interface ISubscriptionService bạn cung cấp
    public class SubscriptionService(ApplicationDbContext context, IHttpContextAccessor accessor) : ISubscriptionService
    {
        public async Task<PaginationWrapper<List<SubscriptionResponse>, SubscriptionResponse>> GetAllSubscriptionAsync(
            int page, int pageSize, string? search)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = context.Subscriptions
                .Include(s => s.User)
                .Include(s => s.SubscriptionPlan)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(s =>
                    s.SubscriptionId.Contains(term) ||
                    s.UserId.Contains(term) ||
                    s.PlanId.Contains(term) ||
                    s.Status.ToString().Contains(term) ||
                    (s.User.FullName != null && s.User.FullName.Contains(term)) ||
                    (s.User.Email != null && s.User.Email.Contains(term)) ||
                    (s.SubscriptionPlan.Name != null && s.SubscriptionPlan.Name.Contains(term)));
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(s => s.Status == SubscriptionStatus.Active) // ưu tiên gói đang Active
                .ThenByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = items.Select(Map).ToList();

    
            return new PaginationWrapper<List<SubscriptionResponse>, SubscriptionResponse>(
                responses, page, totalItems, pageSize);
        }

        public async Task<SubscriptionResponse?> GetBySubscriptionAsync(string id)
        {
            var s = await context.Subscriptions
                .Include(x => x.User)
                .Include(x => x.SubscriptionPlan)
                .FirstOrDefaultAsync(x => x.SubscriptionId == id);

            return s is null ? null : Map(s);
        }

        // Interface trả về 1 bản ghi → chọn gói "đang Active" (nếu có), nếu không thì lấy gói mới nhất
        public async Task<SubscriptionResponse> GetByUserAsync(string userId)
        {
            var s = await context.Subscriptions
                .Include(x => x.User)
                .Include(x => x.SubscriptionPlan)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.Status == SubscriptionStatus.Active)
                .ThenByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync() ?? throw new ValidationException
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Code = "404",
                    ErrorMessage = "Subscription not found for this user."
                };
            return Map(s);
        }

        public async Task<SubscriptionResponse> CancelSubscription(string subscriptionId)
        {
            var userId = JwtUtils.GetUserId(accessor);
            if (userId is null)
            {
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    ErrorMessage = "Unauthorized",
                    Code = "401"
                };
            }
            
            var userEntity = context.Users.FirstOrDefault(u => u.UserId == userId && u.Status == UserStatus.Active);
            if (userEntity is null)
            {
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessage = "User not found",
                    Code = "400"
                };
            }
            var s = await context.Subscriptions
                .Include(x => x.User)
                .Include(x => x.SubscriptionPlan)
                .FirstOrDefaultAsync(x => x.SubscriptionId == subscriptionId);

            if (s is null)
            {
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessage = "Subscription not found",
                    Code = "404"
                };
            }

            switch (userEntity.Role)
            {
                case UserRole.Staff:
                    throw new ValidationException
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        ErrorMessage = "You cannot cancel subscription for staff",
                        Code = "400"
                    };
                case UserRole.Driver:
                    var planUser = s.UserId;
                    if (planUser != userId)
                    {
                        throw new ValidationException
                        {
                            StatusCode = HttpStatusCode.BadRequest,
                            ErrorMessage = "You cannot cancel subscription for other driver",
                            Code = "400"
                        };
                    }
                    break;
                case UserRole.Admin:
                default:
                    break;
            }
            
            s.Status = SubscriptionStatus.Cancelled;
            s.EndDate = DateTime.UtcNow;
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                context.Subscriptions.Update(s);
                await context.SaveChangesAsync();
                var payment = s.SubscriptionPlan;
                var refundAmount = Math.Round(CalculateRefundAmount(s.StartDate, s.NumberOfSwaps, payment.SwapAmount, payment), 2);
                if (userEntity.Role == UserRole.Driver)
                {
                    userEntity.Balance += refundAmount;
                    context.Users.Update(userEntity);
                }
                else
                {
                    var user = s.User;
                    user.Balance += refundAmount;
                    context.Users.Update(user);
                }
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Map(s);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessage = ex.Message,
                    Code = "500"
                };
            }
        }

        private double CalculateRefundAmount(DateTime startDate, int usedSwaps, int maxSwaps, SubscriptionPlan plan)
        {
            var cancelDate = DateTime.UtcNow;
            const int periodDays = 30;
            
            var usedDays = (cancelDate - startDate).TotalDays;
            var usageRatio = maxSwaps > 0 ? (double)usedSwaps / maxSwaps : 0;
            var timeRatio = usedDays / periodDays;
            
            var consumedPortion = Math.Clamp(Math.Max(usageRatio, timeRatio), 0, 1);
            
            var refundAmount = plan.MonthlyFee * (1 - consumedPortion);
            
            refundAmount = Math.Clamp(refundAmount, 0, plan.MonthlyFee);
            return refundAmount;
        }


        public async Task AddAsync(SubscriptionRequest request)
        {
            ValidateRequestDates(request.StartDate, request.EndDate);

            // Ensure user + plan tồn tại
            var userExists = await context.Users.AnyAsync(u => u.UserId == request.UserId);
            if (!userExists)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400",
                    ErrorMessage = "User does not exist."
                };

            var planExists = await context.SubscriptionPlans.AnyAsync(p => p.PlanId == request.PlanId);
            if (!planExists)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400",
                    ErrorMessage = "Subscription plan does not exist."
                };

            // Một user chỉ có 1 gói Active tại một thời điểm (tùy nghiệp vụ)
            if (request.Status == SubscriptionStatus.Active)
            {
                var hasActive = await context.Subscriptions.AnyAsync(s =>
                    s.UserId == request.UserId && s.Status == SubscriptionStatus.Active);
                if (hasActive)
                    throw new ValidationException
                    {
                        StatusCode = HttpStatusCode.Conflict,
                        Code = "409",
                        ErrorMessage = "User already has an active subscription."
                    };
            }

            var entity = new Subscription
            {
                SubscriptionId = Guid.NewGuid().ToString(),
                UserId = request.UserId,
                PlanId = request.PlanId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = request.Status,
                NumberOfSwaps = request.NumberOfSwap,
                CreatedAt = DateTime.UtcNow
            };

            context.Subscriptions.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SubscriptionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.SubscriptionId))
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400",
                    ErrorMessage = "SubscriptionId is required."
                };

            ValidateRequestDates(request.StartDate, request.EndDate);

            var entity = await context.Subscriptions.FirstOrDefaultAsync(s => s.SubscriptionId == request.SubscriptionId);
            if (entity is null)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Code = "404",
                    ErrorMessage = "Subscription not found."
                };

            // Nếu chuyển sang Active → đảm bảo không có gói Active khác của user
            if (request.Status == SubscriptionStatus.Active)
            {
                var hasAnotherActive = await context.Subscriptions.AnyAsync(s =>
                    s.UserId == entity.UserId &&
                    s.SubscriptionId != entity.SubscriptionId &&
                    s.Status == SubscriptionStatus.Active);
                if (hasAnotherActive)
                    throw new ValidationException
                    {
                        StatusCode = HttpStatusCode.Conflict,
                        Code = "409",
                        ErrorMessage = "User already has another active subscription."
                    };
            }

            // Kiểm tra plan tồn tại nếu đổi
            if (!string.Equals(entity.PlanId, request.PlanId, StringComparison.Ordinal))
            {
                var planExists = await context.SubscriptionPlans.AnyAsync(p => p.PlanId == request.PlanId);
                if (!planExists)
                    throw new ValidationException
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Code = "400",
                        ErrorMessage = "Subscription plan does not exist."
                    };
            }

            entity.PlanId = request.PlanId;
            entity.StartDate = request.StartDate;
            entity.EndDate = request.EndDate;
            entity.Status = request.Status;
            entity.NumberOfSwaps = request.NumberOfSwap;

            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await context.Subscriptions.FirstOrDefaultAsync(s => s.SubscriptionId == id);
            if (entity is null)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Code = "404",
                    ErrorMessage = "Subscription not found."
                };

            context.Subscriptions.Remove(entity);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Có thể bị ràng buộc bởi thanh toán/subscription payment...
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.Conflict,
                    Code = "409",
                    ErrorMessage = "Cannot delete this subscription because it is referenced by other records."
                };
            }
        }

        // NEW METHOD - Added based on the new interface
        public async Task<List<SubscriptionResponse>> GetSubscriptionDetailAsync()
        {
            var subscriptions = await context.Subscriptions
                .Include(s => s.User)
                .Include(s => s.SubscriptionPlan)
                .OrderByDescending(s => s.Status == SubscriptionStatus.Active)
                .ThenByDescending(s => s.CreatedAt)
                .ThenBy(s => s.User.FullName)
                .ToListAsync();

            return subscriptions.Select(Map).ToList();
        }
        public async Task ResetExpiredSubscriptionsAsync()
        {
            var now = DateTime.UtcNow;

            // Find active subscriptions that have expired
            var expiredSubscriptions = await context.Subscriptions
                .Include(s => s.SubscriptionPlan)
                .Where(s =>
                    s.Status == SubscriptionStatus.Active &&
                    s.EndDate <= now)
                .ToListAsync();

            if (!expiredSubscriptions.Any())
            {
                return; // No subscriptions to reset
            }

            foreach (var subscription in expiredSubscriptions)
            {
                // Reset swap amount to plan default
                subscription.NumberOfSwaps = subscription.SubscriptionPlan.SwapAmount;

                // Extend for another month
                subscription.StartDate = now;
                subscription.EndDate = now.AddMonths(1);
            }

            await context.SaveChangesAsync();
        }
        private static void ValidateRequestDates(DateTime start, DateTime end)
        {
            if (end <= start)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400",
                    ErrorMessage = "EndDate must be greater than StartDate."
                };
        }

        private static SubscriptionResponse Map(Subscription s) => new()
        {
            SubscriptionId = s.SubscriptionId,
            UserId = s.UserId,
            PlanId = s.PlanId,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            Status = s.Status,
            NumberOfSwap = s.NumberOfSwaps,
            CreatedAt = s.CreatedAt,
            // Thêm thông tin chi tiết nếu cần
            UserName = s.User?.FullName,
            UserEmail = s.User?.Email,
            PlanName = s.SubscriptionPlan?.Name
        };

        public async Task<PaginationWrapper<List<SubscriptionPurchaseResponse>, SubscriptionPurchaseResponse>> GetAllSubscriptionPurchasesAsync(
           int page,
           int pageSize,
           string? search = null)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            // Query với Include SubscriptionPayments và SubscriptionPlan
            var query = context.Subscriptions
                .Include(s => s.User)
                .Include(s => s.SubscriptionPayments)
                .Include(s => s.SubscriptionPlan)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(s =>
                    s.User.FullName.ToLower().Contains(term) ||
                    s.User.Email.ToLower().Contains(term) ||
                    s.SubscriptionPlan.Name.ToLower().Contains(term) ||
                    s.SubscriptionId.ToLower().Contains(term));
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SubscriptionPurchaseResponse
                {
                    // Subscription Info
                    SubscriptionId = s.SubscriptionId,
                    UserId = s.UserId,
                    UserFullName = s.User.FullName,
                    UserAvatarUrl = s.User.AvatarUrl,
                    UserEmail = s.User.Email,
                    PlanId = s.PlanId,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    Status = s.Status.ToString(),
                    NumberOfSwaps = s.NumberOfSwaps,
                    CreatedAt = s.CreatedAt,

                    // SubscriptionPlan Info
                    PlanInfo = new SubscriptionPlanDto
                    {
                        PlanId = s.SubscriptionPlan.PlanId,
                        Name = s.SubscriptionPlan.Name,
                        Description = s.SubscriptionPlan.Description,
                        MonthlyFee = s.SubscriptionPlan.MonthlyFee,
                        Active = s.SubscriptionPlan.Active,
                        SwapAmount = s.SubscriptionPlan.SwapAmount,
                        CreatedAt = s.SubscriptionPlan.CreatedAt
                    },

                    // SubscriptionPayment List
                    Payments = s.SubscriptionPayments.Select(sp => new SubscriptionPaymentDto
                    {
                        SubPayId = sp.SubPayId,
                        SubscriptionId = sp.SubscriptionId,
                        OrderCode = sp.OrderCode,
                        Amount = sp.Amount,
                        Currency = sp.Currency,
                        PaymentMethod = sp.PaymentMethod.ToString(),
                        Status = sp.Status.ToString(),
                        CreatedAt = sp.CreatedAt
                    }).ToList()
                })
                .ToListAsync();

            return new PaginationWrapper<List<SubscriptionPurchaseResponse>, SubscriptionPurchaseResponse>(
                items, page, totalItems, pageSize);
        }
    }
}