using System.Net;
using System.Text.Json;
using BusinessObject.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;
using Service.Exceptions;
using Service.Interfaces;

namespace EV_Driver.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController(IPaymentService paymentService): ControllerBase
{

    [HttpPost("init")]
    public async Task<ActionResult<ResponseObject<CreatePaymentResponse>>> CreatePaymentAsync(CreatePaymentRequest request)
    {
        var result = await paymentService.CreatePaymentAsync(request);
        return Ok(new ResponseObject<CreatePaymentResponse>
        {
            Content = result,
            Message = "Create payment successfully",
            Code = "200",
            Success = true
        });
    }
    
    [HttpGet("{paymentId}")]
    public async Task<ActionResult<ResponseObject<PaymentResponse>>> GetPaymentAsync (string paymentId)
    {
        var result = await paymentService.GetPaymentDetailAsync(paymentId);
        return Ok(new ResponseObject<PaymentResponse>
        {
            Content = result,
            Message = "Get payment successfully",
            Code = "200",
            Success = true
        });
    }
    
    [HttpPost("hook")]
    [AllowAnonymous]
    public async Task<IActionResult> HandlePaymentWebhook()
    {
        var payload = await new StreamReader(Request.Body).ReadToEndAsync();
        var webhookRequest = JsonSerializer.Deserialize<WebhookType>(payload, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        });

        if (webhookRequest == null)
            return BadRequest("Invalid payload");
        
        if (!paymentService.ValidatePayOsSignature(webhookRequest))
        {
            return Unauthorized("Invalid signature");
        }
        
        try
        {
            await paymentService.ProcessPayOsPaymentAsync(webhookRequest.data, webhookRequest.success);
        }
        catch(ValidationException ex)
        {
            return ex.StatusCode switch
            {
                HttpStatusCode.InternalServerError => StatusCode(500),
                HttpStatusCode.NotFound => NotFound(ex.ErrorMessage),
                _ => StatusCode(208)
            };
        }
        return Ok();
    }

    [HttpGet("my-swap-payment")]
    public async Task<ActionResult<ResponseObject<PaymentResponse>>> GetPaymentByUserAsync(string userId)
    {
        var result = await paymentService.GetPaymentDetailByDriverAsync(userId);
        return Ok(new ResponseObject<PaymentResponse>
        {
            Content = result,
            Message = "Get payment successfully",
            Code = "200",
            Success = true
        });
    }

    [HttpGet("me-pay")]
    [Authorize]
    public async Task<ActionResult<ResponseObject<List<PaymentResponse>>>> GetMyPayments()
    {
        var result = await paymentService.GetMyPaymentsAsync();
        return Ok(new ResponseObject<List<PaymentResponse>>
        {
            Content = result,
            Message = "Get payment history successfully",
            Code = "200",
            Success = true
        });
    }
}