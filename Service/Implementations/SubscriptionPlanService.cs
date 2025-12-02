using System.Net;
using BusinessObject;
using BusinessObject.Dtos;
using BusinessObject.DTOs;
using BusinessObject.Entities;
using Microsoft.EntityFrameworkCore;
using Service.Exceptions;
using Service.Interfaces;

namespace Service.Implementations
{
    public class SubscriptionPlanService(ApplicationDbContext context) : ISubscriptionPlanService
    {
        
        public async Task<SubscriptionPlanResponse> GetAllAsync()
        {
            var plan = await context.SubscriptionPlans
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            if (plan is null)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Code = "404",
                    ErrorMessage = "No subscription plan found."
                };

            return Map(plan);
        }

        public async Task<SubscriptionPlanResponse?> GetByIdAsync(string id)
        {
            var plan = await context.SubscriptionPlans.FirstOrDefaultAsync(p => p.PlanId == id);
            return plan is null ? null : Map(plan);
        }

        public async Task AddAsync(SubscriptionPlanRequest request)
        {
            ValidateRequest(request);

            // Tên gói không trùng
            var exists = await context.SubscriptionPlans.AnyAsync(p => p.Name == request.Name);
            if (exists)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.Conflict,
                    Code = "409",
                    ErrorMessage = "Subscription plan name already exists."
                };

            var entity = new SubscriptionPlan
            {
                PlanId = Guid.NewGuid().ToString(),
                Name = request.Name.Trim(),
                Description = request.Description,
                MonthlyFee = request.MonthlyFee,
                Active = request.Active,
                SwapAmount = request.SwapAmount,
                CreatedAt = DateTime.UtcNow
            };

            context.SubscriptionPlans.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SubscriptionPlanRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PlanId))
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400",
                    ErrorMessage = "PlanId is required."
                };

            ValidateRequest(request);

            var entity = await context.SubscriptionPlans.FirstOrDefaultAsync(p => p.PlanId == request.PlanId);
            if (entity is null)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Code = "404",
                    ErrorMessage = "Subscription plan not found."
                };

            // Tên trùng với gói khác
            var nameInUse = await context.SubscriptionPlans.AnyAsync(p =>
                p.Name == request.Name && p.PlanId != request.PlanId);
            if (nameInUse)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.Conflict,
                    Code = "409",
                    ErrorMessage = "Subscription plan name already exists."
                };

            entity.Name = request.Name.Trim();
            entity.Description = request.Description;
            entity.MonthlyFee = request.MonthlyFee;
            entity.Active = request.Active;
            entity.SwapAmount = request.SwapAmount;

            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await context.SubscriptionPlans.FirstOrDefaultAsync(p => p.PlanId == id);
            if (entity is null)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Code = "404",
                    ErrorMessage = "Subscription plan not found."
                };

            // Có thể đang được tham chiếu bởi Subscription
            context.SubscriptionPlans.Remove(entity);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.Conflict,
                    Code = "409",
                    ErrorMessage = "Cannot delete this plan because it is referenced by subscriptions."
                };
            }
        }

        public async Task<PaginationWrapper<List<SubscriptionPlanResponse>, SubscriptionPlanResponse>> GetAllSubscriptionPlanAsync(
            int page, int pageSize, string? search)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = context.SubscriptionPlans.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(p =>
                    p.Name.Contains(term) ||
                    (p.Description != null && p.Description.Contains(term)));
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = items.Select(Map).ToList();

            return new PaginationWrapper<List<SubscriptionPlanResponse>, SubscriptionPlanResponse>(
                responses, page, totalItems, pageSize);
        }

        private static void ValidateRequest(SubscriptionPlanRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400",
                    ErrorMessage = "Name is required."
                };
            if (req.MonthlyFee < 0)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400",
                    ErrorMessage = "MonthlyFee must be non-negative."
                };
            if (req.SwapAmount < 0)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400",
                    ErrorMessage = "SwapAmount must be non-negative."
                };
        }

        private static SubscriptionPlanResponse Map(SubscriptionPlan p) => new()
        {
            PlanId = p.PlanId,
            Name = p.Name,
            Description = p.Description,
            MonthlyFee = p.MonthlyFee,
            Active = p.Active,
            SwapAmount = p.SwapAmount,
            CreatedAt = p.CreatedAt
        };
    }
}