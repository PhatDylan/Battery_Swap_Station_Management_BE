using BusinessObject.Dtos;
using BusinessObject.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

namespace EV_Driver.Controllers;

[ApiController]
[Route("api/admin/support-tickets")]
[Authorize(Roles = "Admin")]
public class AdminSupportTicketController(ISupportTicketService supportTicketService) : ControllerBase
{
    /// <summary>
    /// Admin: Lấy danh sách tất cả support tickets (phân trang, tìm kiếm).
    /// Gợi ý: search="Battery Swap Emergency" để lọc các ticket khẩn cấp sau khi thay pin.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ResponseObject<List<SupportTicketResponse>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = await supportTicketService.GetAllSupportTicketAsync(page, pageSize, search);
        return Ok(new ResponseObject<List<SupportTicketResponse>>
        {
            Message = "Get support tickets successfully",
            Code = "200",
            Success = true
        }.UnwrapPagination(result));
    }

    /// <summary>
    /// Admin: Lấy danh sách support tickets theo userId (join qua SupportTicket.UserId).
    /// </summary>
    [HttpGet("by-user/{userId}")]
    public async Task<ActionResult<ResponseObject<List<SupportTicketResponse>>>> GetByUser(
        string userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = await supportTicketService.GetAllSupportTicketByUserAsync(userId, page, pageSize, search);
        return Ok(new ResponseObject<List<SupportTicketResponse>>
        {
            Message = "Get user support tickets successfully",
            Code = "200",
            Success = true
        }.UnwrapPagination(result));
    }
}