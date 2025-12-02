using BusinessObject.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations;
using Service.Interfaces;

namespace EV_Driver.Controllers;

[ApiController]
[Route("api/admin/battery-coordination")]
[Authorize (Roles = "Admin")]
public class AdminBatteryCoordinationController(IBatteryCoordinationService service) : ControllerBase
{
   
    [HttpPost("plan")]
    public async Task<ActionResult<ResponseObject<DispatchPlanResponse>>> Plan([FromBody] RebalanceRequest request, CancellationToken ct)
    {
        var plan = await service.PlanRebalanceAsync(request, ct);
        return Ok(new ResponseObject<DispatchPlanResponse>
        {
            Content = plan,
            Message = "Generated dispatch plan successfully",
            Code = "200",
            Success = true
        });
    }

 
    [HttpPost("execute")]
    public async Task<ActionResult<ResponseObject<ExecuteDispatchResult>>> Execute([FromBody] ExecuteDispatchRequest request, CancellationToken ct)
    {
        var result = await service.ExecuteMovesAsync(request, ct);
        return Ok(new ResponseObject<ExecuteDispatchResult>
        {
            Content = result,
            Message = "Executed dispatch successfully",
            Code = "200",
            Success = true
        });
    }
}