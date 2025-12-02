using BusinessObject.Dtos;
using BusinessObject.DTOs;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

namespace EV_Driver.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionPlanController(ISubscriptionPlanService service) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<ResponseObject<List<SubscriptionPlanResponse>>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var result = await service.GetAllSubscriptionPlanAsync(page, pageSize, search);
            return Ok(new ResponseObject<List<SubscriptionPlanResponse>>
            {
                Message = "Get subscription plans successfully",
                Code = "200",
                Success = true
            }.UnwrapPagination(result));
        }

        [HttpGet("latest")]
        public async Task<ActionResult<ResponseObject<SubscriptionPlanResponse>>> GetLatest()
        {
            var result = await service.GetAllAsync();
            return Ok(new ResponseObject<SubscriptionPlanResponse>
            {
                Message = "Get latest subscription plan successfully",
                Code = "200",
                Success = true,
                Content = result
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseObject<SubscriptionPlanResponse>>> GetById(string id)
        {
            var result = await service.GetByIdAsync(id);
            if (result == null)
            {
                return Ok(new ResponseObject<SubscriptionPlanResponse>
                {
                    Message = "Subscription plan not found",
                    Code = "404",
                    Success = false,
                    Content = null
                });
            }

            return Ok(new ResponseObject<SubscriptionPlanResponse>
            {
                Message = "Get subscription plan successfully",
                Code = "200",
                Success = true,
                Content = result
            });
        }

        [HttpPost]
        public async Task<ActionResult<ResponseObject<object>>> Add([FromBody] SubscriptionPlanRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await service.AddAsync(request);
            return Ok(new ResponseObject<object>
            {
                Message = "Subscription plan created successfully",
                Code = "200",
                Success = true,
                Content = null
            });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseObject<object>>> Update(string id, [FromBody] SubscriptionPlanRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            request.PlanId = id;
            await service.UpdateAsync(request);
            return Ok(new ResponseObject<object>
            {
                Message = "Subscription plan updated successfully",
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
                Message = "Subscription plan deleted successfully",
                Code = "200",
                Success = true,
                Content = null
            });
        }
    }
}