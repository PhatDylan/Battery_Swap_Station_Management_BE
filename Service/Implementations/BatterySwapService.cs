using BusinessObject;
using BusinessObject.DTOs.BatterySwap;
using BusinessObject.Entities;
using BusinessObject.Enums;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;

namespace Service.Implementations
{
    public class BatterySwapService : IBatterySwapService
    {
        private readonly ApplicationDbContext _context;

        public BatterySwapService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BatterySwapDetailDto?> CreateSwapFromBookingAsync(string bookingId, string staffId, CreateBatterySwapDto createSwapDto)
        {
            // Tìm booking confirmed
            var booking = await _context.Bookings
                .Include(b => b.Vehicle)
                .Include(b => b.Battery)
                .Include(b => b.User)
                .Include(b => b.Station)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId &&
                                        b.Status == BBRStatus.Confirmed);

            if (booking == null)
                return null;

            // Validate staff
            var staff = await _context.Users.FindAsync(staffId);
            if (staff == null || (staff.Role != UserRole.Staff && staff.Role != UserRole.Admin))
                return null;

            // Tìm station staff
            var stationStaff = await _context.StationStaffs
                .FirstOrDefaultAsync(ss => ss.UserId == staffId && ss.StationId == booking.StationId);

            if (stationStaff == null)
                return null;

            // Validate new battery
            var newBattery = await _context.Batteries
                .FirstOrDefaultAsync(b => b.BatteryId == createSwapDto.NewBatteryId &&
                                         b.StationId == booking.StationId &&
                                         b.Status == BatteryStatus.Available);

            if (newBattery == null)
                return null;

            // Tạo BatterySwap
            var batterySwap = new BatterySwap
            {
                VehicleId = booking.VehicleId,
                StationStaffId = stationStaff.StationStaffId,
                UserId = booking.UserId,
                StationId = booking.StationId,
                BatteryId = createSwapDto.NewBatteryId,
                Status = BBRStatus.Pending, // Bắt đầu với Pending
                SwappedAt = DateTime.UtcNow
            };

            _context.BatterySwaps.Add(batterySwap);

            // Update booking status
            booking.Status = BBRStatus.Confirmed; // Booking vẫn confirmed, swap sẽ có flow riêng

            // Update battery status
            newBattery.Status = BatteryStatus.InUse;

            await _context.SaveChangesAsync();

            return await GetBatterySwapDetailAsync(batterySwap.SwapId);
        }

        public async Task<BatterySwapDetailDto?> GetBatterySwapDetailAsync(string swapId)
        {
            var swap = await _context.BatterySwaps
                .Include(bs => bs.Vehicle)
                .ThenInclude(v => v.BatteryType)
                .Include(bs => bs.Battery)
                .ThenInclude(b => b.BatteryType)
                .Include(bs => bs.Station)
                .Include(bs => bs.User)
                .Include(bs => bs.StationStaff)
                .ThenInclude(ss => ss.User)
                .FirstOrDefaultAsync(bs => bs.SwapId == swapId);

            return swap != null ? MapToBatterySwapDetailDto(swap) : null;
        }

        public async Task<List<BatterySwapSummaryDto>> GetSwapsByBookingAsync(string bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Vehicle)
                .Include(b => b.Battery)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
                return new List<BatterySwapSummaryDto>();

            var swaps = await _context.BatterySwaps
                .Include(bs => bs.Vehicle)
                .Include(bs => bs.Battery)
                .Include(bs => bs.Station)
                .Include(bs => bs.StationStaff)
                .ThenInclude(ss => ss.User)
                .Where(bs => (bs.VehicleId == booking.VehicleId || bs.BatteryId == booking.BatteryId) &&
                            bs.UserId == booking.UserId &&
                            bs.StationId == booking.StationId)
                .ToListAsync();

            return swaps.Select(MapToBatterySwapSummaryDto).ToList();
        }

        private BatterySwapDetailDto MapToBatterySwapDetailDto(BatterySwap swap)
        {
            return new BatterySwapDetailDto
            {
                SwapId = swap.SwapId,
                VehicleId = swap.VehicleId,
                VehicleLicensePlate = swap.Vehicle.LicensePlate,
                VehicleBrand = swap.Vehicle.VBrand,
                VehicleModel = swap.Vehicle.Model,
                BatteryId = swap.BatteryId,
                BatterySerialNo = swap.Battery.SerialNo,
                BatteryTypeName = swap.Battery.BatteryType.BatteryTypeName,
                BatteryVoltage = swap.Battery.Voltage ?? "",
                BatteryCapacity = swap.Battery.CapacityWh ?? "",
                StationId = swap.StationId,
                StationName = swap.Station.Name,
                StationAddress = swap.Station.Address,
                StaffId = swap.StationStaff.StationStaffId,
                StaffName = swap.StationStaff.User.FullName,
                UserId = swap.UserId,
                UserName = swap.User.FullName,
                UserEmail = swap.User.Email,
                UserPhone = swap.User.Phone ?? "",
                Status = swap.Status,
                SwappedAt = swap.SwappedAt,
                CreatedAt = swap.CreatedAt,
            };
        }

        private BatterySwapSummaryDto MapToBatterySwapSummaryDto(BatterySwap swap)
        {
            return new BatterySwapSummaryDto
            {
                SwapId = swap.SwapId,
                VehicleId = swap.VehicleId,
                VehicleLicensePlate = swap.Vehicle.LicensePlate,
                BatteryId = swap.BatteryId,
                BatterySerialNo = swap.Battery.SerialNo,
                StationId = swap.StationId,
                StationName = swap.Station.Name,
                StaffId = swap.StationStaff.StationStaffId,
                StaffName = swap.StationStaff.User.FullName,
                Status = swap.Status,
                SwappedAt = swap.SwappedAt,
                CreatedAt = swap.CreatedAt
            };
        }
    }
}