using BusinessObject.Dtos;
using BusinessObject.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

namespace EV_Driver.Controllers;

[ApiController]
[Route("api/admin/management-battery")]
[Authorize(Roles = "Admin")]
public class AdminManagementBatteryController(IAdminManagementBatteryService service) : ControllerBase
{
    [HttpGet("report")]
    public async Task<ActionResult<ResponseObject<CountBatteryAdminDto>>> GetReport(CancellationToken cancellationToken)
    {
        var result = await service.GetPeakSwapReportAsync(cancellationToken);
        return Ok(new ResponseObject<CountBatteryAdminDto>
        {
            Content = result,
            Message = "Get report successfully",
            Code = "200",
            Success = true
        });
    }
}