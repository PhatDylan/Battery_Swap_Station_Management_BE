using BusinessObject.Dtos;
using BusinessObject.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

namespace EV_Driver.Controllers;

[ApiController]
[Route("api/admin/revenue")]
[Authorize(Roles = "Admin")]
public class AdminRevenueController(IAdminRevenueService service) : ControllerBase
{
    /// <summary>
    /// Lấy báo cáo doanh thu theo ngày/tháng/năm
    /// Doanh thu = Tiền từ swap (pay-per-use) + Tiền từ subscription
    /// </summary>
    [HttpGet("report")]
    public async Task<ActionResult<ResponseObject<RevenueReportDto>>> GetRevenueReport(CancellationToken cancellationToken)
    {
        var result = await service.GetRevenueReportAsync(cancellationToken);
        return Ok(new ResponseObject<RevenueReportDto>
        {
            Content = result,
            Message = "Get revenue report successfully",
            Code = "200",
            Success = true
        });
    }
}
