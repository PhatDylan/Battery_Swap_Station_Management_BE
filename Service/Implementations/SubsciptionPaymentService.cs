using System.Net;
using BusinessObject;
using BusinessObject.Dtos;
using BusinessObject.Entities;
using BusinessObject.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Net.payOS;
using Net.payOS.Types;
using Service.Exceptions;
using Service.Interfaces;
using Service.Utils;

namespace Service.Implementations
{
    public class SubscriptionPaymentService( IConfiguration configuration, ApplicationDbContext context, IHttpContextAccessor accessor) : ISubscriptionPaymentService
    {
        private readonly PayOS _payOs = new(configuration["PayOSSetting:ClientId"] ?? "",
            configuration["PayOSSetting:ApiKey"] ?? "",
            configuration["PayOSSetting:ChecksumKey"] ?? "");

        public async Task<CreateSubscriptionPaymentResponse> CreateSubscriptionPaymentAsync(
            CreateSubscriptionPaymentRequest request)
        {
            var userId = JwtUtils.GetUserId(accessor);
            if (string.IsNullOrEmpty(userId))
                throw new ValidationException
                {
                    ErrorMessage = "Unauthorized",
                    Code = "401",
                    StatusCode = HttpStatusCode.Unauthorized
                };
            
            var plan = await context.SubscriptionPlans.FirstOrDefaultAsync(p => p.PlanId == request.PlanId);
            if (plan is null || !plan.Active)
                throw new ValidationException
                {
                    ErrorMessage = "Subscription plan not found or inactive",
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400"
                };

            // Check if user already has active subscription
            var hasActiveSubscription = await context.Subscriptions.AnyAsync(s =>
                s.UserId == userId && s.Status == SubscriptionStatus.Active);
            if (hasActiveSubscription)
                throw new ValidationException
                {
                    ErrorMessage = "User already has an active subscription",
                    StatusCode = HttpStatusCode.Conflict,
                    Code = "409"
                };

            // Only allow online payment methods (not Cash)
            if (request.PaymentMethod == PayMethod.Cash)
                throw new ValidationException
                {
                    ErrorMessage = "Subscription must be paid online",
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400"
                };

            var paymentOccured = await context.SubscriptionPayments.FirstOrDefaultAsync(sp => sp.Subscription.UserId == userId 
                && sp.Status == PayStatus.Completed 
                && sp.PaymentMethod == request.PaymentMethod 
                && sp.Subscription.PlanId == request.PlanId);

            if (paymentOccured != null)
            {
                return new CreateSubscriptionPaymentResponse
                {
                    SubPayId = paymentOccured.SubPayId,
                    SubscriptionId = paymentOccured.SubscriptionId,
                    OrderCode = paymentOccured.OrderCode,
                    Amount = paymentOccured.Amount,
                    PaymentMethod = paymentOccured.PaymentMethod,
                    Status = PayStatus.Completed,
                    PaymentUrl = paymentOccured.PaymentUrl,
                    CreatedAt = paymentOccured.CreatedAt
                };
            }
            
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            // Create Subscription first (Pending status)
            var subscription = new Subscription
            {
                SubscriptionId = Guid.NewGuid().ToString(),
                UserId = userId,
                PlanId = request.PlanId,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = SubscriptionStatus.Inactive,
                NumberOfSwaps = 0,
                CreatedAt = DateTime.UtcNow
            };

            // Create subscription payment
            

            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                context.Subscriptions.Add(subscription);
                
                await context.SaveChangesAsync();
                
                var payOsItemData = new List<ItemData>
                {
                    new(plan.Name, 1, (int)plan.MonthlyFee)
                };
                var redirectUrl = configuration["Subscription:RedirectUrl"]!;
                
                var paymentData = new PaymentData(
                    timestamp,
                    (int)plan.MonthlyFee,
                    $"Subscription: {plan.Name}",
                    payOsItemData,
                    redirectUrl,
                    redirectUrl
                );

                var createPayment = await _payOs.createPaymentLink(paymentData);
                var subscriptionPayment = new SubscriptionPayment
                {
                    SubPayId = Guid.NewGuid().ToString(),
                    SubscriptionId = subscription.SubscriptionId,
                    OrderCode = timestamp.ToString(),
                    Amount = plan.MonthlyFee,
                    Currency = "VND",
                    PaymentMethod = request.PaymentMethod,
                    PaymentUrl = createPayment.checkoutUrl,
                    Status = PayStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                context.SubscriptionPayments.Add(subscriptionPayment);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new CreateSubscriptionPaymentResponse
                {
                    SubPayId = subscriptionPayment.SubPayId,
                    SubscriptionId = subscription.SubscriptionId,
                    OrderCode = subscriptionPayment.OrderCode,
                    Amount = subscriptionPayment.Amount,
                    PaymentMethod = subscriptionPayment.PaymentMethod,
                    Status = subscriptionPayment.Status,
                    PaymentUrl = createPayment.checkoutUrl,
                    CreatedAt = subscriptionPayment.CreatedAt
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new ValidationException
                {
                    ErrorMessage = ex.Message,
                    Code = "500",
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
        }

        public async Task<SubscriptionPaymentResponse> GetSubscriptionPaymentDetailAsync(string subPayId)
        {
            var payment = await context.SubscriptionPayments
                .Include(sp => sp.Subscription)
                .ThenInclude(s => s.User)
                .Include(sp => sp.Subscription)
                .ThenInclude(s => s.SubscriptionPlan)
                .FirstOrDefaultAsync(sp => sp.SubPayId == subPayId);

            if (payment is null)
                throw new ValidationException
                {
                    ErrorMessage = "Subscription payment not found",
                    StatusCode = HttpStatusCode.NotFound,
                    Code = "404"
                };

            return MapToResponse(payment);
        }

        public async Task<List<SubscriptionPaymentResponse>> GetMySubscriptionPaymentsAsync()
        {
            var userId = JwtUtils.GetUserId(accessor);
            if (string.IsNullOrEmpty(userId))
                throw new ValidationException
                {
                    ErrorMessage = "Unauthorized",
                    Code = "401",
                    StatusCode = HttpStatusCode.Unauthorized
                };

            var payments = await context.SubscriptionPayments
                .Include(sp => sp.Subscription)
                .ThenInclude(s => s.User)
                .Include(sp => sp.Subscription)
                .ThenInclude(s => s.SubscriptionPlan)
                .Where(sp => sp.Subscription.UserId == userId)
                .OrderByDescending(sp => sp.CreatedAt)
                .ToListAsync();

            return payments.Select(MapToResponse).ToList();
        }

        private static SubscriptionPaymentResponse MapToResponse(SubscriptionPayment payment)
        {
            return new SubscriptionPaymentResponse
            {
                SubPayId = payment.SubPayId,
                SubscriptionId = payment.SubscriptionId,
                OrderCode = payment.OrderCode,
                Amount = payment.Amount,
                Currency = payment.Currency,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status,
                CreatedAt = payment.CreatedAt,
                UserName = payment.Subscription?.User?.FullName,
                PlanName = payment.Subscription?.SubscriptionPlan?.Name
            };
        }
    }
}
