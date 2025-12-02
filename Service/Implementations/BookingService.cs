using System.Net;
using BusinessObject;
using BusinessObject.DTOs;
using BusinessObject.Entities;
using BusinessObject.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Service.Exceptions;
using Service.Interfaces;
using Service.Utils;

namespace Service.Implementations;

public class BookingService(ApplicationDbContext context, IHttpContextAccessor accessor, IConfiguration configuration) : IBookingService
{
    public async Task CreateBooking(CreateBookingRequest request)
    {
        var userId = JwtUtils.GetUserId(accessor);
        if (string.IsNullOrEmpty(userId))
            throw new ValidationException
            {
                ErrorMessage = "Unauthorized",
                Code = "401",
                StatusCode = HttpStatusCode.Unauthorized
            };

        if (context.Bookings.Any(b => b.Status == BBRStatus.Pending && b.UserId == userId))
        {
            throw new ValidationException
            {
                ErrorMessage = "You need to wait for your last booking completed",
                Code = "409",
                StatusCode = HttpStatusCode.Conflict
            };
        }

        if (request.BookingDate is not null && request.BookingDate < DateTime.UtcNow)
        {
            throw new ValidationException
            {
                ErrorMessage = "Booking date must be in the future.",
                Code = "400",
                StatusCode = HttpStatusCode.BadRequest
            };
        }

        var vehicle = await context.Vehicles.Include(vehicle => vehicle.Batteries).FirstOrDefaultAsync(v =>
            v.UserId == userId && v.VehicleId == request.VehicleId) ?? throw new ValidationException
            {
                ErrorMessage = "You are not the owner of this vehicle.",
                Code = "400",
                StatusCode = HttpStatusCode.BadRequest
            };
        if (vehicle.Batteries.Count < request.SlotIds.Count)
            throw new ValidationException
            {
                ErrorMessage = "Battery capacity does not match.",
                Code = "400",
                StatusCode = HttpStatusCode.BadRequest
            };

        var stationActive = await context.Stations.AnyAsync(s => s.IsActive && s.StationId == request.StationId);
        if (!stationActive)
            throw new ValidationException
            {
                ErrorMessage = "Not found station.",
                Code = "400",
                StatusCode = HttpStatusCode.BadRequest
            };

        var validSlots = await context.StationBatterySlots
            .Include(s => s.Battery)
            .Where(b =>
                b.Battery != null &&
                b.Battery.BatteryTypeId == vehicle.BatteryTypeId &&
                b.StationId == request.StationId &&
                b.Status == SBSStatus.Available)
            .ToListAsync();

        if (validSlots.Count < request.SlotIds.Count)
        {
            throw new ValidationException
            {
                ErrorMessage = "Not enough batteries available.",
                Code = "400",
                StatusCode = HttpStatusCode.BadRequest
            };
        }
        
        var slots = validSlots.Where(s => request.SlotIds.Contains(s.StationSlotId)).ToList();

        await using var tx = await context.Database.BeginTransactionAsync();
        try
        {
            var booking = new Booking
            {
                BookingId = Guid.NewGuid().ToString(),
                UserId = userId,
                StationId = request.StationId,
                VehicleId = request.VehicleId,
                Status = BBRStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                BookingTime = request.BookingDate ?? DateTime.UtcNow.AddHours(3)
            };
            context.Bookings.Add(booking);

            var bookingSlots = new List<BatteryBookingSlot>();
            foreach (var slot in slots)
            {
                if (slot.Battery is null)
                    throw new ValidationException
                    {
                        ErrorMessage = "Station battery slot not found for a selected battery.",
                        Code = "400",
                        StatusCode = HttpStatusCode.BadRequest
                    };

                bookingSlots.Add(new BatteryBookingSlot
                {
                    Booking = booking,
                    BookingSlotId = Guid.NewGuid().ToString(),
                    BatteryId = slot.Battery.BatteryId,
                    StationSlotId = slot.StationSlotId,
                    Status = SBSStatus.Available,
                    CreatedAt = DateTime.UtcNow
                });

                slot.Status = SBSStatus.Full_slot;
            }

            if (bookingSlots.Count == 0)
                throw new ValidationException
                {
                    ErrorMessage = "No valid batteries to book.",
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };

            context.BatteryBookingSlots.AddRange(bookingSlots);
            context.StationBatterySlots.UpdateRange(validSlots);
            await context.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw new ValidationException
            {
                ErrorMessage = "Failed to create booking.",
                Code = "500",
                StatusCode = HttpStatusCode.InternalServerError
            };
        }
    }

