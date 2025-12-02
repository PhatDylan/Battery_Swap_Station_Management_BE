using BusinessObject.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

namespace EV_Driver.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StationStaffController(IStationStaffService stationStaffService) : ControllerBase
    {
        [HttpPost("assign")]
        public async Task<ActionResult<ResponseObject<StationStaffResponse>>> AssignStaffToStation([FromBody] AssignStaffRequest request)
        {
            var result = await stationStaffService.AssignStaffToStationAsync(request);
            return Ok(new ResponseObject<StationStaffResponse>
            {
                Content = result,
                Message = "Staff assigned to station successfully",
                Code = "200",
                Success = true
            });
        }

        [HttpDelete("{stationStaffId}")]
        public async Task<ActionResult<ResponseObject<StationStaffResponse>>> RemoveStaffFromStation(string stationStaffId)
        {
            var result = await stationStaffService.RemoveStaffFromStationAsync(stationStaffId);
            return Ok(new ResponseObject<StationStaffResponse>
            {
                Content = result,
                Message = "Staff removed from station successfully",
                Code = "200",
                Success = true
            });
        }

        [HttpGet("station/{stationId}")]
        public async Task<ActionResult<ResponseObject<List<StationStaffResponse>>>> GetStationStaffs(
            string stationId,
            int page = 1,
            int pageSize = 10,
            string? search = null)
        {
            var result = await stationStaffService.GetStationStaffsAsync(stationId, page, pageSize, search);
            return Ok(new ResponseObject<List<StationStaffResponse>>
            {
                Message = "Get station staffs successfully",
                Code = "200",
                Success = true
            }.UnwrapPagination(result));
        }
        
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ResponseObject<StationStaffBaseResponse>>> GetBaseStationStaffByUserId(
            string userId)
        {
            var result = await stationStaffService.GetStationStaffBaseAsync(userId);
            return Ok(new ResponseObject<StationStaffBaseResponse>
            {
                Message = "Get station staffs successfully",
                Code = "200",
                Success = true,
                Content = result
            });
        }

        [HttpGet("staff/{staffUserId}/stations")]
        public async Task<ActionResult<ResponseObject<List<StaffStationResponse>>>> GetStaffStations(
            string staffUserId,
            int page = 1,
            int pageSize = 10)
        {
            var result = await stationStaffService.GetStaffStationsAsync(staffUserId, page, pageSize);
            return Ok(new ResponseObject<List<StaffStationResponse>>
            {
                Message = "Get staff stations successfully",
                Code = "200",
                Success = true
            }.UnwrapPagination(result));
        }

        [HttpGet("{stationStaffId}")]
        public async Task<ActionResult<ResponseObject<StationStaffResponse>>> GetStationStaff(string stationStaffId)
        {
            var result = await stationStaffService.GetStationStaffAsync(stationStaffId);
            return Ok(new ResponseObject<StationStaffResponse>
            {
                Content = result,
                Message = "Get station staff assignment successfully",
                Code = "200",
                Success = true
            });
        }

        [HttpGet("station/{stationId}/all")]
        public async Task<ActionResult<ResponseObject<List<StationStaffResponse>>>> GetAllStaffsByStation(string stationId)
        {
            var result = await stationStaffService.GetAllStaffsByStationAsync(stationId);
            return Ok(new ResponseObject<List<StationStaffResponse>>
            {
                Content = result,
                Message = "Get all station staffs successfully",
                Code = "200",
                Success = true
            });
        }
    }
}