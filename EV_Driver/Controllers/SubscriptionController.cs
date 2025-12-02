using BusinessObject.Dtos;
using BusinessObject.DTOs;
using BusinessObject.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.Implementations;
using Service.Interfaces;
using Service.Utils;
using System.Net;

// ✅ Thêm using này
// ✅ Thêm using này

namespace EV_Driver.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionController(
        ISubscriptionService service,
        IHttpContextAccessor accessor) : ControllerBase  // ✅ Thêm accessor
    {
        [HttpGet]
        public async Task<ActionResult<ResponseObject<List<SubscriptionResponse>>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var result = await service.GetAllSubscriptionAsync(page, pageSize, search);
            return Ok(new ResponseObject<List<SubscriptionResponse>>
            {
                Message = "Get subscriptions successfully",
                Code = "200",
                Success = true
            }.UnwrapPagination(result));
        }

        [HttpGet("details")]
        public async Task<ActionResult<ResponseObject<List<SubscriptionResponse>>>> GetSubscriptionDetails()
        {
            var subscriptions = await service.GetSubscriptionDetailAsync();
            return Ok(new ResponseObject<List<SubscriptionResponse>>
            {
                Message = "Get subscription details successfully",
                Code = "200",
                Success = true,
                Content = subscriptions
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseObject<SubscriptionResponse>>> GetById(string id)
        {
            var result = await service.GetBySubscriptionAsync(id);
            if (result == null)
            {
                return Ok(new ResponseObject<SubscriptionResponse>
                {
                    Message = "Subscription not found",
                    Code = "404",
                    Success = false,
                    Content = null
                });
            }

            return Ok(new ResponseObject<SubscriptionResponse>
            {
                Message = "Get subscription successfully",
                Code = "200",
                Success = true,
                Content = result
            });
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ResponseObject<SubscriptionResponse>>> GetByUser(string userId)
        {
            var result = await service.GetByUserAsync(userId);
            return Ok(new ResponseObject<SubscriptionResponse>
            {
                Message = "Get subscription by user successfully",
                Code = "200",
                Success = true,
                Content = result
            });
        }
        
        [HttpPut("{subscriptionId}/cancel")]
        public async Task<ActionResult<ResponseObject<SubscriptionResponse>>> CancelUserSubscription(string subscriptionId)
        {
            var result = await service.CancelSubscription(subscriptionId);
            return Ok(new ResponseObject<SubscriptionResponse>
            {
                Message = "Cancel subscription by user successfully",
                Code = "200",
                Success = true,
                Content = result
            });
        }

        // ✅ NEW ENDPOINT - Get my subscription
        [HttpGet("by-user/me")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult<ResponseObject<SubscriptionResponse>>> GetMyActiveSubscription()
        {
            var userId = JwtUtils.GetUserId(accessor);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ResponseObject<SubscriptionResponse>
                {
                    Message = "Unauthorized",
                    Code = "401",
                    Success = false,
                    Content = null
                });
            }

            try
            {
                var result = await service.GetByUserAsync(userId);
                return Ok(new ResponseObject<SubscriptionResponse>
                {
                    Message = "Get my active subscription successfully",
                    Code = "200",
                    Success = true,
                    Content = result
                });
            }
            catch (ValidationException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return Ok(new ResponseObject<SubscriptionResponse>
                {
                    Message = "No active subscription found",
                    Code = "404",
                    Success = false,
                    Content = null
                });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ResponseObject<object>>> Add([FromBody] SubscriptionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await service.AddAsync(request);
            return Ok(new ResponseObject<object>
            {
                Message = "Subscription created successfully",
                Code = "200",
                Success = true,
                Content = null
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseObject<object>>> Update(string id, [FromBody] SubscriptionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            request.SubscriptionId = id;
            await service.UpdateAsync(request);
            return Ok(new ResponseObject<object>
            {
                Message = "Subscription updated successfully",
                Code = "200",
                Success = true,
                Content = null
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseObject<object>>> Delete(string id)
        {
            await service.DeleteAsync(id);
            return Ok(new ResponseObject<object>
            {
                Message = "Subscription deleted successfully",
                Code = "200",
                Success = true,
                Content = null
            });
        }

        // ✅ BONUS: Admin/Staff check user's active subscription
        [HttpGet("user/{userId}/active")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<ActionResult<ResponseObject<SubscriptionResponse>>> GetActiveSubscriptionByUser(string userId)
        {
            try
            {
                var result = await service.GetByUserAsync(userId);

                // Check if subscription is still active and not expired
                if (result.Status != SubscriptionStatus.Active || result.EndDate <= DateTime.UtcNow)
                {
                    return Ok(new ResponseObject<SubscriptionResponse>
                    {
                        Message = "No active subscription found",
                        Code = "404",
                        Success = false,
                        Content = null
                    });
                }

                return Ok(new ResponseObject<SubscriptionResponse>
                {
                    Message = "Get active subscription successfully",
                    Code = "200",
                    Success = true,
                    Content = result
                });
            }
            catch (ValidationException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return Ok(new ResponseObject<SubscriptionResponse>
                {
                    Message = "No active subscription found",
                    Code = "404",
                    Success = false,
                    Content = null
                });
            }
        }
     
        [HttpGet("purchases")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseObject<List<SubscriptionPurchaseResponse>>>> GetAllSubscriptionPurchases(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            var result = await service.GetAllSubscriptionPurchasesAsync(page, pageSize, search);
            return Ok(new ResponseObject<List<SubscriptionPurchaseResponse>>
            {
                Message = "Get all subscription purchases successfully",
                Code = "200",
                Success = true
            }.UnwrapPagination(result));
        }
    }
}