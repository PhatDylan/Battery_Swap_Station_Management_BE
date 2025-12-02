using BusinessObject.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

namespace EV_Driver.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminStaffController(
    IAbsenceService absenceService,
    IReassignmentService reassignmentService,
    IAvailabilityService availabilityService
) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("staff-absences")]
    public async Task<IActionResult> MarkAbsence([FromBody] MarkAbsenceRequest request)
    {
        var result = await absenceService.MarkAbsentAsync(request);
        return CreatedAtAction(nameof(MarkAbsence), result);
    }

    [AllowAnonymous]
    [HttpPost("station-staff/reassign")]
    public async Task<IActionResult> Reassign([FromBody] ReassignStaffRequest request)
    {
        var (ovr, abs) = await reassignmentService.ReassignAsync(request);
        return Ok(new
        {
            Absence = new
            {
                abs.StaffAbsenceId,
                abs.UserId,
                Date = abs.Date.ToString("yyyy-MM-dd"),
                abs.Reason
            },
            Override = new
            {
                ovr.StationStaffOverrideId,
                ovr.UserId,
                ovr.StationId,
                Date = ovr.Date.ToString("yyyy-MM-dd"),
                ovr.ShiftId,
                ovr.Reason
            }
        });
    }

    [AllowAnonymous]
    [HttpGet("station-staff/availability")]
    public async Task<IActionResult> Availability([FromQuery] AvailabilityQuery query)
    {
        var data = await availabilityService.GetAvailableStaffAsync(query);
        return Ok(data);
    }
}