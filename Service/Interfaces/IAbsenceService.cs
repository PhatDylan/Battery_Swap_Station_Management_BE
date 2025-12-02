using BusinessObject.DTOs;
using BusinessObject.Entities;

namespace Service.Interfaces;

public interface IAbsenceService
{
    Task<StaffAbsence> MarkAbsentAsync(MarkAbsenceRequest request);
}