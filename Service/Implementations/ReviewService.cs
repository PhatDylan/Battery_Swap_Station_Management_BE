using BusinessObject;
using BusinessObject.Dtos;
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
    public class ReviewService(ApplicationDbContext context, IHttpContextAccessor accessor) : IReviewService
    {
        public async Task<ReviewResponse> GetReviewDetailAsync()
        {
            var reviews = await context.Reviews
                .Select(r => new ReviewResponse
                {
                    ReviewId = r.ReviewId,
                    UserId = r.UserId,
                    StationId = r.StationId,
                    SwapId = r.SwapId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (reviews == null)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Code = "404",
                    ErrorMessage = "No reviews found"
                };

            return reviews;
        }

        public async Task<ReviewResponse> GetByStationAsync(string stationId)
        {
            var review = await context.Reviews
                .Where(r => r.StationId == stationId)
                .Select(r => new ReviewResponse
                {
                    ReviewId = r.ReviewId,
                    UserId = r.UserId,
                    StationId = r.StationId,
                    SwapId = r.SwapId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (review == null)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Code = "404",
                    ErrorMessage = "No review found for this station"
                };

            return review;
        }

        public async Task<ReviewResponse> GetByUserAsync(string userId)
        {
            var review = await context.Reviews
                .Where(r => r.UserId == userId)
                .Select(r => new ReviewResponse
                {
                    ReviewId = r.ReviewId,
                    UserId = r.UserId,
                    StationId = r.StationId,
                    SwapId = r.SwapId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (review == null)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Code = "404",
                    ErrorMessage = "No review found for this user"
                };

            return review;
        }

        public async Task<ReviewResponse?> GetByIdAsync(string id)
        {
            var r = await context.Reviews.FindAsync(id);
            if (r == null) return null;

            return new ReviewResponse
            {
                ReviewId = r.ReviewId,
                UserId = r.UserId,
                StationId = r.StationId,
                SwapId = r.SwapId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            };
        }

        public async Task<PaginationWrapper<List<ReviewResponse>, ReviewResponse>> GetAllReviewAsync(int page, int pageSize, string? search)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = context.Reviews.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(r =>
                    r.ReviewId.ToLower().Contains(term) ||
                    r.UserId.ToLower().Contains(term) ||
                    r.StationId.ToLower().Contains(term) ||
                    r.SwapId.ToLower().Contains(term) ||
                    (r.Comment != null && r.Comment.ToLower().Contains(term)) ||
                    r.Rating.ToString().Contains(term));
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReviewResponse
                {
                    ReviewId = r.ReviewId,
                    UserId = r.UserId,
                    StationId = r.StationId,
                    SwapId = r.SwapId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return new PaginationWrapper<List<ReviewResponse>, ReviewResponse>(
                items, page, totalItems, pageSize);
        }

        public async Task AddAsync(ReviewRequest review)
        {
            // Validate
            if (review.Rating < 1 || review.Rating > 5)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400",
                    ErrorMessage = "Rating must be between 1-5"
                };
            if (string.IsNullOrWhiteSpace(review.UserId) ||
                string.IsNullOrWhiteSpace(review.StationId) ||
                string.IsNullOrWhiteSpace(review.SwapId))
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400",
                    ErrorMessage = "UserId, StationId và SwapId is obligatory"
                };

            //Kiểm tra swap tồn tại, đúng user, đúng station và đã Completed
            var swap = await context.BatterySwaps
                .FirstOrDefaultAsync(bs => bs.SwapId == review.SwapId
                                           && bs.UserId == review.UserId
                                           && bs.StationId == review.StationId
                                           && bs.Status == BBRStatus.Completed);
            if (swap == null)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400",
                    ErrorMessage = "You must successfully change the battery to review."
                };

            //Mỗi swap chỉ review 1 lần
            var exists = await context.Reviews
                .AnyAsync(r => r.SwapId == review.SwapId && r.UserId == review.UserId);
            if (exists)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400",
                    ErrorMessage = "You have already reviewed this battery swap."
                };

            var entity = new Review
            {
                ReviewId = Guid.NewGuid().ToString(),
                UserId = review.UserId,
                StationId = review.StationId,
                SwapId = review.SwapId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = DateTime.UtcNow
            };

            context.Reviews.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ReviewRequest review)
        {
            if (string.IsNullOrWhiteSpace(review.ReviewId))
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400",
                    ErrorMessage = "ReviewId is obligatory"
                };

            var entity = await context.Reviews.FindAsync(review.ReviewId);
            if (entity == null)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Code = "404",
                    ErrorMessage = "Review is not exist"
                };

            if (review.Rating < 1 || review.Rating > 5)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400",
                    ErrorMessage = "Rating must be between 1-5"
                };

            entity.Rating = review.Rating;
            entity.Comment = review.Comment;
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await context.Reviews.FindAsync(id);
            if (entity == null)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Code = "404",
                    ErrorMessage = "Review is not exist"
                };

            context.Reviews.Remove(entity);
            await context.SaveChangesAsync();
        }
    }
}