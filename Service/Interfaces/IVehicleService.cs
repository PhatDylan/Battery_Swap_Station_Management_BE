using BusinessObject.DTOs;

namespace Service.Interfaces;

public interface IVehicleService
{
    Task<VehicleResponse> GetVehicleAsync(string vehicleId);
    Task<VehicleResponse> CreateVehicleAsync(VehicleRequest vehicleRequest);
    Task<VehicleResponse> UpdateVehicleAsync(string vehicleId, VehicleRequest vehicleRequest);
    Task<VehicleResponse> DeleteVehicleAsync(string vehicleId);

    Task<PaginationWrapper<List<VehicleResponse>, VehicleResponse>> GetAllVehiclesAsync(int page, int pageSize,
        string? search);

    Task<PaginationWrapper<List<VehicleResponse>, VehicleResponse>> GetVehiclesByUserAsync(string userId, int page,
        int pageSize, string? search);
}