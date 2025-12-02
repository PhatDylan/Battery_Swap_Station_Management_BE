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
    public class BatteryTypeService(ApplicationDbContext context) : IBatteryTypeService
    {
        public async Task<BatteryTypeResponse?> GetByIdAsync(string id)
        {
            var entity = await context.BatteryTypes.FirstOrDefaultAsync(x => x.BatteryTypeId == id);
            return entity is null ? null : MapToResponse(entity);
        }

        public async Task AddAsync(BatteryTypeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.BatteryTypeName))
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400",
                    ErrorMessage = "BatteryTypeName is required."
                };

            var exists = await context.BatteryTypes
                .AnyAsync(x => x.BatteryTypeName == request.BatteryTypeName);
            if (exists)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400",
                    ErrorMessage = "BatteryTypeName already exists."
                };

            var entity = new BatteryType
            {
                BatteryTypeId = Guid.NewGuid().ToString(),
                BatteryTypeName = request.BatteryTypeName.Trim()
            };

            context.BatteryTypes.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(BatteryTypeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.BatteryTypeId))
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400",
                    ErrorMessage = "BatteryTypeId is required."
                };

            if (string.IsNullOrWhiteSpace(request.BatteryTypeName))
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400",
                    ErrorMessage = "BatteryTypeName is required."
                };

            var entity = await context.BatteryTypes
                .FirstOrDefaultAsync(x => x.BatteryTypeId == request.BatteryTypeId);
            if (entity is null)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Code = "404",
                    ErrorMessage = "Battery type not found."
                };

            var nameInUse = await context.BatteryTypes.AnyAsync(x =>
                x.BatteryTypeName == request.BatteryTypeName &&
                x.BatteryTypeId != request.BatteryTypeId);
            if (nameInUse)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400",
                    ErrorMessage = "BatteryTypeName already exists."
                };

            entity.BatteryTypeName = request.BatteryTypeName.Trim();
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await context.BatteryTypes.FirstOrDefaultAsync(x => x.BatteryTypeId == id);
            if (entity is null)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Code = "404",
                    ErrorMessage = "Battery type not found."
                };

            context.BatteryTypes.Remove(entity);
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
                    ErrorMessage = "Cannot delete this BatteryType because it is being referenced by other records."
                };
            }
        }
        public async Task<PaginationWrapper<List<BatteryTypeResponse>, BatteryTypeResponse>> GetAllBatteryTypeAsync(int page, int pageSize, string? search)
        {
            var entities = await context.BatteryTypes.ToListAsync();
            var totalItems = entities.Count;
            var responses = entities.Select(MapToResponse).ToList();
            return new PaginationWrapper<List<BatteryTypeResponse>, BatteryTypeResponse>(responses, totalItems, page, pageSize);
        }

        private static BatteryTypeResponse MapToResponse(BatteryType e) => new()
        {
            BatteryTypeId = e.BatteryTypeId,
            BatteryTypeName = e.BatteryTypeName
        };
    }
}