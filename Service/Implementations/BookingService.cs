using BusinessObject;
using BusinessObject.DTOs.Booking;
using BusinessObject.DTOs.BatterySwap;
using BusinessObject.Entities;
using BusinessObject.Enums;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;

namespace Service.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeSpan StartTime = new TimeSpan(8, 0, 0);   // 8:00 AM
        private readonly TimeSpan EndTime = new TimeSpan(20, 0, 0);    // 8:00 PM
        private readonly int IntervalMinutes = 15;

        public BookingService(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Time Slots Management

        public List<string> GetAllTimeSlots()
        {
            var timeSlots = new List<string>();
            var currentTime = StartTime;

            while (currentTime < EndTime)
            {
                timeSlots.Add(currentTime.ToString(@"hh\:mm"));
                currentTime = currentTime.Add(TimeSpan.FromMinutes(IntervalMinutes));
            }

            return timeSlots;
        }

        // ✅ SỬA: Đổi tên method và return type
        public async Task<AvailableTimeSlotDto?> GetAvailableTimeSlotsAsync(string stationId, DateTime date)
        {
            var station = await _context.Stations.FindAsync(stationId);
            if (station == null || !station.IsActive)
                return null;

            var timeSlots = GetAllTimeSlots();
            var bookingsOnDate = await _context.Bookings
                .Include(b => b.User)
                .Where(b => b.StationId == stationId &&
                           b.BookingDate.Date == date.Date &&
                           (b.Status == BBRStatus.Pending ||
                            b.Status == BBRStatus.Confirmed ||
                            b.Status == BBRStatus.Completed))
                .ToListAsync();

            var availableSlots = timeSlots.Select(slot =>
            {
                var booking = bookingsOnDate.FirstOrDefault(b => b.TimeSlot.ToString(@"hh\:mm") == slot);
                return new TimeSlotDto
                {
                    TimeSlot = slot,
                    IsAvailable = booking == null || booking.Status != BBRStatus.Confirmed,
                    BookedByUserId = booking?.UserId,
                    BookedByUserName = booking?.User.FullName,
                    BookingStatus = booking?.Status,
                    BookingId = booking?.BookingId
                };
            }).ToList();

            // ✅ SỬA: Đổi class name
            return new AvailableTimeSlotDto
            {
                Date = date,
                StationId = stationId,
                StationName = station.Name,
                StationAddress = station.Address,
                TimeSlots = availableSlots,
                OperatingHours = "08:00 - 20:00"
            };
        }

        #endregion

        #region Booking CRUD Operations

        // ✅ SỬA: Đổi parameter type
        public async Task<BookingResponseDto?> CreateBookingAsync(string userId, CreateBookingDto request)
        {
            // Validate user 
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.Role != UserRole.Driver || user.Status != UserStatus.Active) // 1 = Customer, 1 = Active
                return null;

            // Validate vehicle belongs to user
            var vehicle = await _context.Vehicles
                .Include(v => v.BatteryType)
                .FirstOrDefaultAsync(v => v.VehicleId == request.VehicleId && v.UserId == userId);
            if (vehicle == null)
                return null;

            // Validate station
            var station = await _context.Stations.FindAsync(request.StationId);
            if (station == null || !station.IsActive)
                return null;

            // Validate battery type
            var batteryType = await _context.BatteryTypes.FindAsync(request.BatteryTypeId);
            if (batteryType == null)
                return null;

            // Parse and validate time slot
            if (!TimeSpan.TryParse(request.TimeSlot, out TimeSpan timeSlot))
                return null;

            if (timeSlot < StartTime || timeSlot >= EndTime)
                return null;

            // Check availability
            if (!await IsTimeSlotAvailableAsync(request.StationId, request.BookingDate, request.TimeSlot))
                return null;

            if (!await CanUserBookAsync(userId, request.VehicleId, request.BookingDate, request.TimeSlot))
                return null;

            // Get available battery for booking 
            var availableBattery = await _context.Batteries
                .FirstOrDefaultAsync(b => b.StationId == request.StationId &&
                                         b.BatteryTypeId == request.BatteryTypeId &&
                                         b.Status == BatteryStatus.Available);
            if (availableBattery == null)
                return null;

            // Create booking
            var booking = new Booking
            {
                UserId = userId,
                VehicleId = request.VehicleId,
                StationId = request.StationId,
                BatteryId = availableBattery.BatteryId,
                BatteryTypeId = request.BatteryTypeId,
                BookingDate = request.BookingDate.Date,
                TimeSlot = timeSlot,
                Status = BBRStatus.Pending,
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return await GetBookingResponseAsync(booking.BookingId);
        }

        public async Task<BookingDetailDto?> GetBookingDetailAsync(string bookingId)
        {
            var booking = await GetBookingResponseAsync(bookingId);
            if (booking == null)
                return null;

            var relatedSwaps = await GetRelatedBatterySwapsAsync(bookingId);

            return new BookingDetailDto
            {
                BookingId = booking.BookingId,
                StationId = booking.StationId,
                UserId = booking.UserId,
                VehicleId = booking.VehicleId,
                BatteryId = booking.BatteryId,
                BatteryTypeId = booking.BatteryTypeId,
                BookingDate = booking.BookingDate,
                TimeSlot = booking.TimeSlot,
                Status = booking.Status,
                CreatedAt = booking.CreatedAt,
                UserName = booking.UserName,
                UserEmail = booking.UserEmail,
                UserPhone = booking.UserPhone,
                StationName = booking.StationName,
                StationAddress = booking.StationAddress,
                VehicleBrand = booking.VehicleBrand,
                VehicleModel = booking.VehicleModel,
                LicensePlate = booking.LicensePlate,
                BatteryTypeName = booking.BatteryTypeName,
                ConfirmedByName = booking.ConfirmedByName,
                ConfirmedAt = booking.ConfirmedAt,
                CompletedAt = booking.CompletedAt,
                UpdatedAt = booking.UpdatedAt,
                CanCancel = booking.CanCancel,
                CanModify = booking.CanModify,
                RelatedSwaps = relatedSwaps
            };
        }

        // ✅ SỬA: Đổi parameter type
        public async Task<BookingResponseDto?> UpdateBookingStatusAsync(string bookingId, string staffId, UpdateBookingStatusDto request)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
                return null;

            // Validate staff
            var staff = await _context.Users.FindAsync(staffId);
            if (staff == null || (staff.Role != UserRole.Staff && staff.Role != UserRole.Admin)) // 2 = Staff, 3 = Admin
                return null;

            var oldStatus = booking.Status;
            booking.Status = request.Status;
            booking.UpdatedAt = DateTime.UtcNow;

            if (request.Status == BBRStatus.Confirmed && oldStatus == BBRStatus.Pending)
            {
                booking.ConfirmBy = staffId;
            }
            else if (request.Status == BBRStatus.Completed)
            {
                booking.CompleteAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return await GetBookingResponseAsync(bookingId);
        }

        public async Task<BookingResponseDto?> CancelBookingAsync(string bookingId, string userId)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == userId);

            if (booking == null || booking.Status == BBRStatus.Completed || booking.Status == BBRStatus.Cancelled)
                return null;

            // Check if can cancel (at least 30 minutes before)
            var bookingDateTime = booking.BookingDate.Date.Add(booking.TimeSlot);
            if (DateTime.Now.AddMinutes(30) > bookingDateTime)
                return null;

            booking.Status = BBRStatus.Cancelled;
            booking.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetBookingResponseAsync(bookingId);
        }

        #endregion

        #region Booking Queries

        // ✅ SỬA: Đổi parameter type
        public async Task<List<BookingSummaryDto>> GetUserBookingsAsync(string userId, BookingFilterDto? filter = null)
        {
            var query = _context.Bookings
                .Include(b => b.Station)
                .Include(b => b.Vehicle)
                .Where(b => b.UserId == userId);

            query = ApplyFilters(query, filter);

            var bookings = await query
                .OrderByDescending(b => b.BookingDate)
                .ThenByDescending(b => b.TimeSlot)
                .ToListAsync();

            return bookings.Select(b => new BookingSummaryDto
            {
                BookingId = b.BookingId,
                BookingDate = b.BookingDate,
                TimeSlot = b.TimeSlot.ToString(@"hh\:mm"),
                Status = b.Status,
                StationName = b.Station.Name,
                LicensePlate = b.Vehicle.LicensePlate,
                CreatedAt = b.CreatedAt
            }).ToList();
        }

        public async Task<List<BookingResponseDto>> GetStationBookingsAsync(string stationId, BookingFilterDto? filter = null)
        {
            var query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Vehicle)
                .Include(b => b.Station)
                .Include(b => b.BatteryType)
                .Where(b => b.StationId == stationId);

            query = ApplyFilters(query, filter);

            var bookings = await query
                .OrderBy(b => b.BookingDate)
                .ThenBy(b => b.TimeSlot)
                .ToListAsync();

            return await MapToBookingResponseDtoListAsync(bookings);
        }

        public async Task<List<BookingResponseDto>> GetPendingBookingsForStaffAsync()
        {
            var bookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Vehicle)
                .Include(b => b.Station)
                .Include(b => b.BatteryType)
                .Where(b => b.Status == BBRStatus.Pending)
                .OrderBy(b => b.BookingDate)
                .ThenBy(b => b.TimeSlot)
                .ToListAsync();

            return await MapToBookingResponseDtoListAsync(bookings);
        }

        // ✅ SỬA: Đổi parameter type và bỏ Include sai
        public async Task<List<BookingResponseDto>> GetBookingsByStatusAsync(BBRStatus status, BookingFilterDto? filter = null)
        {
            var query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Vehicle)
                .Include(b => b.Station)
                .Include(b => b.BatteryType)
                .Where(b => b.Status == status);

            query = ApplyFilters(query, filter);

            var bookings = await query
                .OrderBy(b => b.BookingDate)
                .ThenBy(b => b.TimeSlot)
                .ToListAsync();

            return await MapToBookingResponseDtoListAsync(bookings);
        }

        #endregion

        #region Statistics

        public async Task<BookingStatisticsDto> GetBookingStatisticsAsync(string? stationId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Bookings.AsQueryable();

            if (!string.IsNullOrEmpty(stationId))
                query = query.Where(b => b.StationId == stationId);

            if (fromDate.HasValue)
                query = query.Where(b => b.BookingDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(b => b.BookingDate <= toDate.Value);

            var bookings = await query.ToListAsync();

            var totalBookings = bookings.Count;
            var pendingBookings = bookings.Count(b => b.Status == BBRStatus.Pending);
            var confirmedBookings = bookings.Count(b => b.Status == BBRStatus.Confirmed);
            var completedBookings = bookings.Count(b => b.Status == BBRStatus.Completed);
            var cancelledBookings = bookings.Count(b => b.Status == BBRStatus.Cancelled);

            var completionRate = totalBookings > 0 ? (double)completedBookings / totalBookings * 100 : 0;
            var cancellationRate = totalBookings > 0 ? (double)cancelledBookings / totalBookings * 100 : 0;

            var dailyStats = bookings
                .GroupBy(b => b.BookingDate.Date)
                .Select(g => new DailyBookingCountDto
                {
                    Date = g.Key,
                    Count = g.Count(),
                    CompletedCount = g.Count(b => b.Status == BBRStatus.Completed),
                    CancelledCount = g.Count(b => b.Status == BBRStatus.Cancelled)
                })
                .OrderBy(d => d.Date)
                .ToList();

            return new BookingStatisticsDto
            {
                TotalBookings = totalBookings,
                PendingBookings = pendingBookings,
                ConfirmedBookings = confirmedBookings,
                CompletedBookings = completedBookings,
                CancelledBookings = cancelledBookings,
                CompletionRate = Math.Round(completionRate, 2),
                CancellationRate = Math.Round(cancellationRate, 2),
                DailyStats = dailyStats
            };
        }

        public async Task<List<BookingResponseDto>> GetTodayBookingsAsync(string? stationId = null)
        {
            var today = DateTime.Today;
            var query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Vehicle)
                .Include(b => b.Station)
                .Include(b => b.BatteryType)
                .Where(b => b.BookingDate.Date == today);

            if (!string.IsNullOrEmpty(stationId))
                query = query.Where(b => b.StationId == stationId);

            var bookings = await query
                .OrderBy(b => b.TimeSlot)
                .ToListAsync();

            return await MapToBookingResponseDtoListAsync(bookings);
        }

        #endregion

        #region Validation

        public async Task<bool> CanUserBookAsync(string userId, string vehicleId, DateTime bookingDate, string timeSlot)
        {
            if (!TimeSpan.TryParse(timeSlot, out TimeSpan timeSlotSpan))
                return false;

            // Check if user already has a booking at the same time
            var existingBooking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.UserId == userId &&
                                         b.BookingDate.Date == bookingDate.Date &&
                                         b.TimeSlot == timeSlotSpan &&
                                         (b.Status == BBRStatus.Pending ||
                                          b.Status == BBRStatus.Confirmed ||
                                          b.Status == BBRStatus.Completed));

            return existingBooking == null;
        }

        public async Task<bool> IsTimeSlotAvailableAsync(string stationId, DateTime bookingDate, string timeSlot)
        {
            if (!TimeSpan.TryParse(timeSlot, out TimeSpan timeSlotSpan))
                return false;

            // Check if time slot is already confirmed
            var existingBooking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.StationId == stationId &&
                                         b.BookingDate.Date == bookingDate.Date &&
                                         b.TimeSlot == timeSlotSpan &&
                                         b.Status == BBRStatus.Confirmed);

            return existingBooking == null;
        }

        #endregion

        #region Related Data

        public async Task<List<BatterySwapSummaryDto>> GetRelatedBatterySwapsAsync(string bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Vehicle)
                .Include(b => b.Battery)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
                return new List<BatterySwapSummaryDto>();

            // Find battery swaps through Vehicle or Battery 
            var swaps = await _context.BatterySwaps
                .Include(bs => bs.Vehicle)
                .Include(bs => bs.Battery)
                .Include(bs => bs.Station)
                .Include(bs => bs.StationStaff)
                .ThenInclude(ss => ss.User)
                .Where(bs => (bs.VehicleId == booking.VehicleId || bs.BatteryId == booking.BatteryId) &&
                            bs.UserId == booking.UserId &&
                            bs.StationId == booking.StationId &&
                            bs.SwappedAt.Date == booking.BookingDate.Date)
                .ToListAsync();

            return swaps.Select(bs => new BatterySwapSummaryDto
            {
                SwapId = bs.SwapId,
                VehicleId = bs.VehicleId,
                VehicleLicensePlate = bs.Vehicle.LicensePlate,
                BatteryId = bs.BatteryId,
                BatterySerialNo = bs.Battery.SerialNo,
                StationId = bs.StationId,
                StationName = bs.Station.Name,
                StaffId = bs.StationStaff.StationStaffId,
                StaffName = bs.StationStaff.User.FullName,
                Status = bs.Status,
                SwappedAt = bs.SwappedAt,
                CreatedAt = bs.CreatedAt
            }).ToList();
        }

        #endregion

        #region Private Helper Methods

        private IQueryable<Booking> ApplyFilters(IQueryable<Booking> query, BookingFilterDto? filter)
        {
            if (filter == null)
                return query;

            if (!string.IsNullOrEmpty(filter.StationId))
                query = query.Where(b => b.StationId == filter.StationId);

            if (!string.IsNullOrEmpty(filter.UserId))
                query = query.Where(b => b.UserId == filter.UserId);

            if (filter.BookingDate.HasValue)
                query = query.Where(b => b.BookingDate.Date == filter.BookingDate.Value.Date);

            if (filter.Status.HasValue)
                query = query.Where(b => b.Status == filter.Status.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(b => b.BookingDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(b => b.BookingDate <= filter.ToDate.Value);

            return query;
        }

        private async Task<BookingResponseDto?> GetBookingResponseAsync(string bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Vehicle)
                .Include(b => b.Station)
                .Include(b => b.BatteryType)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            return booking != null ? await MapToBookingResponseDtoAsync(booking) : null;
        }

        // Method để map list với optimization
        private async Task<List<BookingResponseDto>> MapToBookingResponseDtoListAsync(List<Booking> bookings)
        {
            // Get all staff IDs in one query để tối ưu performance
            var staffIds = bookings
                .Where(b => !string.IsNullOrEmpty(b.ConfirmBy))
                .Select(b => b.ConfirmBy!)
                .Distinct()
                .ToList();

            var staffDict = new Dictionary<string, string>();
            if (staffIds.Any())
            {
                var staffs = await _context.Users
                    .Where(u => staffIds.Contains(u.UserId))
                    .Select(u => new { u.UserId, u.FullName })
                    .ToListAsync();

                staffDict = staffs.ToDictionary(s => s.UserId, s => s.FullName);
            }

            return bookings.Select(booking => MapToBookingResponseDto(booking, staffDict)).ToList();
        }

        // Method để map single booking với async
        private async Task<BookingResponseDto> MapToBookingResponseDtoAsync(Booking booking)
        {
            string? confirmedByName = null;
            if (!string.IsNullOrEmpty(booking.ConfirmBy))
            {
                var staff = await _context.Users.FindAsync(booking.ConfirmBy);
                confirmedByName = staff?.FullName;
            }

            return MapToBookingResponseDto(booking, confirmedByName);
        }

        // Method chính để map booking
        private BookingResponseDto MapToBookingResponseDto(Booking booking, Dictionary<string, string>? staffDict = null)
        {
            var bookingDateTime = booking.BookingDate.Date.Add(booking.TimeSlot);
            var canCancel = (booking.Status == BBRStatus.Pending || booking.Status == BBRStatus.Confirmed) &&
                           DateTime.Now.AddMinutes(30) <= bookingDateTime;
            var canModify = booking.Status == BBRStatus.Pending &&
                           DateTime.Now.AddHours(2) <= bookingDateTime;

            string? confirmedByName = null;
            if (!string.IsNullOrEmpty(booking.ConfirmBy))
            {
                if (staffDict != null && staffDict.ContainsKey(booking.ConfirmBy))
                {
                    confirmedByName = staffDict[booking.ConfirmBy];
                }
            }

            return new BookingResponseDto
            {
                BookingId = booking.BookingId,
                StationId = booking.StationId,
                UserId = booking.UserId,
                VehicleId = booking.VehicleId,
                BatteryId = booking.BatteryId,
                BatteryTypeId = booking.BatteryTypeId,
                BookingDate = booking.BookingDate,
                TimeSlot = booking.TimeSlot.ToString(@"hh\:mm"),
                Status = booking.Status,
                CreatedAt = booking.CreatedAt,
                UserName = booking.User.FullName,
                UserEmail = booking.User.Email,
                UserPhone = booking.User.Phone ?? "",
                StationName = booking.Station.Name,
                StationAddress = booking.Station.Address,
                VehicleBrand = booking.Vehicle.VBrand,
                VehicleModel = booking.Vehicle.Model,
                LicensePlate = booking.Vehicle.LicensePlate,
                BatteryTypeName = booking.BatteryType.BatteryTypeName,
                ConfirmedByName = confirmedByName,
                ConfirmedAt = booking.Status == BBRStatus.Confirmed ? booking.UpdatedAt : null,
                CompletedAt = booking.CompleteAt,
                UpdatedAt = booking.UpdatedAt,
                CanCancel = canCancel,
                CanModify = canModify
            };
        }

        // Overload cho single booking
        private BookingResponseDto MapToBookingResponseDto(Booking booking, string? confirmedByName)
        {
            var bookingDateTime = booking.BookingDate.Date.Add(booking.TimeSlot);
            var canCancel = (booking.Status == BBRStatus.Pending || booking.Status == BBRStatus.Confirmed) &&
                           DateTime.Now.AddMinutes(30) <= bookingDateTime;
            var canModify = booking.Status == BBRStatus.Pending &&
                           DateTime.Now.AddHours(2) <= bookingDateTime;

            return new BookingResponseDto
            {
                BookingId = booking.BookingId,
                StationId = booking.StationId,
                UserId = booking.UserId,
                VehicleId = booking.VehicleId,
                BatteryId = booking.BatteryId,
                BatteryTypeId = booking.BatteryTypeId,
                BookingDate = booking.BookingDate,
                TimeSlot = booking.TimeSlot.ToString(@"hh\:mm"),
                Status = booking.Status,
                CreatedAt = booking.CreatedAt,
                UserName = booking.User.FullName,
                UserEmail = booking.User.Email,
                UserPhone = booking.User.Phone ?? "",
                StationName = booking.Station.Name,
                StationAddress = booking.Station.Address,
                VehicleBrand = booking.Vehicle.VBrand,
                VehicleModel = booking.Vehicle.Model,
                LicensePlate = booking.Vehicle.LicensePlate,
                BatteryTypeName = booking.BatteryType.BatteryTypeName,
                ConfirmedByName = confirmedByName,
                ConfirmedAt = booking.Status == BBRStatus.Confirmed ? booking.UpdatedAt : null,
                CompletedAt = booking.CompleteAt,
                UpdatedAt = booking.UpdatedAt,
                CanCancel = canCancel,
                CanModify = canModify
            };
        }

        #endregion
    }
}