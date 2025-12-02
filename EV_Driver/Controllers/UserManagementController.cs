using BusinessObject.DTOs;
using BusinessObject.Enums;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

namespace EV_Driver.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserManagementController(IUserManagementService userManagementService) : ControllerBase
    {
        [HttpPut("promote-to-staff/{userId}")]
        public async Task<ActionResult<ResponseObject<UserProfileResponse>>> PromoteToStaff(string userId)
        {
            var result = await userManagementService.PromoteUserToStaffAsync(userId);
            return Ok(new ResponseObject<UserProfileResponse>
            {
                Content = result,
                Message = "User promoted to staff successfully",
                Code = "200",
                Success = true
            });
        }

        [HttpPut("demote-to-user/{userId}")]
        public async Task<ActionResult<ResponseObject<UserProfileResponse>>> DemoteToUser(string userId)
        {
            var result = await userManagementService.DemoteStaffToUserAsync(userId);
            return Ok(new ResponseObject<UserProfileResponse>
            {
                Content = result,
                Message = "Staff demoted to user successfully",
                Code = "200",
                Success = true
            });
        }

        [HttpPost("create-staff")]
        public async Task<ActionResult<ResponseObject<UserProfileResponse>>> CreateStaff([FromBody] CreateStaffRequest request)
        {
            var result = await userManagementService.CreateStaffAccountAsync(request);
            return Ok(new ResponseObject<UserProfileResponse>
            {
                Content = result,
                Message = "Staff account created successfully",
                Code = "200",
                Success = true
            });
        }

        [HttpGet("users")]
        public async Task<ActionResult<ResponseObject<List<UserProfileResponse>>>> GetAllUsers(
            int page = 1,
            int pageSize = 10,
            string? search = null,
            UserRole? role = null)
        {
            var result = await userManagementService.GetAllUsersAsync(page, pageSize, search, role);
            return Ok(new ResponseObject<List<UserProfileResponse>>
            {
                Message = "Users retrieved successfully",
                Code = "200",
                Success = true
            }.UnwrapPagination(result));
        }
    }
}