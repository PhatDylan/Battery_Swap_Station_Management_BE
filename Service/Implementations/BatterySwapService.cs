// Service/Implementations/BatterySwapService.cs

using System.Net;
using BusinessObject;
using BusinessObject.DTOs;
using BusinessObject.Entities;
using BusinessObject.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Service.Exceptions;
using Service.Interfaces;
using Service.Utils;

// THÊM để dùng Where method cho char

namespace Service.Implementations
{
    public class BatterySwapService(ApplicationDbContext context, IHttpContextAccessor accessor) : IBatterySwapService
    {
        public async Task<BatterySwapResponse> CreateBatterySwapFromBookingAsync(CreateBatterySwapRequest request)
        {
            var staffUserId = JwtUtils.GetUserId(accessor);
            if (string.IsNullOrEmpty(staffUserId))
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    ErrorMessage = "Unauthorized",
                    Code = "401"
                };

            // Validate staff role
            var staff = await context.Users.FindAsync(staffUserId);
            if (staff is not { Role: UserRole.Staff })
                throw new ValidationException
                {
                    ErrorMessage = "Only staff can create battery swaps",
                    Code = "403",
                    StatusCode = HttpStatusCode.Forbidden
                };

            // Get booking with all necessary includes
            var booking = await context.Bookings
                .Include(b => b.User)
                .Include(b => b.Vehicle)
                .Include(b => b.Station)
                .Include(b => b.BatteryBookingSlots)
                    .ThenInclude(bbs => bbs.Battery)
                        .ThenInclude(b => b.BatteryType)
                .FirstOrDefaultAsync(b => b.BookingId == request.BookingId) ?? throw new ValidationException
                {
                    ErrorMessage = "Booking not found",
                    Code = "404",
                    StatusCode = HttpStatusCode.NotFound
                };

            // Validate booking status
            if (booking.Status != BBRStatus.Confirmed)
                throw new ValidationException
                {
                    ErrorMessage = "Only confirmed bookings can be used for battery swap",
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };

            // Check if staff is assigned to this station
            var stationStaff = await context.StationStaffs
                .FirstOrDefaultAsync(ss => ss.UserId == staffUserId && ss.StationId == booking.StationId) ?? throw new ValidationException
                {
                    ErrorMessage = "You are not assigned to manage this station",
                    Code = "403",
                    StatusCode = HttpStatusCode.Forbidden
                };

            // Check if battery swap already exists for this booking
            var existingSwap = await context.BatterySwaps
                .FirstOrDefaultAsync(bs => bs.VehicleId == booking.VehicleId &&
                                          bs.UserId == booking.UserId &&
                                          bs.StationId == booking.StationId &&
                                          bs.CreatedAt.Date == DateTime.UtcNow.Date);

            if (existingSwap != null)
                throw new ValidationException
                {
                    ErrorMessage = "Battery swap already exists for this booking today",
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };

            // ===== VALIDATION MỚI: Validate new battery (ToBatteryId) =====
            var newBattery = await context.Batteries
                .Include(b => b.BatteryType)
                .Include(b => b.StationBatterySlot)
                .FirstOrDefaultAsync(b => b.BatteryId == request.ToBatteryId &&
                                          b.StationId == booking.StationId &&
                                          b.Status == BatteryStatus.Available);

            if (newBattery == null)
                throw new ValidationException
                {
                    ErrorMessage = "New battery not found or not available at this station",
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };

            // ===== THÊM VALIDATION: Check owner của pin mới phải là Station =====
            if (newBattery.Owner != BatteryOwner.Station)
                throw new ValidationException
                {
                    ErrorMessage = "Can only swap with station-owned batteries. The selected battery does not belong to the station.",
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };

            // Validate battery type compatibility
            var vehicleBatteryType = await context.Vehicles
                .Where(v => v.VehicleId == booking.VehicleId)
                .Select(v => v.BatteryTypeId)
                .FirstOrDefaultAsync();

