using BusinessObject.DTOs;

namespace Service.Interfaces;

public interface IAvailabilityService
{
    Task<List<AvailableStaffResponse>> GetAvailableStaffAsync(AvailabilityQuery query);
}