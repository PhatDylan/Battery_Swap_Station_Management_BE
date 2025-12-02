using BusinessObject.Dtos;
using BusinessObject.DTOs;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

namespace EV_Driver.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BatteryTypeController(IBatteryTypeService service) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<ResponseObject<List<BatteryTypeResponse>>>> GetAll(int page = 1, int pageSize = 10, string? search = null)
        {
            var result = await service.GetAllBatteryTypeAsync(page, pageSize, search);
            return Ok(new ResponseObject<List<BatteryTypeResponse>>
            {
                Message = "BatteryType get all successfully",
                Code = "200",
                Success = true
            }.UnwrapPagination(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ResponseObject<object>>> Add([FromBody] BatteryTypeRequest request)
        {
            await service.AddAsync(request);
            return Ok(new ResponseObject<object>
            {
                Message = "BatteryType created successfully",
                Code = "200",
                Success = true,
                Content = null
            });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseObject<object>>> Update(string id, [FromBody] BatteryTypeRequest request)
        {
            request.BatteryTypeId = id;
            await service.UpdateAsync(request);
            return Ok(new ResponseObject<object>
            {
                Message = "BatteryType updated successfully",
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
                Message = "BatteryType deleted successfully",
                Code = "200",
                Success = true,
                Content = null
            });
        }
    }
}