            if (newBattery.BatteryTypeId != vehicleBatteryType)
                throw new ValidationException
                {
                    ErrorMessage = "Battery type is not compatible with the vehicle",
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };

            // ===== VALIDATION MỚI: Get old battery from vehicle (BatteryId) =====
            // Lấy pin hiện tại đang gắn trên xe của driver
            var oldBattery = await context.Batteries
                .Include(b => b.BatteryType)
                .FirstOrDefaultAsync(b => b.VehicleId == booking.VehicleId &&
                                          b.Status == BatteryStatus.InUse);

            // Fallback: Nếu không tìm thấy qua VehicleId, thử lấy từ booking slots
            if (oldBattery == null)
            {
                oldBattery = booking.BatteryBookingSlots.FirstOrDefault()?.Battery;
            }

            if (oldBattery == null)
                throw new ValidationException
                {
                    ErrorMessage = "No battery found attached to the vehicle or in booking slots",
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };

            // ===== THÊM VALIDATION: Check owner của pin cũ =====
            // Cho phép cả Driver và Station owned batteries để linh hoạt
            if (oldBattery.Owner != BatteryOwner.Driver && oldBattery.Owner != BatteryOwner.Station)
                throw new ValidationException
                {
                    ErrorMessage = "Old battery has invalid owner type",
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };

            // ===== THÊM VALIDATION: Không được swap cùng một pin =====
            if (oldBattery.BatteryId == newBattery.BatteryId)
                throw new ValidationException
                {
                    ErrorMessage = "Cannot swap battery with itself. Old battery and new battery must be different.",
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };

            // ===== THÊM VALIDATION: Check battery type compatibility giữa 2 pin =====
            if (oldBattery.BatteryTypeId != newBattery.BatteryTypeId)
                throw new ValidationException
                {
                    ErrorMessage = $"Battery type mismatch. Old battery type: {oldBattery.BatteryType?.BatteryTypeName}, New battery type: {newBattery.BatteryType?.BatteryTypeName}",
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };

            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // Create battery swap
                var batterySwap = new BatterySwap
                {
                    VehicleId = booking.VehicleId,
                    StationStaffId = stationStaff.StationStaffId,
                    UserId = booking.UserId,
                    StationId = booking.StationId,
                    BatteryId = oldBattery.BatteryId,      // Pin cũ của Driver (từ xe)
                    ToBatteryId = request.ToBatteryId,      // Pin mới từ Station
                    Status = BBRStatus.Pending,
                    SwappedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                context.BatterySwaps.Add(batterySwap);

                // ===== CẬP NHẬT STATUS CỦA CẢ 2 PIN =====

                // Pin cũ: Tháo ra khỏi xe, về station để sạc
                oldBattery.Status = BatteryStatus.Charging;
                oldBattery.VehicleId = null;  // Tháo khỏi xe
                oldBattery.StationId = booking.StationId;  // Đưa về station
                oldBattery.UpdatedAt = DateTime.UtcNow;

                // Pin mới: Từ station lên xe driver
                newBattery.Status = BatteryStatus.InUse;
                newBattery.VehicleId = booking.VehicleId;  // Gắn lên xe
                newBattery.StationId = null;  // Rời khỏi station (hoặc giữ nguyên để track)
                newBattery.UpdatedAt = DateTime.UtcNow;

                // Update station battery slot status nếu có
                if (newBattery.StationBatterySlot != null)
                {
                    newBattery.StationBatterySlot.Status = SBSStatus.Empty_slot;
                    newBattery.StationBatterySlot.LastUpdated = DateTime.UtcNow;
                    newBattery.StationBatterySlot.BatteryId = null;  // Slot trống
                }

                // Tìm slot trống để đặt pin cũ vào
                var emptySlot = await context.StationBatterySlots
                    .FirstOrDefaultAsync(s => s.StationId == booking.StationId &&
                                             s.Status == SBSStatus.Empty_slot);

                if (emptySlot != null)
                {
                    emptySlot.BatteryId = oldBattery.BatteryId;
                    emptySlot.Status = SBSStatus.Full_slot;
                    emptySlot.LastUpdated = DateTime.UtcNow;
                }

                context.Batteries.Update(oldBattery);
                context.Batteries.Update(newBattery);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Query lại với full includes
                var savedBatterySwap = await context.BatterySwaps
                    .Include(bs => bs.Vehicle)
                    .Include(bs => bs.User)
                    .Include(bs => bs.Station)
                    .Include(bs => bs.StationStaff)
                        .ThenInclude(ss => ss.User)
                    .Include(bs => bs.Battery)
                        .ThenInclude(b => b.BatteryType)
                    .Include(bs => bs.ToBattery)
                        .ThenInclude(b => b.BatteryType)
                    .Include(bs => bs.Payment)
                    .FirstOrDefaultAsync(bs => bs.SwapId == batterySwap.SwapId);

                return MapToBatterySwapResponse(savedBatterySwap!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new ValidationException
                {
                    ErrorMessage = $"Failed to create battery swap: {ex.Message}",
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }
        }

        public async Task<BatterySwapResponse> UpdateBatterySwapStatusAsync(string swapId, UpdateBatterySwapStatusRequest request)
        {
            var staffUserId = JwtUtils.GetUserId(accessor);
            if (string.IsNullOrEmpty(staffUserId))
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    ErrorMessage = "Unauthorized",
                    Code = "401"
                };

            var batterySwap = await context.BatterySwaps
                .Include(bs => bs.Vehicle)
                .Include(bs => bs.User)
                .Include(bs => bs.Station)
                .Include(bs => bs.StationStaff)
                    .ThenInclude(ss => ss.User)
                .Include(bs => bs.Battery)
                .Include(bs => bs.ToBattery)
                .Include(bs => bs.Payment)
                .FirstOrDefaultAsync(bs => bs.SwapId == swapId);

            if (batterySwap == null)
                throw new ValidationException
                {
                    ErrorMessage = "Battery swap not found",
                    Code = "404",
                    StatusCode = HttpStatusCode.NotFound
                };

            // Check if staff is assigned to this station
            var stationStaff = await context.StationStaffs
                .FirstOrDefaultAsync(ss => ss.UserId == staffUserId && ss.StationId == batterySwap.StationId);

            if (stationStaff == null)
                throw new ValidationException
                {
                    ErrorMessage = "You are not assigned to manage this station",
                    Code = "403",
                    StatusCode = HttpStatusCode.Forbidden
                };

            // Validate status transition
            if (!IsValidStatusTransition(batterySwap.Status, request.Status))
                throw new ValidationException
                {
                    ErrorMessage = $"Invalid status transition from {batterySwap.Status} to {request.Status}",
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };

            try
            {
                batterySwap.Status = request.Status;

                // If completing the swap, update related booking status
                if (request.Status == BBRStatus.Completed)
                {
                    var relatedBooking = await context.Bookings
                        .FirstOrDefaultAsync(b => b.VehicleId == batterySwap.VehicleId &&
                                                  b.UserId == batterySwap.UserId &&
                                                  b.StationId == batterySwap.StationId &&
                                                  b.Status == BBRStatus.Confirmed);

                    if (relatedBooking != null)
                    {
                        relatedBooking.Status = BBRStatus.Completed;
                    }
                }

                await context.SaveChangesAsync();
                return MapToBatterySwapResponse(batterySwap);
            }
            catch (Exception ex)
            {
                throw new ValidationException
                {
                    ErrorMessage = ex.Message,
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }
        }

        public async Task<BatterySwapDetailResponse> GetBatterySwapDetailAsync(string swapId)
        {
            var currentUserId = JwtUtils.GetUserId(accessor);
            if (string.IsNullOrEmpty(currentUserId))
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    ErrorMessage = "Unauthorized",
                    Code = "401"
                };

            var batterySwap = await context.BatterySwaps
                .Include(bs => bs.Vehicle)
                .Include(bs => bs.User)
                .Include(bs => bs.Station)
                .Include(bs => bs.StationStaff)
                    .ThenInclude(ss => ss.User)
                .Include(bs => bs.Battery)
                    .ThenInclude(b => b.BatteryType)
                .Include(bs => bs.ToBattery)
                    .ThenInclude(b => b.BatteryType)
                .Include(bs => bs.Payment)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(bs => bs.SwapId == swapId);

            if (batterySwap == null)
                throw new ValidationException
                {
                    ErrorMessage = "Battery swap not found",
                    Code = "404",
                    StatusCode = HttpStatusCode.NotFound
                };

            var currentUser = await context.Users.FindAsync(currentUserId);

            // Check permission
            var hasPermission = false;

            switch (currentUser?.Role)
            {
                case UserRole.Staff:
                    {
                        var isStaffOfStation = await context.StationStaffs
                            .AnyAsync(ss => ss.UserId == currentUserId && ss.StationId == batterySwap.StationId);
                        hasPermission = isStaffOfStation;
                        break;
                    }
                case UserRole.Admin:
                    hasPermission = batterySwap.Station.UserId == currentUserId;
                    break;
                case UserRole.Driver:
                    hasPermission = batterySwap.UserId == currentUserId;
                    break;
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!hasPermission)
                throw new ValidationException
                {
                    ErrorMessage = "You don't have permission to view this battery swap",
                    Code = "403",
                    StatusCode = HttpStatusCode.Forbidden
                };

            var response = MapToBatterySwapDetailResponse(batterySwap);

            var payment = batterySwap.Payment;
            response.Payment = new PaymentResponse
            {
                PayId = payment.PayId,
                UserId = payment.UserId,
                UserName = payment.User.FullName,
                OrderCode = payment.OrderCode,
                Amount = payment.Amount,
                Currency = payment.Currency,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status,
                CreatedAt = payment.CreatedAt
            };

            return response;
        }