    public async Task<BookingResponse> GetBookingAsync(string bookingId)
    {
        var b = await context.Bookings
            .Include(x => x.User)
            .Include(x => x.Station)
            .Include(x => x.Vehicle)
            .Include(x => x.BatteryBookingSlots)
                .ThenInclude(s => s.Battery)
                    .ThenInclude(bb => bb.BatteryType)
            .FirstOrDefaultAsync(x => x.BookingId == bookingId);

        if (b is null)
            throw new ValidationException
            {
                ErrorMessage = "Booking not found.",
                Code = "404",
                StatusCode = HttpStatusCode.NotFound
            };

        return ToResponse(b);
    }
    
    public async Task<PaginationWrapper<List<BookingResponse>, BookingResponse>> GetAllStationBookingAsync(string stationId, int page, int pageSize, string? search)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;

        var query = context.Bookings
            .Include(x => x.User)
            .Include(x => x.Station)
            .Include(x => x.Vehicle)
            .ThenInclude(v => v.Batteries)
            .Include(x => x.BatteryBookingSlots)
            .ThenInclude(s => s.Battery)
            .ThenInclude(bb => bb.BatteryType)
            .AsQueryable();
        
        query = query.Where(b => b.StationId == stationId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(b =>
                b.BookingId.Contains(term) ||
                b.User.FullName.Contains(term) ||
                b.User.Email.Contains(term) ||
                b.Station.Name.Contains(term) ||
                b.Vehicle.LicensePlate.Contains(term));
        }

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var responses = items.Select(ToResponse).ToList();

     
        return new PaginationWrapper<List<BookingResponse>, BookingResponse>(responses, page, totalItems, pageSize);
    }
    
    public async Task<PaginationWrapper<List<BookingResponse>, BookingResponse>> GetAllMyStationBookingAsync(int page, int pageSize, string? search)
    {
        var userId = JwtUtils.GetUserId(accessor);
        var stationId = context.StationStaffs.Where(s => s.UserId == userId).Select(s => s.StationId).FirstOrDefault();
        if (string.IsNullOrEmpty(stationId))
        {
            throw new ValidationException
            {
                ErrorMessage = "This user is not assigned to any station.",
                Code = "403",
                StatusCode = HttpStatusCode.Forbidden
            };
        }
        
        return await GetAllStationBookingAsync(stationId, page, pageSize, search);
    }
    
    public async Task<PaginationWrapper<List<BookingResponse>, BookingResponse>> GetAllMyBookingAsync(int page, int pageSize, string? search)
    {
        var userId = JwtUtils.GetUserId(accessor);
        if (string.IsNullOrEmpty(userId))
        {
            throw new ValidationException
            {
                ErrorMessage = "Unauthorized",
                Code = "401",
                StatusCode = HttpStatusCode.Unauthorized
            };
        }
        
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;

        var query = context.Bookings
            .Include(x => x.User)
            .Include(x => x.Station)
            .Include(x => x.Vehicle)
            .ThenInclude(v => v.Batteries)
            .Include(x => x.BatteryBookingSlots)
            .ThenInclude(s => s.Battery)
            .ThenInclude(bb => bb.BatteryType)
            .AsQueryable();
        
        query = query.Where(b => b.UserId == userId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(b =>
                b.BookingId.Contains(term) ||
                b.User.FullName.Contains(term) ||
                b.User.Email.Contains(term) ||
                b.Station.Name.Contains(term) ||
                b.Vehicle.LicensePlate.Contains(term));
        }

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var responses = items.Select(ToResponse).ToList();

     
        return new PaginationWrapper<List<BookingResponse>, BookingResponse>(responses, page, totalItems, pageSize);
    }

    public async Task<PaginationWrapper<List<BookingResponse>, BookingResponse>> GetAllBookingAsync(
        int page, int pageSize, string? search)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;

        var query = context.Bookings
            .Include(x => x.User)
            .Include(x => x.Station)
            .Include(x => x.Vehicle)
            .ThenInclude(v => v.Batteries)
            .Include(x => x.BatteryBookingSlots)
                .ThenInclude(s => s.Battery)
                    .ThenInclude(bb => bb.BatteryType)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(b =>
                b.BookingId.Contains(term) ||
                b.User.FullName.Contains(term) ||
                b.User.Email.Contains(term) ||
                b.Station.Name.Contains(term) ||
                b.Vehicle.LicensePlate.Contains(term));
        }

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var responses = items.Select(ToResponse).ToList();

     
        return new PaginationWrapper<List<BookingResponse>, BookingResponse>(responses, page, totalItems, pageSize);
    }
    public async Task<PaginationWrapper<List<BookingResponse>, BookingResponse>> GetAllStationBookingAsync(
    string stationId, int page, int pageSize, string? search, BBRStatus? status) // ✅ Thêm parameter
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;

        var query = context.Bookings
            .Include(x => x.User)
            .Include(x => x.Station)
            .Include(x => x.Vehicle)
                .ThenInclude(v => v.Batteries)
            .Include(x => x.BatteryBookingSlots)
                .ThenInclude(s => s.Battery)
                    .ThenInclude(bb => bb.BatteryType)
            .AsQueryable();

        query = query.Where(b => b.StationId == stationId);

        // Filter theo status nếu có
        if (status.HasValue)
        {
            query = query.Where(b => b.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(b =>
                b.BookingId.Contains(term) ||
                b.User.FullName.Contains(term) ||
                b.User.Email.Contains(term) ||
                b.Station.Name.Contains(term) ||
                b.Vehicle.LicensePlate.Contains(term));
        }

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var responses = items.Select(ToResponse).ToList();

        return new PaginationWrapper<List<BookingResponse>, BookingResponse>(responses, page, totalItems, pageSize);
    }

    public async Task<PaginationWrapper<List<BookingResponse>, BookingResponse>> GetAllMyStationBookingAsync(
        int page, int pageSize, string? search, BBRStatus? status) // ✅ Thêm parameter
    {
        var userId = JwtUtils.GetUserId(accessor);
        var stationId = context.StationStaffs.Where(s => s.UserId == userId).Select(s => s.StationId).FirstOrDefault();
        if (string.IsNullOrEmpty(stationId))
        {
            throw new ValidationException
            {
                ErrorMessage = "This user is not assigned to any station.",
                Code = "403",
                StatusCode = HttpStatusCode.Forbidden
            };
        }

        return await GetAllStationBookingAsync(stationId, page, pageSize, search, status); // ✅ Pass status
    }
    public async Task RejectSwapAsync(string bookingId, StaffRejectSwapRequest? request, CancellationToken ct = default)
    {
        //var staffUserId = JwtUtils.GetUserId(accessor);
        //if (string.IsNullOrEmpty(staffUserId))
        //    throw new ValidationException
        //    {
        //        StatusCode = HttpStatusCode.Unauthorized,
        //        Code = "401",
        //        ErrorMessage = "Unauthorized"
        //    };

        var booking = await context.Bookings
            .Include(b => b.BatteryBookingSlots)
            .FirstOrDefaultAsync(bs => bs.BookingId == bookingId, ct);
        if (booking == null)
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.NotFound,
                Code = "404",
                ErrorMessage = "Battery swap not found"
            };

        //var stationAssigned = await context.StationStaffs
        //    .AnyAsync(ss => ss.UserId == staffUserId && ss.StationId == booking.StationId, ct);
        //if (!stationAssigned)
        //    throw new ValidationException
        //    {
        //        StatusCode = HttpStatusCode.Forbidden,
        //        Code = "403",
        //        ErrorMessage = "You are not assigned to this station."
        //    };

        if (booking.Status != BBRStatus.Pending)
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.BadRequest,
                Code = "400",
                ErrorMessage = "Only pending swaps can be rejected"
            };

        await using var transaction = await context.Database.BeginTransactionAsync(ct);
        try
        {
            // Update booking status
            booking.Status = BBRStatus.Cancelled; // 3
            context.Bookings.Update(booking);

            // Reset station battery slots to Available
            var stationSlotIds = booking.BatteryBookingSlots.Select(bbs => bbs.StationSlotId).ToList();
            await context.StationBatterySlots
                .Where(sbs => stationSlotIds.Contains(sbs.StationSlotId))
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(s => s.Status, SBSStatus.Available)
                    .SetProperty(s => s.LastUpdated, DateTime.UtcNow), ct);

            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task ConfirmSwapAsync(string bookingId, CancellationToken ct = default)
    {
        //var staffUserId = JwtUtils.GetUserId(accessor);
        //if (string.IsNullOrEmpty(staffUserId))
        //    throw new ValidationException
        //    {
        //        StatusCode = HttpStatusCode.Unauthorized,
        //        Code = "401",
        //        ErrorMessage = "Unauthorized"
        //    };

        var booking = await context.Bookings.FirstOrDefaultAsync(bs => bs.BookingId == bookingId, ct);
        if (booking == null)
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.NotFound,
                Code = "404",
                ErrorMessage = "Battery swap not found"
            };

        //var stationAssigned = await context.StationStaffs
        //    .AnyAsync(ss => ss.UserId == staffUserId && ss.StationId == booking.StationId, ct);
        //if (!stationAssigned)
        //    throw new ValidationException
        //    {
        //        StatusCode = HttpStatusCode.Forbidden,
        //        Code = "403",
        //        ErrorMessage = "You are not assigned to this station."
        //    };

        if (booking.Status != BBRStatus.Pending)
            throw new ValidationException
            {
                StatusCode = HttpStatusCode.BadRequest,
                Code = "400",
                ErrorMessage = "Only pending swaps can be confirmed"
            };
        booking.Status = BBRStatus.Confirmed; // 2
        context.Bookings.Update(booking);
        await context.SaveChangesAsync(ct);
    }
    public async Task<EstimatePriceResponse> CalculateEstimatedPriceAsync(EstimatePriceRequest request)
    {
        // Validate vehicleId
        var vehicle = await context.Vehicles
            .Include(v => v.BatteryType)
            .FirstOrDefaultAsync(v => v.VehicleId == request.VehicleId);

        if (vehicle is null)
            throw new ValidationException
            {
                ErrorMessage = "Vehicle not found.",
                Code = "404",
                StatusCode = HttpStatusCode.NotFound
            };

        // Lấy battery hiện tại của vehicle (status = InUse)
        var battery = await context.Batteries
            .Include(b => b.BatteryType)
            .Where(b => b.VehicleId == request.VehicleId && b.Status == BatteryStatus.InUse)
            .OrderByDescending(b => b.UpdatedAt ?? b.CreatedAt)
            .FirstOrDefaultAsync();

        if (battery is null)
            throw new ValidationException
            {
                ErrorMessage = "No battery attached to this vehicle.",
                Code = "404",
                StatusCode = HttpStatusCode.NotFound
            };

        // Lấy thông tin station
        var station = await context.Stations
            .FirstOrDefaultAsync(s => s.StationId == request.StationId);

        if (station is null)
            throw new ValidationException
            {
                ErrorMessage = "Station not found.",
                Code = "404",
                StatusCode = HttpStatusCode.NotFound
            };

        // Lấy config values
        var batteryPercentageMin = float.Parse(configuration["Battery:BatteryPercentageMin"] ?? "0.2");
        var batterySurcharge = int.Parse(configuration["Battery:Surcharge"] ?? "30000");
        var electricPrice = station.ElectricityRate;

        // Tính estimated price
        var consume = battery.CapacityWh - battery.CurrentCapacityWh;
        var batteryPercentage = battery.CapacityWh > 0 ? (double)battery.CurrentCapacityWh / battery.CapacityWh : 0.0;
        var estimatedPrice = 0;
        var breakdown = string.Empty;

        if (batteryPercentage > batteryPercentageMin)
        {
            estimatedPrice = batterySurcharge;
            breakdown = $"Pin #{battery.SerialNo}: Phụ thu {batterySurcharge:N0} VND (còn {batteryPercentage * 100:F1}% > {batteryPercentageMin * 100:F0}%)";
        }
        else
        {
            estimatedPrice = consume * electricPrice / 1000;
            // Ensure estimatedPrice is not infinity or NaN
            if (double.IsInfinity(estimatedPrice) || double.IsNaN(estimatedPrice))
                estimatedPrice = 0;
            breakdown = $"Pin #{battery.SerialNo}: {consume}Wh × {electricPrice:N0} VND/kWh = {estimatedPrice:N0} VND";
        }

        return new EstimatePriceResponse
        {
            VehicleId = vehicle.VehicleId,
            BatteryId = battery.BatteryId,
            BatteryTypeId = battery.BatteryTypeId,
            BatteryTypeName = battery.BatteryType.BatteryTypeName,
            EstimatedPrice = estimatedPrice,
            PriceBreakdown = breakdown,
            BatteryCapacity = battery.CapacityWh,
            CurrentCapacity = battery.CurrentCapacityWh,
            BatteryPercentage = double.IsInfinity(batteryPercentage) || double.IsNaN(batteryPercentage) ? 0.0 : Math.Round(batteryPercentage * 100, 2),
            StationName = station.Name,
            ElectricityRate = electricPrice
        };
    }
    private (int totalPrice, string breakdown) CalculateEstimatedPrice(Booking b)
    {
        // Lấy config values
        var batteryPercentageMin = float.Parse(configuration["Battery:BatteryPercentageMin"] ?? "0.2");
        var batterySurcharge = int.Parse(configuration["Battery:Surcharge"] ?? "30000");
        var electricPrice = b.Station.ElectricityRate;

        var totalPrice = 0;
        var details = new List<string>();

        // Lấy batteries từ vehicle
        if (b.Vehicle?.Batteries == null || !b.Vehicle.Batteries.Any())
        {
            return (0, "No battery information available");
        }

        var vehicleBatteries = b.Vehicle.Batteries.ToList();

        foreach (var battery in vehicleBatteries)
        {
            var consume = battery.CapacityWh - battery.CurrentCapacityWh;
            var batteryPrice = 0;
            var currentBatteryPercentage = battery.CapacityWh > 0 ? (double)battery.CurrentCapacityWh / battery.CapacityWh : 0.0;

            if (currentBatteryPercentage > batteryPercentageMin)
            {
                // Pin còn nhiều điện -> tính phụ thu
                batteryPrice = batterySurcharge;
                details.Add($"Pin #{battery.SerialNo}: Phụ thu {batterySurcharge:N0} VND (còn >{batteryPercentageMin * 100:F0}%)");
            }
            else
            {
                // Pin yếu -> tính theo điện tiêu thụ
                batteryPrice = consume * electricPrice / 1000;
                // Ensure batteryPrice is not infinity or NaN
                if (double.IsInfinity(batteryPrice) || double.IsNaN(batteryPrice))
                    batteryPrice = 0;
                details.Add($"Pin #{battery.SerialNo}: {consume}Wh × {electricPrice:N0} = {batteryPrice:N0} VND");
            }

            totalPrice += batteryPrice;
        }

        return (totalPrice, string.Join(" | ", details));
    }
    private BookingResponse ToResponse(Booking b)
    {
       
        var firstSlot = b.BatteryBookingSlots.FirstOrDefault();
        var firstBattery = firstSlot?.Battery;
        var batteryType = firstBattery?.BatteryType;
        var (estimatedPrice, priceBreakdown) = CalculateEstimatedPrice(b);

        return new BookingResponse
        {
            // Base
            BookingId = b.BookingId,
            StationId = b.StationId,
            UserId = b.UserId,
            VehicleId = b.VehicleId,
            BatteryId = firstBattery?.BatteryId ?? string.Empty,
            BatteryTypeId = batteryType?.BatteryTypeId ?? string.Empty,
            TimeSlot = b.BookingTime.ToString("dd/MM/yyyy HH:mm"),
            Status = b.Status,
            CreatedAt = b.CreatedAt,

            // Thêm giá
            EstimatedPrice = estimatedPrice,
            PriceBreakdown = priceBreakdown,

            // User
            UserName = b.User.FullName,
            UserEmail = b.User.Email,
            UserPhone = b.User.Phone ?? string.Empty,

            // Station
            StationName = b.Station.Name,
            StationAddress = b.Station.Address,

            // Vehicle
            VehicleBrand = b.Vehicle.VBrand,
            VehicleModel = b.Vehicle.Model,
            LicensePlate = b.Vehicle.LicensePlate,

    
            BatteryTypeName = batteryType?.BatteryTypeName ?? string.Empty,


            ConfirmedByName = null,
            ConfirmedAt = null,
            CompletedAt = null,
            UpdatedAt = null,

            CanCancel = b.Status is BBRStatus.Pending or BBRStatus.Confirmed,
            CanModify = b.Status is BBRStatus.Pending
        };
    }
}