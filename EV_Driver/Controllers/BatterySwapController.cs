using BusinessObject.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

namespace EV_Driver.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BatterySwapController(IBatterySwapService batterySwapService) : ControllerBase
    {
        [HttpPost("create")]
        public async Task<ActionResult<ResponseObject<BatterySwapResponse>>> CreateBatterySwapFromBooking([FromBody] CreateBatterySwapRequest request)
        {
            var result = await batterySwapService.CreateBatterySwapFromBookingAsync(request);
            return Ok(new ResponseObject<BatterySwapResponse>
            {
                Content = result,
                Message = "Battery swap created successfully",
                Code = "200",
                Success = true
            });
        }

        [HttpPut("{swapId}/status")]
        public async Task<ActionResult<ResponseObject<BatterySwapResponse>>> UpdateBatterySwapStatus(
            string swapId,
            [FromBody] UpdateBatterySwapStatusRequest request)
        {
            var result = await batterySwapService.UpdateBatterySwapStatusAsync(swapId, request);
            return Ok(new ResponseObject<BatterySwapResponse>
            {
                Content = result,
                Message = "Battery swap status updated successfully",
                Code = "200",
                Success = true
            });
        }

        [HttpGet("{swapId}")]
        public async Task<ActionResult<ResponseObject<BatterySwapDetailResponse>>> GetBatterySwapDetail(string swapId)
        {
            var result = await batterySwapService.GetBatterySwapDetailAsync(swapId);
            return Ok(new ResponseObject<BatterySwapDetailResponse>
            {
                Content = result,
                Message = "Get battery swap detail successfully",
                Code = "200",
                Success = true
            });
        }

        [HttpGet("station/{stationId}")]
        public async Task<ActionResult<ResponseObject<List<BatterySwapResponse>>>> GetStationBatterySwaps(
            string stationId,
            int page = 1,
            int pageSize = 10,
            string? search = null)
        {
            var result = await batterySwapService.GetStationBatterySwapsAsync(stationId, page, pageSize, search);
            return Ok(new ResponseObject<List<BatterySwapResponse>>
            {
                Message = "Get station battery swaps successfully",
                Code = "200",
                Success = true
            }.UnwrapPagination(result));
        }

        [HttpGet("my-swaps")]
        [Authorize(Roles = "Staff")]
        public async Task<ActionResult<ResponseObject<List<BatterySwapResponse>>>> GetMyBatterySwaps(
            int page = 1,
            int pageSize = 10,
            string? search = null)
        {
            var result = await batterySwapService.GetMyBatterySwapsAsync(page, pageSize, search);
            return Ok(new ResponseObject<List<BatterySwapResponse>>
            {
                Message = "Get my battery swaps successfully",
                Code = "200",
                Success = true
            }.UnwrapPagination(result));
        }
        [HttpGet("driver/me")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult<ResponseObject<List<BatterySwapResponse>>>> GetDriverSwapHistory( int page = 1, int pageSize = 10, string? search = null)
        {
            var result = await batterySwapService.GetDriverSwapHistoryAsync(page, pageSize, search);
            return Ok(new ResponseObject<List<BatterySwapResponse>>
            {
                Message = "Get driver swap successfully",
                Code = "200",
                Success = true
            }.UnwrapPagination(result));
        }

        [HttpGet("booking/{bookingId}")]
        public async Task<ActionResult<ResponseObject<List<BatterySwapResponse>>>> GetSwapsByBooking(string bookingId)
        {
            var result = await batterySwapService.GetSwapsByBookingAsync(bookingId);
            return Ok(new ResponseObject<List<BatterySwapResponse>>
            {
                Content = result,
                Message = "Get battery swaps by booking successfully",
                Code = "200",
                Success = true
            });
        }
    }
}
