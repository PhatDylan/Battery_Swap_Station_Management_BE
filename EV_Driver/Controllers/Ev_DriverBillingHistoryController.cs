using System.Net;
using BusinessObject.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

namespace EV_Driver.Controllers;

[ApiController]
[Route("api/my/billing")]
[Authorize]
public class Ev_DriverBillingHistoryController(IBillingHistoryService billingService) : ControllerBase
{
    // Payments list
    [HttpGet("payments")]
    public async Task<ActionResult<ResponseObject<MyPaymentsListResponse>>> GetMyPayments([FromQuery] MyPaymentsListRequest request)
    {
        var resp = await billingService.GetMyPaymentsAsync(request);

        return Ok(new ResponseObject<MyPaymentsListResponse>
        {
            Content = resp,
            Message = "Get my payments successfully",
            Code = "200",
            Success = true
        });
    }

    // Payment detail
    [HttpGet("payments/{payId}")]
    public async Task<ActionResult<ResponseObject<PaymentResponse>>> GetMyPaymentById(string payId)
    {
        var payment = await billingService.GetMyPaymentByIdAsync(payId);
        if (payment == null)
        {
            return NotFound(new ResponseObject<PaymentResponse>
            {
                Content = null,
                Message = "Payment not found",
                Code = ((int)HttpStatusCode.NotFound).ToString(),
                Success = false
            });
        }

        return Ok(new ResponseObject<PaymentResponse>
        {
            Content = payment,
            Message = "Get my payment successfully",
            Code = "200",
            Success = true
        });
    }

    // Transaction history list
    [HttpGet("transactions")]
    public async Task<ActionResult<ResponseObject<TransactionHistoryListResponse>>> GetMyTransactions([FromQuery] TransactionHistoryQueryRequest request)
    {
        var resp = await billingService.GetMyTransactionHistoryAsync(request);

        return Ok(new ResponseObject<TransactionHistoryListResponse>
        {
            Content = resp,
            Message = "Get my transactions successfully",
            Code = "200",
            Success = true
        });
    }
}