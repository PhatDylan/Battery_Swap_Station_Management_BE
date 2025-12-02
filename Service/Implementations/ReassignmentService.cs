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

public class ReassignmentService(ApplicationDbContext context, IHttpContextAccessor accessor) : IReassignmentService
{
    public async Task<(StationStaffOverride Override, StaffAbsence Absence)> ReassignAsync(ReassignStaffRequest request)
    {
        if (request.AbsentUserId == request.ReplacementUserId)
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.BadRequest,
                ErrorMessage = "Replacement must be different from absent staff",
                Code = "400"
            };

        var adminUserId = JwtUtils.GetUserId(accessor);
        if (string.IsNullOrEmpty(adminUserId))
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.Unauthorized,
                ErrorMessage = "Unauthorized",
                Code = "401"
            };

        var date = DateTime.SpecifyKind(DateTime.ParseExact(request.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture), DateTimeKind.Utc).Date;

       
        var absentAssignment = await context.StationStaffs
            .Include(ss => ss.Station)
            .FirstOrDefaultAsync(ss => ss.UserId == request.AbsentUserId && ss.StationId == request.StationId);

        if (absentAssignment == null)
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.BadRequest,
                ErrorMessage = "Absent staff is not assigned to this station",
                Code = "400"
            };

        var absence = await context.Set<StaffAbsence>()
            .FirstOrDefaultAsync(a => a.UserId == request.AbsentUserId && a.Date == date);

        if (absence == null)
        {
            absence = new StaffAbsence
            {
                UserId = request.AbsentUserId,
                Date = date,
                Reason = request.Reason,
                CreatedBy = adminUserId!,
                CreatedAt = DateTime.UtcNow
            };
            await context.AddAsync(absence);
        }

        var replacement = await context.Users.FirstOrDefaultAsync(u => u.UserId == request.ReplacementUserId && u.Role == UserRole.Staff);
        if (replacement == null)
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.BadRequest,
                ErrorMessage = "Replacement user not found or not a staff",
                Code = "400"
            };

        var replacementAbsent = await context.Set<StaffAbsence>()
            .AnyAsync(a => a.UserId == request.ReplacementUserId && a.Date == date);
        if (replacementAbsent)
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.BadRequest,
                ErrorMessage = "Replacement staff is absent on this date",
                Code = "400"
            };

    
        var overrideExists = await context.Set<StationStaffOverride>()
            .AnyAsync(o =>
                o.UserId == request.ReplacementUserId &&
                o.Date == date &&
                (request.ShiftId == null || o.ShiftId == request.ShiftId));
        if (overrideExists)
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.Conflict,
                ErrorMessage = "Replacement staff already reassigned on this date",
                Code = "409"
            };

     
        var hasOtherStation = await context.StationStaffs
            .AnyAsync(ss => ss.UserId == request.ReplacementUserId && ss.StationId != request.StationId);
        if (hasOtherStation)
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.Conflict,
                ErrorMessage = "Replacement staff is assigned to another station",
                Code = "409"
            };

        var @override = new StationStaffOverride
        {
            UserId = request.ReplacementUserId,
            StationId = request.StationId,
            Date = date,
            ShiftId = request.ShiftId,
            Reason = request.Reason,
            CreatedBy = adminUserId!,
            CreatedAt = DateTime.UtcNow
        };

        await context.AddAsync(@override);

        await context.SaveChangesAsync();

        return (@override, absence);
    }
}