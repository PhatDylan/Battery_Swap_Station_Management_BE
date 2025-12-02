using BusinessObject.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

namespace EV_Driver.Controllers
{
    [ApiController]
    [Route("api/staff/inventory")]
    [Authorize(Roles = "Staff")]

    public class StaffInventoryBatteryController(IStaffInventoryBatteryService inventoryService) : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet("summary")]
        public async Task<ActionResult<ResponseObject<BatteryInventorySummaryResponse>>> Summary(
            [FromQuery] BatteryInventorySummaryRequest request)
        {
            var data = await inventoryService.GetSummaryAsync(request);
            return Ok(new ResponseObject<BatteryInventorySummaryResponse>
            {
                Content = data,
                Message = "Get station inventory summary successfully",
                Code = "200",
                Success = true
            });
        }

        [AllowAnonymous]
        // Danh sách tồn kho có phân loại theo dung lượng/model/tình trạng
        [HttpGet]
        public async Task<ActionResult<ResponseObject<List<BatteryInventoryItemResponse>>>> List(
            [FromQuery] BatteryInventorySearchRequest request)
        {
            var result = await inventoryService.SearchAsync(request);

            return Ok(new ResponseObject<List<BatteryInventoryItemResponse>>
            {
                Message = "Get station inventory successfully",
                Code = "200",
                Success = true
            }.UnwrapPagination(result));
        }
    }
}