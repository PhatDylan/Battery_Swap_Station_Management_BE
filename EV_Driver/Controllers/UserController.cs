using BusinessObject.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

namespace EV_Driver.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController(IUserService userService): ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<ResponseObject<UserProfileResponse>>> GetMeUserProfile()
    {
        var user = await userService.GetMeProfileAsync();
        return Ok(new ResponseObject<UserProfileResponse>
        {
            Content = user,
            Message = "User profile retrieved successfully",
            Code = "200",
            Success = true
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ResponseObject<UserProfileResponse>>> GetProfileById(string id)
    {
        var user = await userService.GetUserProfileAsync(id);
        return Ok(new ResponseObject<UserProfileResponse>
        {
            Content = user,
            Message = "User profile retrieved successfully",
            Code = "200",
            Success = true
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ResponseObject<UserProfileResponse>>> UpdateUserProfile(string id, [FromBody] UserProfileRequest request)
    {
        var user = await userService.UpdateUserProfileResponse(id, request);
        return Ok(new ResponseObject<UserProfileResponse>
        {
            Content = user,
            Message = "User profile updated successfully",
            Code = "200",
            Success = true
        });
    }
    [HttpPut("me")]
    public async Task<ActionResult<ResponseObject<UserProfileResponse>>> UpdateMeProfile([FromBody] UserProfileRequest request)
    {
        var user = await userService.UpdateMeProfileAsync(request);
        return Ok(new ResponseObject<UserProfileResponse>
        {
            Content = user,
            Message = "User profile updated successfully",
            Code = "200",
            Success = true
        });
    }
    
    [HttpPut("me/password")]
    public async Task<ActionResult<ResponseObject<object>>> UpdateMePassword([FromBody] ChangePasswordRequest request)
    {
        await userService.UpdatePassword(request);
        return Ok(new ResponseObject<object>
        {
            Message = "Password updated successfully",
            Code = "200",
            Success = true
        });
    }

    [HttpGet]
    public async Task<ActionResult<ResponseObject<List<UserProfileResponse>>>> GetAllUserProfile(int page, int size,
        string? search)
    {
        var response = await userService.GetAllUsersAsync(page, size, search);
        return Ok(new ResponseObject<List<UserProfileResponse>>
        {
            Message = "User profile retrieved successfully",
            Code = "200",
            Success = true
        }.UnwrapPagination(response));
    }
}