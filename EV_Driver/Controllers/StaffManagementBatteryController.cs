using BusinessObject.DTOs;
using BusinessObject.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

namespace EV_Driver.Controllers;

[ApiController]
[Route("api/staff/swaps")]
[Authorize(Roles = "Staff")]
public class StaffManagementBatteryController(IStaffManagementBatteryService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ResponseObject<List<StaffSwapListItemResponse>>>> GetMyStationSwaps(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? stationId = null,
        [FromQuery] BBRStatus? status = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await service.GetMyStationSwapsAsync(page, pageSize, stationId, status, search, ct);
        return Ok(new ResponseObject<List<StaffSwapListItemResponse>>
        {
            Message = "Get staff station swaps successfully",
            Code = "200",
            Success = true
        }.UnwrapPagination(result));
    }

    [HttpPut("{swapId}/reject")]
    public async Task<ActionResult<ResponseObject<object>>> Reject(string swapId, [FromBody] StaffRejectSwapRequest request, CancellationToken ct)
    {
        await service.RejectSwapAsync(swapId, request, ct);
        return Ok(new ResponseObject<object>
        {
            Message = "Swap rejected successfully",
            Code = "200",
            Success = true
        });
    }

    [HttpPut("{swapId}/confirm")]
    public async Task<ActionResult<ResponseObject<object>>> Confirm(string swapId, CancellationToken ct)
    {
        await service.ConfirmSwapAsync(swapId, ct);
        return Ok(new ResponseObject<object>
        {
            Message = "Swap confirmed successfully",
            Code = "200",
            Success = true
        });
    }
    
    [HttpPut("{swapId}/completed")]
    public async Task<ActionResult<ResponseObject<object>>> Completed(string swapId, CancellationToken ct)
    {
        await service.CompleteSwapAsync(swapId, ct);
        return Ok(new ResponseObject<object>
        {
            Message = "Swap completed successfully",
            Code = "200",
            Success = true
        });
    }
}