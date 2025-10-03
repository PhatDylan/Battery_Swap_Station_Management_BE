using BusinessObject.Dtos.UserDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.Interfaces;

namespace EV_Driver.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<ResponseObject<AuthResponseDto>>> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                throw new InvalidModelStateException(ModelState);
            }

            var result = await authService.RegisterAsync(registerDto);

            return Ok(new ResponseObject<AuthResponseDto>
            {
                Content = result,
                Message = "Register account successful",
                Code = "200",
                Success = true
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<ResponseObject<AuthResponseDto>>> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                throw new InvalidModelStateException(ModelState);
            }

            var result = await authService.LoginAsync(loginDto);

            return Ok(new ResponseObject<AuthResponseDto>
            {
                Content = result,
                Message = "Login successful",
                Code = "200",
                Success = true
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            return Ok(new ResponseObject<object>
            {
                Message = "Logout successful",
                Code = "200",
                Success = true
            });
        }
    }
}