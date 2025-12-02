using BusinessObject;
using BusinessObject.DTOs;
using BusinessObject.Entities;
using BusinessObject.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Utils;
using System.Globalization;
using System.Net;
using Service.Exceptions;

namespace Service.Implementations;

public class AbsenceService(ApplicationDbContext context, IHttpContextAccessor accessor) : IAbsenceService
{
    public async Task<StaffAbsence> MarkAbsentAsync(MarkAbsenceRequest request)
    {
        var adminUserId = JwtUtils.GetUserId(accessor);
        if (string.IsNullOrEmpty(adminUserId))
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.Unauthorized,
                ErrorMessage = "Unauthorized",
                Code = "401"
            };

        var date = DateTime.SpecifyKind(DateTime.ParseExact(request.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture), DateTimeKind.Utc);

        var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == request.UserId && u.Role == UserRole.Staff);
        if (user == null)
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.BadRequest,
                ErrorMessage = "User not found or not a staff",
                Code = "400"
            };

        
        var exists = await context.Set<StaffAbsence>()
            .AnyAsync(a => a.UserId == request.UserId && a.Date.Date == date.Date);
        if (exists)
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.Conflict,
                ErrorMessage = "Staff already marked absent for this date",
                Code = "409"
            };

        var absence = new StaffAbsence
        {
            UserId = request.UserId,
            Date = date.Date,
            Reason = request.Reason,
            CreatedBy = adminUserId!,
            CreatedAt = DateTime.UtcNow
        };

        await context.AddAsync(absence);

        await context.SaveChangesAsync();
        return absence;
    }
}