        public async Task<PaginationWrapper<List<BatterySwapResponse>, BatterySwapResponse>> GetStationBatterySwapsAsync(string stationId, int page, int pageSize, string? search)
        {
            var staffUserId = JwtUtils.GetUserId(accessor);
            if (string.IsNullOrEmpty(staffUserId))
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    ErrorMessage = "Unauthorized",
                    Code = "401"
                };

            var currentUser = await context.Users.FindAsync(staffUserId);

            var hasPermission = currentUser?.Role switch
            {
                UserRole.Staff => await context.StationStaffs.AnyAsync(ss =>
                    ss.UserId == staffUserId && ss.StationId == stationId),
                UserRole.Admin => await context.Stations.AnyAsync(s =>
                    s.StationId == stationId && s.UserId == staffUserId),
                _ => false
            };

            if (!hasPermission)
                throw new ValidationException
                {
                    ErrorMessage = "You don't have permission to view battery swaps for this station",
                    Code = "403",
                    StatusCode = HttpStatusCode.Forbidden
                };

            var query = context.BatterySwaps
                .Include(bs => bs.Vehicle)
                .Include(bs => bs.User)
                .Include(bs => bs.Station)
                .Include(bs => bs.StationStaff)
                    .ThenInclude(ss => ss.User)
                .Include(bs => bs.Battery)
                .Include(bs => bs.ToBattery)
                .Include(bs => bs.Payment)
                .Where(bs => bs.StationId == stationId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(bs => bs.User.FullName.Contains(search) ||
                                         bs.Vehicle.LicensePlate.Contains(search));
            }

            var totalItems = await query.CountAsync();
            var batterySwaps = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderByDescending(bs => bs.CreatedAt)
                .ToListAsync();

