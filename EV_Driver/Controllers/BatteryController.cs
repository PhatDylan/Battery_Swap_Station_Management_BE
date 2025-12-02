using BusinessObject.Dtos;
using BusinessObject.DTOs;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

namespace EV_Driver.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BatteryController(IBatteryService batteryService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<ResponseObject<List<BatteryResponse>>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var battery = await batteryService.GetAllBatteriesAsync(page, pageSize, search);
            return Ok(new ResponseObject<List<BatteryResponse>>
            {
                Message = "Get batteries successfully",
                Code = "200",
                Success = true
            }.UnwrapPagination(battery));
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseObject<BatteryResponse>>> GetById(string id)
        {
            var battery = await batteryService.GetByBatteryAsync(id);
            return Ok(new ResponseObject<BatteryResponse>
            {
                Message = "Get batteries successfully",
                Code = "200",
                Success = true,
                Content = battery
            });
        }

        // Lấy pin theo SerialNo 
        [HttpGet("serial/{serialNo:int}")]
        public async Task<ActionResult<ResponseObject<BatteryResponse>>> GetBySerial(int serialNo)
        {
            var battery = await batteryService.GetBySerialAsync(serialNo);
            return Ok(new ResponseObject<BatteryResponse>
            {
                Message = "Get batteries by serial number successfully",
                Code = "200",
                Success = true,
                Content = battery
            });
        }

        [HttpGet("station/{stationId}")]
        public async Task<ActionResult<ResponseObject<List<BatteryResponse>>>> GetByStation(string stationId, int page = 1, int pageSize = 10, string? search = null)
        {
            var battery = await batteryService.GetByStationAsync(stationId, page, pageSize, search);
            return Ok(new ResponseObject<List<BatteryResponse>>
            {
                Message = "Get Station ID successfully",
                Code = "200",
                Success = true
            }.UnwrapPagination(battery));
        }

        [HttpGet("station/{stationId}/storage")]
        public async Task<ActionResult<ResponseObject<List<BatteryResponse>>>> GetBatteryStorageByStation(string stationId, int page = 1, int pageSize = 10, string? search = null)
        {
            var battery = await batteryService.GetAllBatteryInStorage(stationId, page, pageSize, search);
            return Ok(new ResponseObject<List<BatteryResponse>>
            {
                Message = "Get Station ID successfully",
                Code = "200",
                Success = true
            }.UnwrapPagination(battery));
        }

        [HttpGet("available")]
        public async Task<ActionResult<ResponseObject<BatteryResponse>>> GetAvailable([FromQuery] string stationId)
        {
             var battery = await batteryService.GetAvailableAsync(stationId);
             return Ok(new ResponseObject<BatteryResponse>
             {
                 Message = "Get Available Battery by Station successfully",
                 Code = "200",
                 Success = true,
                 Content = battery
             });
        }
        
        [HttpPost]
        public async Task<ActionResult<ResponseObject<object>>> Add([FromBody] BatteryRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await batteryService.AddAsync(request);
            return Ok(new ResponseObject<object>
            {
                Message = "Battery created successfully",
                Code = "200",
                Success = true,
                Content = null
            });
        }
        
        [HttpPost("station/bulk")]
        public async Task<ActionResult<ResponseObject<object>>> AddBulkBatteryToStation([FromBody] IEnumerable<BatteryAddBulkStationRequest> request)
        {
            await batteryService.AddBatteryToStation(request);
            return Ok(new ResponseObject<object>
            {
                Message = "Battery add successfully",
                Code = "200",
                Success = true,
                Content = null
            });
        }

        [HttpPut]
        public async Task<ActionResult<ResponseObject<object>>> Update([FromBody] BatteryRequest request)
        {
             await batteryService.UpdateAsync(request);
             return Ok(new ResponseObject<object>
             {
                 Message = "Battery updated successfully",
                 Code = "200",
                 Success = true,
                 Content = null
             });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseObject<object>>> Delete(string id)
        {
             await batteryService.DeleteAsync(id);
             return Ok(new ResponseObject<object>
             {
                 Message = "Battery deleted successfully",
                 Code = "200",
                 Success = true,
                 Content = null
             });
        }
        // 1) Lấy battery không thuộc vehicle và không ở station nào
        [HttpGet("unassigned")]
        public async Task<ActionResult<ResponseObject<List<BatteryResponse>>>> GetUnassignedAndNotInStation([FromQuery] string? batteryTypeId)
        {
            var list = await batteryService.GetUnassignedAndNotInStationAsync(batteryTypeId);
            return Ok(new ResponseObject<List<BatteryResponse>>
            {
                Message = "Get unassigned batteries successfully",
                Code = "200",
                Success = true,
                Content = list
            });
        }
        
        
        [HttpGet("station/{stationId}/assigned")]
        public async Task<ActionResult<ResponseObject<List<BatteryResponse>>>> GetBatteryAssignedByStationId(string stationId, int page = 1, int pageSize = 10, string? search = null)
        {
            var list = await batteryService.GetBatteryAssignedByStationIdAsync(stationId, page, pageSize, search);
            return Ok(new ResponseObject<List<BatteryResponse>>
            {
                Message = "Get assigned batteries successfully",
                Code = "200",
                Success = true,
            }.UnwrapPagination(list));
        }

        // 2) Gắn pin vào vehicle
        [HttpPost("attach")]
        public async Task<ActionResult<ResponseObject<object>>> AttachBatteryToVehicle([FromBody] BatteryAttachRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await batteryService.AttachBatteryToVehicleAsync(request);
                return Ok(new ResponseObject<object>
                {
                    Message = "Battery attached to vehicle successfully",
                    Code = "200",
                    Success = true,
                    Content = null
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResponseObject<object> { Message = ex.Message, Code = "400", Success = false, Content = null });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ResponseObject<object> { Message = ex.Message, Code = "409", Success = false, Content = null });
            }
            catch (Exception ex)
            {
                // log ex
                return StatusCode(500, new ResponseObject<object> { Message = "Internal server error", Code = "500", Success = false, Content = null });
            }
        }
    }
}