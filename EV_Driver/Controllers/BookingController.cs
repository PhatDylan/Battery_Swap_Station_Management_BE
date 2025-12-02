using BusinessObject.DTOs;
using BusinessObject.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations;
using Service.Interfaces;
using Service.Exceptions;

namespace EV_Driver.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingController(IBookingService bookingService) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ResponseObject<object>>> CreateBooking(CreateBookingRequest request)
    {
        await bookingService.CreateBooking(request);
        return Ok(new ResponseObject<string>
        {
            Message = "Booking created successfully",
            Code = "200",
            Success = true
        });
    }

    [HttpGet]
    public async Task<ActionResult<ResponseObject<List<BookingResponse>>>> GetAllBookingAsync([FromQuery] int page, [FromQuery] int size, [FromQuery] string? search)
    {
        var result = await bookingService.GetAllBookingAsync(page, size, search);
        return Ok(new ResponseObject<List<BookingResponse>>
        {
            Message = "Get booking successfully",
            Code = "200",
            Success = true
        }.UnwrapPagination(result));
    }
    
    [HttpGet("me")]
    public async Task<ActionResult<ResponseObject<List<BookingResponse>>>> GetAllMyBookingAsync([FromQuery] int page, [FromQuery] int size, [FromQuery] string? search)
    {
        var result = await bookingService.GetAllMyBookingAsync(page, size, search);
        return Ok(new ResponseObject<List<BookingResponse>>
        {
            Message = "Get booking successfully",
            Code = "200",
            Success = true
        }.UnwrapPagination(result));
    }
    
    [HttpGet("station/{stationId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseObject<List<BookingResponse>>>> GetAllStationBookingAsync(string stationId, [FromQuery] int page, [FromQuery] int size, [FromQuery] string? search)
    {
        var result = await bookingService.GetAllStationBookingAsync(stationId, page, size, search);
        return Ok(new ResponseObject<List<BookingResponse>>
        {
            Message = "Get booking successfully",
            Code = "200",
            Success = true
        }.UnwrapPagination(result));
    }
    
    [HttpGet("station/me")]
    [Authorize]
    public async Task<ActionResult<ResponseObject<List<BookingResponse>>>> GetAllStationBookingAsync([FromQuery] int page, [FromQuery] int size, [FromQuery] string? search)
    {
        var result = await bookingService.GetAllMyStationBookingAsync(page, size, search);
        return Ok(new ResponseObject<List<BookingResponse>>
        {
            Message = "Get booking successfully",
            Code = "200",
            Success = true
        }.UnwrapPagination(result));
    }

    [HttpGet("{bookingId}")]
    public async Task<ActionResult<ResponseObject<BookingResponse>>> GetBookingById(string bookingId)
    {
        var result = await bookingService.GetBookingAsync(bookingId);
        return Ok(new ResponseObject<BookingResponse>
        {
            Content = result,
            Message = "Get booking successfully",
            Code = "200",
            Success = true
        });
    }

    [HttpGet("station/{stationId}/pending")]
    [Authorize(Roles = "Staff")]
    public async Task<ActionResult<ResponseObject<List<BookingResponse>>>> GetAllStationBookingAsync(
    string stationId,
    [FromQuery] int page,
    [FromQuery] int size,
    [FromQuery] string? search,
    [FromQuery] BBRStatus? status = null)
    {
        var result = await bookingService.GetAllStationBookingAsync(stationId, page, size, search, status);
        return Ok(new ResponseObject<List<BookingResponse>>
        {
            Message = "Get booking successfully",
            Code = "200",
            Success = true
        }.UnwrapPagination(result));
    }

    [HttpGet("station/me/status")]
    [Authorize]
    public async Task<ActionResult<ResponseObject<List<BookingResponse>>>> GetAllMyStationBookingAsync(
        [FromQuery] int page,
        [FromQuery] int size,
        [FromQuery] string? search,
        [FromQuery] BBRStatus? status = null) // ? Th�m query parameter
    {
        var result = await bookingService.GetAllMyStationBookingAsync(page, size, search, status);
        return Ok(new ResponseObject<List<BookingResponse>>
        {
            Message = "Get booking successfully",
            Code = "200",
            Success = true
        }.UnwrapPagination(result));
    }
    [HttpPut("{bookingId}/reject")]
    public async Task<ActionResult<ResponseObject<object>>> Reject(string bookingId, [FromBody] StaffRejectSwapRequest request, CancellationToken ct)
    {
        await bookingService.RejectSwapAsync(bookingId, request, ct);
        return Ok(new ResponseObject<object>
        {
            Message = "Swap rejected successfully",
            Code = "200",
            Success = true
        });
    }

    [HttpPut("{bookingId}/confirm")]
    public async Task<ActionResult<ResponseObject<object>>> Confirm(string bookingId, CancellationToken ct)
    {
        await bookingService.ConfirmSwapAsync(bookingId, ct);
        return Ok(new ResponseObject<object>
        {
            Message = "Swap confirmed successfully",
            Code = "200",
            Success = true
        });
    }
    [HttpPost("estimate-price")]
    public async Task<ActionResult<ResponseObject<EstimatePriceResponse>>> EstimatePrice(
            [FromBody] EstimatePriceRequest request)
    {
        try
        {
            var result = await bookingService.CalculateEstimatedPriceAsync(request);
            return Ok(new ResponseObject<EstimatePriceResponse>
            {
                Content = result,
                Message = "Calculate estimated price successfully",
                Code = "200",
                Success = true
            });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ResponseObject<EstimatePriceResponse>
            {
                Content = null,
                Message = ex.ErrorMessage,
                Code = ex.Code,
                Success = false
            });
        }
    }
}