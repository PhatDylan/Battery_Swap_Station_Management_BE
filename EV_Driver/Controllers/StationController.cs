using BusinessObject.Dtos;
using BusinessObject.DTOs;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

namespace EV_Driver.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StationController(IStationService stationService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<ResponseObject<List<StationResponse>>>> GetUserStation(int page, int pageSize, string? search)
        {
            var station = await stationService.GetAllStationsAsync(page, pageSize, search);
            return Ok(new ResponseObject<List<StationResponse>>
            {
                Message = "Get station successfully",
                Code = "200",
                Success = true
            }.UnwrapPagination(station));
        }

        [HttpPost]
        public async Task<ActionResult<ResponseObject<StationResponse>>> CreateUserStation([FromBody] StationRequest request)
        {
            var station = await stationService.CreateStationAsync(request);
            return Ok(new ResponseObject<StationResponse>
            {
                Content = station,
                Message = "Create station successfully",
                Code = "200",
                Success = true
            });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseObject<StationResponse>>> UpdateUserStation(string id, [FromBody] StationRequest request)
        {
            var station = await stationService.UpdateStationAsync(id, request);
            return Ok(new ResponseObject<StationResponse>
            {
                Content = station,
                Message = "Update station successfully",
                Code = "200",
                Success = true
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseObject<StationResponse>>> GetUserStation(string id)
        {
            var station = await stationService.GetStationAsync(id);
            return Ok(new ResponseObject<StationResponse>
            {
                Content = station,
                Message = "Get station successfully",
                Code = "200",
                Success = true
            });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseObject<StationResponse>>> DeleteUserStation(string id)
        {
            var station = await stationService.DeleteStationAsync(id);
            return Ok(new ResponseObject<StationResponse>
            {
                Content = station,
                Message = "Delete station successfully",
                Code = "200",
                Success = true
            });
        }

        [HttpGet("available")]
        public async Task<ActionResult<ResponseObject<List<StationAvailabilityDto>>>> GetAvailable(
        [FromQuery] string batteryTypeId,
        [FromQuery] bool excludeFull = true)
        {
            if (string.IsNullOrWhiteSpace(batteryTypeId))
            {
                return BadRequest(new ResponseObject<object>
                {
                    Message = "batteryTypeId is required.",
                    Code = "400",
                    Success = false,
                    Content = null
                });
            }

            var stations = await stationService.GetStationsWithBatteryTypeAvailableAsync(batteryTypeId, excludeFull);

            return Ok(new ResponseObject<List<StationAvailabilityDto>>
            {
                Message = "Get available stations successfully",
                Code = "200",
                Success = true,
                Content = stations
            });
        }
    }
}