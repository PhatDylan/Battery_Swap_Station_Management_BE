using BusinessObject.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Exceptions;

namespace EV_Driver.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehicleController(IVehicleService vehicleService, IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ResponseObject<List<VehicleResponse>>>> GetUserVehicle(
        int page = 1, 
        int pageSize = 10, 
        string? search = null)
    {
        var vehicle = await vehicleService.GetAllVehiclesAsync(page, pageSize, search);
        return Ok(new ResponseObject<List<VehicleResponse>>
        {
            Message = "Get vehicle successfully",
            Code = "200",
            Success = true
        }.UnwrapPagination(vehicle));
    }


    [HttpGet("by-user/{userId}")]
    public async Task<ActionResult<ResponseObject<List<VehicleResponse>>>> GetVehicleByUser(
       string userId, int page = 1, int pageSize = 10, string? search = null)
    {
        var vehicles = await vehicleService.GetVehiclesByUserAsync(userId, page, pageSize, search);
        return Ok(new ResponseObject<List<VehicleResponse>>
        {
            Message = "Get vehicle by user successfully",
            Code = "200",
            Success = true
        }.UnwrapPagination(vehicles));
    }

    [Authorize]
    [HttpGet("by-user/me")]
    public async Task<ActionResult<List<ResponseObject<VehicleResponse>>>> GetMyVehicle(int page = 1, int pageSize = 10, string? search = null)
    {
        var user = await userService.GetMeProfileAsync();
        var vehicles = await vehicleService.GetVehiclesByUserAsync(user.UserId, page, pageSize, search);
        return Ok(new ResponseObject<List<VehicleResponse>>
        {
            Message = "Get vehicle by user successfully",
            Code = "200",
            Success = true
        }.UnwrapPagination(vehicles));
    }

    [HttpPost]
    public async Task<ActionResult<ResponseObject<VehicleResponse>>> CreateUserVehicle(
        [FromBody] VehicleRequest request)
    {
        var vehicle = await vehicleService.CreateVehicleAsync(request);
        return Ok(new ResponseObject<VehicleResponse>
        {
            Content = vehicle,
            Message = "Create vehicle successfully",
            Code = "200",
            Success = true
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ResponseObject<VehicleResponse>>> UpdateUserVehicle(string id,
        [FromBody] VehicleRequest request)
    {
        var vehicle = await vehicleService.UpdateVehicleAsync(id, request);
        return Ok(new ResponseObject<VehicleResponse>
        {
            Content = vehicle,
            Message = "Update vehicle successfully",
            Code = "200",
            Success = true
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ResponseObject<VehicleResponse>>> GetUserVehicle(string id)
    {
        var vehicle = await vehicleService.GetVehicleAsync(id);
        return Ok(new ResponseObject<VehicleResponse>
        {
            Content = vehicle,
            Message = "Get vehicle successfully",
            Code = "200",
            Success = true
        });
    }
    [HttpDelete("{id}")]
    public async Task<ActionResult<ResponseObject<VehicleResponse>>> DeleteUserVehicle(string id)
    {
        try
        {
            var vehicle = await vehicleService.DeleteVehicleAsync(id);
            return Ok(new ResponseObject<VehicleResponse>
            {
                Content = vehicle,
                Message = "Delete vehicle successfully. All attached batteries have been unlinked.",
                Code = "200",
                Success = true
            });
        }
        catch (ValidationException ex)
        {
            return StatusCode((int)ex.StatusCode, new ResponseObject<VehicleResponse>
            {
                Message = ex.ErrorMessage,
                Code = ex.Code,
                Success = false,
                Content = null
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ResponseObject<VehicleResponse>
            {
                Message = ex.Message,
                Code = "400",
                Success = false,
                Content = null
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResponseObject<VehicleResponse>
            {
                Message = "Internal server error",
                Code = "500",
                Success = false,
                Content = null
            });
        }
    }
}