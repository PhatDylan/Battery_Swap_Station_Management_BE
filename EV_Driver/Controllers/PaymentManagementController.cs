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
public class PaymentManagementController(IPaymentManagementService paymentService) : ControllerBase
{

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseObject<List<PaymentManagementResponse>>>> GetAllPayments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = await paymentService.GetAllPaymentsAsync(page, pageSize, search);
        return Ok(new ResponseObject<List<PaymentManagementResponse>>
        {
            Message = "Get all payments successfully",
            Code = "200",
            Success = true
        }.UnwrapPagination(result));
    }


    [HttpGet("station/{stationId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseObject<List<StationPaymentResponse>>>> GetPaymentsByStation(
        string stationId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = await paymentService.GetPaymentsByStationAsync(stationId, page, pageSize, search);
        return Ok(new ResponseObject<List<StationPaymentResponse>>
        {
            Message = $"Get payments for station successfully",
            Code = "200",
            Success = true
        }.UnwrapPagination(result));
    }
}