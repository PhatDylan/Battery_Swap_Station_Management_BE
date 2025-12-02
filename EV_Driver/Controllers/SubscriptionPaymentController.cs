using BusinessObject.Dtos;
using BusinessObject.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

namespace EV_Driver.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriptionPaymentController(ISubscriptionPaymentService service) : ControllerBase
    {
        [HttpPost("purchase")]
        public async Task<ActionResult<ResponseObject<CreateSubscriptionPaymentResponse>>> PurchaseSubscription(
            [FromBody] CreateSubscriptionPaymentRequest request)
        {
            var result = await service.CreateSubscriptionPaymentAsync(request);
            return Ok(new ResponseObject<CreateSubscriptionPaymentResponse>
            {
                Content = result,
                Message = "Subscription payment created successfully. Please complete payment.",
                Code = "200",
                Success = true
            });
        }
        
        [HttpGet("{subPayId}")]
        public async Task<ActionResult<ResponseObject<SubscriptionPaymentResponse>>> GetPaymentDetail(string subPayId)
        {
            var result = await service.GetSubscriptionPaymentDetailAsync(subPayId);
            return Ok(new ResponseObject<SubscriptionPaymentResponse>
            {
                Content = result,
                Message = "Get subscription payment detail successfully",
                Code = "200",
                Success = true
            });
        }
        
        [HttpGet("my-payments")]
        public async Task<ActionResult<ResponseObject<List<SubscriptionPaymentResponse>>>> GetMyPayments()
        {
            var result = await service.GetMySubscriptionPaymentsAsync();
            return Ok(new ResponseObject<List<SubscriptionPaymentResponse>>
            {
                Content = result,
                Message = "Get subscription payment history successfully",
                Code = "200",
                Success = true
            });
        }
    }
}
