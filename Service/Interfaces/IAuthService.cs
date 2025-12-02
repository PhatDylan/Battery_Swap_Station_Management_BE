using BusinessObject.DTOs;

namespace Service.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
    Task ActivateAccountAsync(string otp);
    Task ResendRegisterOtp();
}