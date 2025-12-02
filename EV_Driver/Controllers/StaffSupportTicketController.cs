using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject.DTOs;
using BusinessObject.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

namespace EV_Driver.Controllers
{
    [ApiController]
    [Route("api/staff/support-tickets")]
    [Authorize(Roles = "Staff")]
    public class StaffSupportTicketController(ISupportTicketService supportTicketService) : ControllerBase
    {


        /// <summary>
        /// Staff: Lấy danh sách tất cả support tickets (phân trang, tìm kiếm).
        /// Query search sẽ áp dụng trên subject, message, user full name, station name.
        /// Gợi ý: search="Battery Swap Emergency" để lọc ticket khẩn cấp.
        /// </summary>
        /// <param name="page">Trang (mặc định 1)</param>
        /// <param name="pageSize">Kích thước trang (mặc định 10)</param>
        /// <param name="search">Từ khóa tìm kiếm</param>
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
        /// Staff: Lấy danh sách support tickets theo userId.
        /// </summary>
        /// <param name="userId">Id của người dùng (Driver)</param>
        /// <param name="page">Trang (mặc định 1)</param>
        /// <param name="pageSize">Kích thước trang (mặc định 10)</param>
        /// <param name="search">Từ khóa tìm kiếm bổ sung</param>
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
}