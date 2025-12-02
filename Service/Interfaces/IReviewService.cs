using BusinessObject.Dtos;
using BusinessObject.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IReviewService
    {
        Task<ReviewResponse> GetReviewDetailAsync();
        Task<ReviewResponse> GetByStationAsync(string stationId);
        Task<ReviewResponse> GetByUserAsync(string userId);
        Task<ReviewResponse?> GetByIdAsync(string id);
        Task AddAsync(ReviewRequest review);
        Task UpdateAsync(ReviewRequest review);
        Task DeleteAsync(string id);
        Task<PaginationWrapper<List<ReviewResponse>, ReviewResponse>> GetAllReviewAsync(int page, int pageSize, string? search);
    }
}
