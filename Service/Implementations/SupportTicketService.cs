using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Dtos;
using BusinessObject.DTOs;
using BusinessObject.Entities;
using BusinessObject.Enums;
using Microsoft.EntityFrameworkCore;
using Service.Exceptions;
using Service.Interfaces;

namespace Service.Implementations
{
    public class SupportTicketService(ApplicationDbContext context) : ISupportTicketService
    {
        private IQueryable<SupportTicket> BaseQuery()
        {
            return context.SupportTickets
                .AsNoTracking()
                .Include(t => t.User)
                .Include(t => t.Station);
        }

        private static IQueryable<SupportTicket> ApplySearch(IQueryable<SupportTicket> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search)) return query;
            var keyword = search.Trim();
            return query.Where(t =>
                t.Subject.Contains(keyword) ||
                (t.Message != null && t.Message.Contains(keyword)) ||
                (t.User != null && t.User.FullName.Contains(keyword)) ||
                (t.Station != null && t.Station.Name.Contains(keyword)));
        }

        private static async Task<PaginationWrapper<List<SupportTicketResponse>, SupportTicketResponse>> Paginate(
            IQueryable<SupportTicket> query, int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(t => t.UpdatedAt)
                .ThenByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = items.Select(ToResponse).ToList();

            return new PaginationWrapper<List<SupportTicketResponse>, SupportTicketResponse>(
                responses, totalItems, page, pageSize);
        }

        public async Task<PaginationWrapper<List<SupportTicketResponse>, SupportTicketResponse>> GetAllSupportTicketAsync(int page, int pageSize, string? search)
        {
            var query = ApplySearch(BaseQuery(), search);
            return await Paginate(query, page, pageSize);
        }

        public async Task<SupportTicketResponse?> GetBySupportTicketAsync(string id)
        {
            var entity = await BaseQuery().FirstOrDefaultAsync(x => x.TicketId == id);
            return entity == null ? null : ToResponse(entity);
        }

        public async Task AddAsync(SupportTicketRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
                throw new ValidationException { StatusCode = HttpStatusCode.BadRequest, Code = "400", ErrorMessage = "UserId is required." };
            if (string.IsNullOrWhiteSpace(request.Subject))
                throw new ValidationException { StatusCode = HttpStatusCode.BadRequest, Code = "400", ErrorMessage = "Subject is required." };
            if (string.IsNullOrWhiteSpace(request.Message))
                throw new ValidationException { StatusCode = HttpStatusCode.BadRequest, Code = "400", ErrorMessage = "Message is required." };

            var entity = new SupportTicket
            {
                TicketId = Guid.NewGuid().ToString(),
                UserId = request.UserId,
                StationId = request.StationId,
                Subject = request.Subject,
                Message = request.Message,
                Priority = request.Priority,
                Status = request.Status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.SupportTickets.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SupportTicketRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TicketId))
                throw new ValidationException { StatusCode = HttpStatusCode.BadRequest, Code = "400", ErrorMessage = "TicketId is required." };

            var entity = await context.SupportTickets.FirstOrDefaultAsync(t => t.TicketId == request.TicketId);
            if (entity == null)
                throw new ValidationException { StatusCode = HttpStatusCode.NotFound, Code = "404", ErrorMessage = "Support ticket not found." };

            entity.StationId = request.StationId;
            entity.Subject = request.Subject ?? entity.Subject;
            entity.Message = request.Message ?? entity.Message;
            entity.Priority = request.Priority;
            entity.Status = request.Status;
            entity.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await context.SupportTickets.FirstOrDefaultAsync(t => t.TicketId == id);
            if (entity == null)
                throw new ValidationException { StatusCode = HttpStatusCode.NotFound, Code = "404", ErrorMessage = "Support ticket not found." };

            context.SupportTickets.Remove(entity);
            await context.SaveChangesAsync();
        }

        public async Task<List<SupportTicketResponse>> GetSupportTicketDetailAsync()
        {
            var tickets = await BaseQuery()
                .OrderByDescending(t => t.UpdatedAt)
                .ThenByDescending(t => t.CreatedAt)
                .ToListAsync();

            return tickets.Select(ToResponse).ToList();
        }

        public async Task<PaginationWrapper<List<SupportTicketResponse>, SupportTicketResponse>> GetAllSupportTicketByUserAsync(string userId, int page, int pageSize, string? search)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ValidationException { StatusCode = HttpStatusCode.BadRequest, Code = "400", ErrorMessage = "userId is required." };

            var query = BaseQuery().Where(t => t.UserId == userId);
            query = ApplySearch(query, search);
            return await Paginate(query, page, pageSize);
        }

        public async Task<SupportTicketResponse> CreateEmergencyTicketAsync(string userId, SupportTicketEmergencyRequest request)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ValidationException { StatusCode = HttpStatusCode.BadRequest, Code = "400", ErrorMessage = "UserId is required." };
            if (string.IsNullOrWhiteSpace(request.Message))
                throw new ValidationException { StatusCode = HttpStatusCode.BadRequest, Code = "400", ErrorMessage = "Message is required." };

            var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                throw new ValidationException { StatusCode = HttpStatusCode.NotFound, Code = "404", ErrorMessage = "User not found." };
            if (user.Status != UserStatus.Active)
                throw new ValidationException { StatusCode = HttpStatusCode.BadRequest, Code = "400", ErrorMessage = "User is not Active." };

            var entity = new SupportTicket
            {
                UserId = userId,
                StationId = request.StationId,
                Subject = "Battery Swap Emergency",
                Message = request.Message,
                Priority = Priority.Urgent,
                Status = SupportTicketStatus.Open,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.SupportTickets.Add(entity);
            await context.SaveChangesAsync();

            var saved = await BaseQuery().FirstAsync(t => t.TicketId == entity.TicketId);
            return ToResponse(saved);
        }

        private static SupportTicketResponse ToResponse(SupportTicket t)
        {
            return new SupportTicketResponse
            {
                TicketId = t.TicketId,
                UserId = t.UserId,
                StationId = t.StationId,
                Subject = t.Subject,
                Message = t.Message ?? string.Empty,
                Priority = t.Priority,
                Status = t.Status,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt ?? t.CreatedAt,
                UserName = t.User?.FullName,
                UserEmail = t.User?.Email,
                StationName = t.Station?.Name,
                StationAddress = t.Station?.Address
            };
        }
    }
}