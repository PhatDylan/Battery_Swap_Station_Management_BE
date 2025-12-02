using BusinessObject.DTOs;
using BusinessObject.DTOs.BatterySwap;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations;
using Service.Interfaces;

namespace EV_Driver.Controllers
{
    [ApiController]
    [Route("api/staff")]
    [Authorize(Roles = "Staff")]

    
    public class BatterySwapResponseController(IBatterySwapResponseService batterySwapResponseService) : ControllerBase
    {
        [HttpGet("completed-by-staff/{stationStaffId}")]
        public async Task<ActionResult<ResponseObject<List<CompletedBatterySwapResponseDto>>>> GetCompletedSwapsByStationStaffId(
            string stationStaffId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            // Validate Station Staff ID
            if (string.IsNullOrWhiteSpace(stationStaffId))
            {
                return BadRequest(new ResponseObject<object>
                {
                    Success = false,
                    Code = "400",
                    Message = "Station Staff ID is required"
                });
            }

            // Validate page number
            if (page < 1)
            {
                return BadRequest(new ResponseObject<object>
                {
                    Success = false,
                    Code = "400",
                    Message = "Page number must be greater than 0"
                });
            }

            // Validate page size
            if (pageSize < 1)
            {
                return BadRequest(new ResponseObject<object>
                {
                    Success = false,
                    Code = "400",
                    Message = "Page size must be greater than 0"
                });
            }

            try
            {
                // Gọi service để lấy dữ liệu có phân trang
                var paginatedResult = await batterySwapResponseService.GetCompletedSwapsByStationStaffIdAsync(
                    stationStaffId,
                    page,
                    pageSize);

                // Kiểm tra nếu không có dữ liệu
                if (paginatedResult.TotalCount == 0)
                {
                    return NotFound(new ResponseObject<object>
                    {
                        Success = false,
                        Code = "404",
                        Message = $"No completed battery swaps found for station staff ID: {stationStaffId}"
                    });
                }

                // Tạo response object và unwrap pagination
                var response = new ResponseObject<List<CompletedBatterySwapResponseDto>>
                {
                    Success = true,
                    Code = "200",
                    Message = "Retrieved completed battery swaps successfully"
                }.UnwrapPagination(paginatedResult);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseObject<object>
                {
                    Success = false,
                    Code = "500",
                    Message = "An error occurred while retrieving battery swaps",
                    Content = new { error = ex.Message }
                });
            }
        }
    }
}
