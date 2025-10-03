<<<<<<< HEAD
﻿using System.Net;
using BusinessObject;
using BusinessObject.DTOs;
=======
﻿using BusinessObject;
using BusinessObject.Dtos.UserDtos;
>>>>>>> fd951b34f67a65aa965bfa7a74835b0380a1ee2b
using BusinessObject.Entities;
using BusinessObject.Enums;
using Microsoft.EntityFrameworkCore;
using Service.Exceptions;
using Service.Interfaces;

namespace Service.Implementations
{
    public class AuthService(ApplicationDbContext context, IJwtService jwtService) : IAuthService
    {
        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            if (await context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                throw new ValidationException
                {
                    ErrorMessage = "This email is already exists",
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400"
                };
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            var user = new User
            {
                FullName = registerDto.FullName,
                Email = registerDto.Email,
                Phone = registerDto.Phone,
                Role = registerDto.Role,
                Status = UserStatus.Active,
                Password = passwordHash
            };

            try
            {
                context.Users.Add(user);
                await context.SaveChangesAsync();
                var token =  jwtService.GenerateJwtToken(user.UserId, user.Role.ToString());
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
            catch (Exception ex)
            {
                throw new ValidationException
                {
                    ErrorMessage = ex.Message,
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400"
                };
            }
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null)
            {
                throw new ValidationException
                {
                    ErrorMessage = "Invalid email",
                    StatusCode = HttpStatusCode.Unauthorized,
                    Code = "401"
                };
            }

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
            {
                throw new ValidationException
                {
                    ErrorMessage = "Invalid email or password",
                    StatusCode = HttpStatusCode.Unauthorized,
                    Code = "401"
                };
            }
            
            if (user.Status != UserStatus.Active)
            {
                throw new ValidationException {
                    ErrorMessage = "User is disabled or banned, please contact admin",
                    StatusCode = HttpStatusCode.Unauthorized,
                    Code = "401"
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
    }
}
