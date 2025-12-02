using System.Net;
using BusinessObject;
using BusinessObject.Dtos;
using BusinessObject.DTOs;
using BusinessObject.Entities;
using BusinessObject.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Service.Exceptions;
using Service.Interfaces;
using Service.Utils;

namespace Service.Implementations;

public class AuthService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IJwtService jwtService, IDistributedCache cache, IEmailService emailService, IEmailTemplateLoaderService emailTemplateLoaderService) : IAuthService
{
    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
    {
        if (await context.Users.AnyAsync(u => u.Email == registerDto.Email))
            throw new ValidationException
            {
                ErrorMessage = "This email is already exists",
                StatusCode = HttpStatusCode.BadRequest,
                Code = "400"
            };
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

        var user = new User
        {
            FullName = registerDto.FullName,
            Email = registerDto.Email,
            Phone = registerDto.Phone,
            Role = UserRole.Driver,
            Status = UserStatus.Inactive,
            Password = passwordHash
        };
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            throw new ValidationException
            {
                ErrorMessage = ex.Message,
                StatusCode = HttpStatusCode.BadRequest,
                Code = "400"
            };
        }
        
        var otp = RandomUtils.GenerateOtp();
        var otpKey = "register_otp" + otp;
        var otpCheck = false;
        do
        {
            var checkOtp = await cache.GetStringAsync(otpKey);
            if (checkOtp == otp)
            {
                otp = RandomUtils.GenerateOtp();
                otpKey = "register_otp" + otp;
            }
            else
            {
                otpCheck = true;
            }
        } while (!otpCheck);
        
        var cacheEntryOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
        };

        try
        {
            await cache.SetStringAsync(otpKey,user.UserId, cacheEntryOptions);
            const string subject = "EV Driver - OTP code to activate account";
            var loader = await emailTemplateLoaderService.RenderTemplateAsync("UserRegister.cshtml",
                new UserRegisterOtpModel
                {
                    FullName = user.FullName,
                    Otp = otp,
                });

            await emailService.SendEmailAsync(user.Email, subject, loader);
        }
        catch (Exception ex)
        {
            await cache.RemoveAsync(otpKey);
            await transaction.RollbackAsync();
            throw new ValidationException
            {
                ErrorMessage = ex.Message,
                StatusCode = HttpStatusCode.InternalServerError,
                Code = "500"
            };
        }
        var token = jwtService.GenerateJwtToken(user.UserId, user.Role.ToString());
        return new AuthResponseDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            Status = user.Status,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
    }

    public async Task ActivateAccountAsync(string otp)
    {
        var userId = JwtUtils.GetUserId(httpContextAccessor);
        if (string.IsNullOrEmpty(userId))
        {
            throw new ValidationException
            {
                ErrorMessage = "Unauthorized",
                StatusCode = HttpStatusCode.Unauthorized,
                Code = "401"
            };
        }
        var otpKey = "register_otp" + otp;
        var otpCheck = await cache.GetStringAsync(otpKey);
        if (otpCheck is null)
        {
            throw new ValidationException
            {
                ErrorMessage = "Invalid OTP",
                StatusCode = HttpStatusCode.BadRequest,
                Code = "400"
            };
        }

        if (otpCheck != userId)
        {
            throw new ValidationException
            {
                ErrorMessage = "User is not valid otp",
                StatusCode = HttpStatusCode.BadRequest,
                Code = "400"
            };
        }
        var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user is null)
        {
            throw new ValidationException
            {
                ErrorMessage = "User not found",
                StatusCode = HttpStatusCode.BadRequest,
                Code = "400"
            };
        }
        try
        {
            user.Status = UserStatus.Active;
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new ValidationException
            {
                ErrorMessage = ex.Message,
                StatusCode = HttpStatusCode.InternalServerError,
                Code = "500"
            };
        }
    }

    public async Task ResendRegisterOtp()
    {
        var userId = JwtUtils.GetUserId(httpContextAccessor);
        if (string.IsNullOrEmpty(userId))
        {
            throw new ValidationException
            {
                ErrorMessage = "Unauthorized",
                StatusCode = HttpStatusCode.Unauthorized,
                Code = "401"
            };
        }
        var user = await context.Users.Select(u =>
        new {
            u.UserId, 
            u.Email,
            u.FullName
        }).FirstOrDefaultAsync(u => u.UserId == userId);
        if (user is null)
        {
            throw new ValidationException
            {
                ErrorMessage = "User not found",
                StatusCode = HttpStatusCode.BadRequest,
                Code = "400"
            };
        }
        var otp = RandomUtils.GenerateOtp();
        var otpKey = "register_otp" + otp;
        var otpCheck = false;
        do
        {
            var checkOtp = await cache.GetStringAsync(otpKey);
            if (checkOtp == otp)
            {
                otp = RandomUtils.GenerateOtp();
                otpKey = "reset_otp" + otp;
            }
            else
            {
                otpCheck = true;
            }
        } while (!otpCheck);
        
        var cacheEntryOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
        };
        
        try
        {
            await cache.SetStringAsync(otpKey,userId, cacheEntryOptions);
            const string subject = "EV Driver - OTP code to activate account";
            var loader = await emailTemplateLoaderService.RenderTemplateAsync("UserRegister.cshtml",
                new UserRegisterOtpModel
                {
                    FullName = user.FullName,
                    Otp = otp,
                });

            await emailService.SendEmailAsync(user.Email, subject, loader);
        }
        catch (Exception ex)
        {
            await cache.RemoveAsync(otpKey);
            throw new ValidationException
            {
                ErrorMessage = ex.Message,
                StatusCode = HttpStatusCode.InternalServerError,
                Code = "500"
            };
        }
    }


    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

        if (user == null)
            throw new ValidationException
            {
                ErrorMessage = "Invalid email",
                StatusCode = HttpStatusCode.Unauthorized,
                Code = "401"
            };

        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
            throw new ValidationException
            {
                ErrorMessage = "Invalid email or password",
                StatusCode = HttpStatusCode.Unauthorized,
                Code = "401"
            };

        if (user.Status == UserStatus.Suspended)
            throw new ValidationException
            {
                ErrorMessage = "User is banned, please contact admin",
                StatusCode = HttpStatusCode.Unauthorized,
                Code = "401"
            };

        var token = jwtService.GenerateJwtToken(user.UserId, user.Role.ToString());

        return new AuthResponseDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            Status = user.Status,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
    }
}