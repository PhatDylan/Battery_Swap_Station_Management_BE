using System.Net;
using BusinessObject;
using BusinessObject.Dtos;
using BusinessObject.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Service.Exceptions;
using Service.Interfaces;
using Service.Utils;

namespace Service.Implementations
{
    public class PasswordResetService(ApplicationDbContext context, IEmailService emailService, IEmailTemplateLoaderService emailTemplateLoaderService, IDistributedCache cache, IHttpContextAccessor accessor)
        : IPasswordResetService
    {
        

        public async Task RequestPasswordResetAsync(ForgotPasswordRequest request)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user is null)
            {
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400",
                    ErrorMessage = "Email not found."
                };
            }

            var otp = RandomUtils.GenerateOtp();
            var otpKey = "reset_otp" + otp;
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
            
            await cache.SetStringAsync(otpKey,user.UserId, cacheEntryOptions);

            const string subject = "EV Driver - OTP code to reset password";

            try
            {
                var loader = await emailTemplateLoaderService.RenderTemplateAsync("ResetPassword.cshtml",
                    new PasswordResetOtpModel
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
                    Code = "500",
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
        }

        public async Task ResentResetPasswordOtp()
        {
            var userId = JwtUtils.GetUserId(accessor);
            if (string.IsNullOrEmpty(userId))
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    ErrorMessage = "Unauthorized",
                    Code = "401"
                };
            var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user is null)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessage = "User not found.",
                    Code = "400"
                };
            var otp = RandomUtils.GenerateOtp();
            var otpKey = "reset_otp" + otp;
            var otpCheck = false;
            do
            {
                if (await cache.GetStringAsync(otpKey) is null)
                {
                    otpCheck = true;
                }
                else
                {
                    otp = RandomUtils.GenerateOtp();
                    otpKey = "reset_otp" + otp;
                }
            } while (!otpCheck);
            
            var cacheEntryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
            };
            
            try
            {
                await cache.SetStringAsync(otpKey,userId, cacheEntryOptions);
                const string subject = "EV Driver - OTP code to reset password";
                var loader = await emailTemplateLoaderService.RenderTemplateAsync("ResetPassword.cshtml",
                    new PasswordResetOtpModel
                    {
                        FullName = user.FullName,
                        Otp = otp,
                    });

                await emailService.SendEmailAsync(user.Email, subject, loader);
            }
            catch (Exception ex)
            {
                throw new ValidationException
                {
                    ErrorMessage = ex.Message,
                    Code = "500",
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest request)
        {
            if (request.NewPassword != request.ConfirmPassword)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400",
                    ErrorMessage = "NewPassword and ConfirmPassword do not match."
                };
            
            var otpKey = "reset_otp" + request.Otp;
            
            var userId = await cache.GetStringAsync(otpKey);
            if (userId is null)
            {
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400",
                    ErrorMessage = "Invalid OTP."
                };
            }
            
            var userEntity = await context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (userEntity is null)
            {
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400",
                    ErrorMessage = "User not found."
                };
            }

            userEntity.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            try
            {
                await context.SaveChangesAsync();
                await cache.RemoveAsync(otpKey);
            }
            catch (Exception ex)
            {
                throw new ValidationException
                {
                    ErrorMessage = ex.Message,
                    Code = "500",
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
        }

        public async Task ReassignPasswordForUser(string userId)
        {
            var authUserId = JwtUtils.GetUserId(accessor);
            if (string.IsNullOrEmpty(authUserId))
            {
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    ErrorMessage = "Unauthorized",
                    Code = "401"
                };
            }
            var checkUser = await context.Users.AnyAsync(u => u.UserId == userId && u.Role == UserRole.Admin);
            if (!checkUser)
            {
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessage = "User is not an admin.",
                    Code = "400"
                };
            }
            var userEntity = await context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (userEntity is null)
            {
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400",
                    ErrorMessage = "User not found."
                };
            }
            var randomPassword = RandomUtils.GeneratePassword();
            userEntity.Password = BCrypt.Net.BCrypt.HashPassword(randomPassword);
            try
            {
                await context.SaveChangesAsync();
                const string subject = "EV Driver - New password";
                var loader = await emailTemplateLoaderService.RenderTemplateAsync("PasswordReassign.cshtml",
                    new PasswordReassignModel
                    {
                        FullName = userEntity.FullName,
                        NewPassword = randomPassword
                    });
                await emailService.SendEmailAsync(userEntity.Email, subject, loader);
            }
            catch (Exception ex)
            {
                throw new ValidationException
                {
                    ErrorMessage = ex.Message,
                    Code = "500",
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
        }
    }
}