            var responses = batterySwaps.Select(MapToBatterySwapResponse).ToList();

            return new PaginationWrapper<List<BatterySwapResponse>, BatterySwapResponse>(responses, totalItems, page, pageSize);
        }

        public async Task<PaginationWrapper<List<BatterySwapResponse>, BatterySwapResponse>> GetMyBatterySwapsAsync(int page, int pageSize, string? search)
        {
            var staffUserId = JwtUtils.GetUserId(accessor);
            if (string.IsNullOrEmpty(staffUserId))
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    ErrorMessage = "Unauthorized",
                    Code = "401"
                };

            var assignedStations = await context.StationStaffs
                .Where(ss => ss.UserId == staffUserId)
                .Select(ss => ss.StationId)
                .ToListAsync();

            if (assignedStations.Count == 0)
                return new PaginationWrapper<List<BatterySwapResponse>, BatterySwapResponse>(
                    [], 0, page, pageSize);

            var query = context.BatterySwaps
                .Include(bs => bs.Vehicle)
                .Include(bs => bs.User)
                .Include(bs => bs.Station)
                .Include(bs => bs.StationStaff)
                    .ThenInclude(ss => ss.User)
                .Include(bs => bs.Battery)
                .Include(bs => bs.ToBattery)
                .Include(bs => bs.Payment)
                .Where(bs => assignedStations.Contains(bs.StationId));

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(bs => bs.User.FullName.Contains(search) ||
                                         bs.Vehicle.LicensePlate.Contains(search) ||
                                         bs.Station.Name.Contains(search));
            }

            var totalItems = await query.CountAsync();
            var batterySwaps = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderByDescending(bs => bs.CreatedAt)
                .ToListAsync();

            var responses = batterySwaps.Select(MapToBatterySwapResponse).ToList();

            return new PaginationWrapper<List<BatterySwapResponse>, BatterySwapResponse>(responses, totalItems, page, pageSize);
        }

        public async Task<List<BatterySwapResponse>> GetSwapsByBookingAsync(string bookingId)
        {
            var currentUserId = JwtUtils.GetUserId(accessor);
            if (string.IsNullOrEmpty(currentUserId))
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    ErrorMessage = "Unauthorized",
                    Code = "401"
                };

            var booking = await context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
                throw new ValidationException
                {
                    ErrorMessage = "Booking not found",
                    Code = "404",
                    StatusCode = HttpStatusCode.NotFound
                };

            var swaps = await context.BatterySwaps
                .Include(bs => bs.Vehicle)
                .Include(bs => bs.User)
                .Include(bs => bs.Station)
                .Include(bs => bs.StationStaff)
                    .ThenInclude(ss => ss.User)
                .Include(bs => bs.Battery)
                .Include(bs => bs.ToBattery)
                .Include(bs => bs.Payment)
                .Where(bs => bs.VehicleId == booking.VehicleId &&
                             bs.UserId == booking.UserId &&
                             bs.StationId == booking.StationId &&
                             bs.CreatedAt.Date == DateTime.UtcNow.Date)
                .ToListAsync();

            return swaps.Select(MapToBatterySwapResponse).ToList();
        }

        public async Task<PaginationWrapper<List<BatterySwapResponse>, BatterySwapResponse>> GetDriverSwapHistoryAsync(int page, int pageSize, string? search)
        {
            // 1. Lấy userId từ JWT
            var driverUserId = JwtUtils.GetUserId(accessor);
            if (string.IsNullOrEmpty(driverUserId))
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    ErrorMessage = "Unauthorized",
                    Code = "401"
                };

            // 2. Kiểm tra role phải là Driver
            var user = await context.Users.FindAsync(driverUserId);
            if (user == null)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessage = "User not found",
                    Code = "404"
                };

            if (user.Role != UserRole.Driver)
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    ErrorMessage = "This endpoint is only for drivers",
                    Code = "403"
                };

            // 3. Tạo query lấy battery swaps của driver
            var query = context.BatterySwaps
                .Include(bs => bs.Vehicle)
                .Include(bs => bs.User)
                .Include(bs => bs.Station)
                .Include(bs => bs.StationStaff)
                    .ThenInclude(ss => ss.User)
                .Include(bs => bs.Battery)
                .Include(bs => bs.ToBattery)
                .Include(bs => bs.Payment)
                .Where(bs => bs.UserId == driverUserId);

            // 4. Tìm kiếm (nếu có)
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(bs =>
                    bs.Vehicle.LicensePlate.Contains(search) ||
                    bs.Station.Name.Contains(search) ||
                    bs.Station.Address.Contains(search)
                );
            }

            // 5. Đếm tổng số records
            var totalItems = await query.CountAsync();

            // 6. Phân trang và sắp xếp
            var batterySwaps = await query
                .OrderByDescending(bs => bs.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 7. Convert sang DTO
            var responses = batterySwaps.Select(MapToBatterySwapResponse).ToList();

            // 8. Trả về kết quả với pagination
            return new PaginationWrapper<List<BatterySwapResponse>, BatterySwapResponse>(
                responses, page, totalItems, pageSize);
        }

        #region Private Helper Methods

        private static bool IsValidStatusTransition(BBRStatus currentStatus, BBRStatus newStatus)
        {
            return currentStatus switch
            {
                BBRStatus.Pending => newStatus is BBRStatus.Confirmed or BBRStatus.Cancelled,
                BBRStatus.Confirmed => newStatus is BBRStatus.Completed or BBRStatus.Cancelled,
                _ => false
            };
        }

        private static BatterySwapResponse MapToBatterySwapResponse(BatterySwap batterySwap)
        {
            return new BatterySwapResponse
            {
                SwapId = batterySwap.SwapId,
                VehicleId = batterySwap.VehicleId,
                LicensePlate = batterySwap.Vehicle.LicensePlate,
                VehicleBrand = batterySwap.Vehicle.VBrand,
                VehicleModel = batterySwap.Vehicle.Model,
                StationStaffId = batterySwap.StationStaffId,
                UserId = batterySwap.UserId,
                UserName = batterySwap.User.FullName,
                UserPhone = batterySwap.User.Phone ?? "",
                StationId = batterySwap.StationId,
                StationName = batterySwap.Station.Name,
                BatteryId = batterySwap.BatteryId,
                BatterySerial = batterySwap.Battery.SerialNo,
                ToBatteryId = batterySwap.ToBatteryId,
                ToBatterySerial = batterySwap.ToBattery.SerialNo,
                Status = batterySwap.Status,
                SwappedAt = batterySwap.SwappedAt,
                CreatedAt = batterySwap.CreatedAt,
                HasPayment = true,
                PaymentId = batterySwap.PaymentId
            };
        }

        private static BatterySwapDetailResponse MapToBatterySwapDetailResponse(BatterySwap batterySwap)
        {
            var response = MapToBatterySwapResponse(batterySwap);
            return new BatterySwapDetailResponse
            {
                SwapId = response.SwapId,
                VehicleId = response.VehicleId,
                LicensePlate = response.LicensePlate,
                VehicleBrand = response.VehicleBrand,
                VehicleModel = response.VehicleModel,
                StationStaffId = response.StationStaffId,
                UserId = response.UserId,
                UserName = response.UserName,
                UserPhone = response.UserPhone,
                StationId = response.StationId,
                StationName = response.StationName,
                BatteryId = response.BatteryId,
                BatterySerial = response.BatterySerial,
                ToBatteryId = response.ToBatteryId,
                ToBatterySerial = response.ToBatterySerial,
                Status = response.Status,
                SwappedAt = response.SwappedAt,
                CreatedAt = response.CreatedAt,
                HasPayment = response.HasPayment,
                PaymentId = response.PaymentId
            };
        }
        #endregion
    }
}