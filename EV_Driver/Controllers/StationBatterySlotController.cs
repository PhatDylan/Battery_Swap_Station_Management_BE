using BusinessObject.Dtos;
using BusinessObject.DTOs;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

namespace EV_Driver.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StationBatterySlotController(IStationBatterySlotService service) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<ResponseObject<List<StationBatterySlotResponse>>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var result = await service.GetAllStationSlotAsync(page, pageSize, search);
            return Ok(new ResponseObject<List<StationBatterySlotResponse>>
            {
                Message = "Get station battery slots successfully",
                Code = "200",
                Success = true
            }.UnwrapPagination(result));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseObject<StationBatterySlotResponse>>> GetById(string id)
        {
            var slot = await service.GetByIdAsync(id);
            if (slot == null)
            {
                return Ok(new ResponseObject<StationBatterySlotResponse>
                {
                    Message = "Station battery slot not found",
                    Code = "404",
                    Success = false,
                    Content = null
                });
            }

            return Ok(new ResponseObject<StationBatterySlotResponse>
            {
                Message = "Get station battery slot successfully",
                Code = "200",
                Success = true,
                Content = slot
            });
        }

        [HttpGet("station/{stationId}")]
        public async Task<ActionResult<ResponseObject<List<StationBatterySlotResponse>>>> GetByStation(string stationId, int page = 1, int pageSize = 10, string? search = null)
        {
            var slot = await service.GetByStationAsync(stationId, page, pageSize, search);
            return Ok(new ResponseObject<List<StationBatterySlotResponse>>
            {
                Message = "Get station battery slot by station successfully",
                Code = "200",
                Success = true
            }.UnwrapPagination(slot));
        }

        [HttpGet("station/{stationId}/enable")]
        public async Task<ActionResult<ResponseObject<List<StationBatterySlotResponse>>>> GetValidSlotsByStation(string stationId, int page = 1, int pageSize = 10, string? search = null)
        {
            var slot = await service.GetValidSlotByStationAsync(stationId, page, pageSize, search);
            return Ok(new ResponseObject<List<StationBatterySlotResponse>>
            {
                Message = "Get station battery slot by station successfully",
                Code = "200",
                Success = true
            }.UnwrapPagination(slot));
        }

        [HttpGet("details/all")]
        public async Task<ActionResult<ResponseObject<List<StationBatterySlotResponse>>>> GetAllDetails()
        {
            var result = await service.GetStationBatterySlotDetailAsync();
            return Ok(new ResponseObject<List<StationBatterySlotResponse>>
            {
                Message = "Get all station battery slot details successfully",
                Code = "200",
                Success = true,
                Content = result
            });
        }

        [HttpPost]
        public async Task<ActionResult<ResponseObject<object>>> Create([FromBody] StationBatterySlotRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await service.AddAsync(request);
            return Ok(new ResponseObject<object>
            {
                Message = "Station battery slot created successfully",
                Code = "200",
                Success = true,
                Content = null
            });
        }

        [HttpPut]
        public async Task<ActionResult<ResponseObject<object>>> Update([FromBody] StationBatterySlotRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await service.UpdateAsync(request);
            return Ok(new ResponseObject<object>
            {
                Message = "Station battery slot updated successfully",
                Code = "200",
                Success = true,
                Content = null
            });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseObject<object>>> Delete(string id)
        {
            await service.DeleteAsync(id);
            return Ok(new ResponseObject<object>
            {
                Message = "Station battery slot deleted successfully",
                Code = "200",
                Success = true,
                Content = null
            });
        }
    }
}