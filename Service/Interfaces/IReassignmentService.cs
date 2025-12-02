using BusinessObject.DTOs;
using BusinessObject.Entities;

namespace Service.Interfaces;

public interface IReassignmentService
{
    Task<(StationStaffOverride Override, StaffAbsence Absence)> ReassignAsync(ReassignStaffRequest request);
}