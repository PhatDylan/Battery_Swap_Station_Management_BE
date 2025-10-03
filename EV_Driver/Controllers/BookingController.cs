using BusinessObject.DTOs.Booking;
using BusinessObject.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using System.Security.Claims;

namespace EV_Driver.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        #region Time Slots

        [HttpGet("time-slots")]
        [AllowAnonymous]
        public IActionResult GetAllTimeSlots()
        {
            var timeSlots = _bookingService.GetAllTimeSlots();
            return Ok(new
            {
                message = "Time slots retrieved successfully",
                data = timeSlots,
                operatingHours = "08:00 - 20:00",
                interval = "15 minutes"
            });
        }

        [HttpGet("available-slots/{stationId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableTimeSlots(string stationId, [FromQuery] DateTime date)
        {
            if (date.Date < DateTime.Today)
            {
                return BadRequest(new { message = "Cannot check availability for past dates" });
            }

            var availableSlots = await _bookingService.GetAvailableTimeSlotsAsync(stationId, date);

            if (availableSlots == null)
            {
                return NotFound(new { message = "Station not found or inactive" });
            }

            return Ok(new
            {
                message = "Available time slots retrieved successfully",
                data = availableSlots
            });
        }

        #endregion

        #region Booking CRUD

        [HttpPost]
        [Authorize(Roles = "1")] // Customer only
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            // Validate booking date
            if (request.BookingDate.Date < DateTime.Today)
            {
                return BadRequest(new { message = "Cannot book for past dates" });
            }

            if (request.BookingDate.Date > DateTime.Today.AddDays(30))
            {
                return BadRequest(new { message = "Cannot book more than 30 days in advance" });
            }

            var result = await _bookingService.CreateBookingAsync(userId, request);

            if (result == null)
            {
                return BadRequest(new { message = "Unable to create booking. Time slot may be unavailable or invalid data provided." });
            }

            return Ok(new
            {
                message = "Booking created successfully",
                data = result
            });
        }

        [HttpGet("{bookingId}")]
        public async Task<IActionResult> GetBookingById(string bookingId)
        {
            var booking = await _bookingService.GetBookingDetailAsync(bookingId);

            if (booking == null)
            {
                return NotFound(new { message = "Booking not found" });
            }

            // Check permission
            var userId = User.FindFirst("userId")?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (booking.UserId != userId && userRole != "2" && userRole != "3")
            {
                return Forbid();
            }

            return Ok(new
            {
                message = "Booking retrieved successfully",
                data = booking
            });
        }

        [HttpPut("{bookingId}/status")]
        [Authorize(Roles = "2,3")] // Staff and Admin only
        public async Task<IActionResult> UpdateBookingStatus(string bookingId, [FromBody] UpdateBookingStatusDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var staffId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(staffId))
            {
                return Unauthorized(new { message = "Invalid staff token" });
            }

            var result = await _bookingService.UpdateBookingStatusAsync(bookingId, staffId, request);

            if (result == null)
            {
                return NotFound(new { message = "Booking not found or unauthorized" });
            }

            return Ok(new
            {
                message = "Booking status updated successfully",
                data = result
            });
        }

        [HttpPut("{bookingId}/cancel")]
        [Authorize(Roles = "1")] // Customer only
        public async Task<IActionResult> CancelBooking(string bookingId)
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var result = await _bookingService.CancelBookingAsync(bookingId, userId);

            if (result == null)
            {
                return BadRequest(new { message = "Unable to cancel booking. Booking not found, already completed/cancelled, or too late to cancel." });
            }

            return Ok(new
            {
                message = "Booking cancelled successfully",
                data = result
            });
        }

        #endregion

        #region Booking Queries

        [HttpGet("my-bookings")]
        [Authorize(Roles = "1")] // Customer only
        public async Task<IActionResult> GetMyBookings([FromQuery] BookingFilterDto? filter = null)
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var bookings = await _bookingService.GetUserBookingsAsync(userId, filter);

            return Ok(new
            {
                message = "User bookings retrieved successfully",
                data = bookings
            });
        }

        [HttpGet("station/{stationId}")]
        [Authorize(Roles = "2,3")] // Staff and Admin only
        public async Task<IActionResult> GetStationBookings(string stationId, [FromQuery] BookingFilterDto? filter = null)
        {
            var bookings = await _bookingService.GetStationBookingsAsync(stationId, filter);

            return Ok(new
            {
                message = "Station bookings retrieved successfully",
                data = bookings
            });
        }

        [HttpGet("pending")]
        [Authorize(Roles = "2,3")] // Staff and Admin only
        public async Task<IActionResult> GetPendingBookings()
        {
            var bookings = await _bookingService.GetPendingBookingsForStaffAsync();

            return Ok(new
            {
                message = "Pending bookings retrieved successfully",
                data = bookings
            });
        }

        [HttpGet("status/{status}")]
        [Authorize(Roles = "2,3")] // Staff and Admin only
        public async Task<IActionResult> GetBookingsByStatus(BBRStatus status, [FromQuery] BookingFilterDto? filter = null)
        {
            var bookings = await _bookingService.GetBookingsByStatusAsync(status, filter);

            return Ok(new
            {
                message = $"Bookings with status {status} retrieved successfully",
                data = bookings
            });
        }

        [HttpGet("today")]
        [Authorize(Roles = "2,3")] // Staff and Admin only
        public async Task<IActionResult> GetTodayBookings([FromQuery] string? stationId = null)
        {
            var bookings = await _bookingService.GetTodayBookingsAsync(stationId);

            return Ok(new
            {
                message = "Today's bookings retrieved successfully",
                data = bookings
            });
        }

        #endregion

        #region Statistics

        [HttpGet("statistics")]
        [Authorize(Roles = "2,3")] // Staff and Admin only
        public async Task<IActionResult> GetBookingStatistics([FromQuery] string? stationId = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var statistics = await _bookingService.GetBookingStatisticsAsync(stationId, fromDate, toDate);

            return Ok(new
            {
                message = "Booking statistics retrieved successfully",
                data = statistics
            });
        }

        #endregion

        #region Validation

        [HttpGet("can-book")]
        [Authorize(Roles = "1")] // Customer only
        public async Task<IActionResult> CanUserBook([FromQuery] string vehicleId, [FromQuery] DateTime bookingDate, [FromQuery] string timeSlot)
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var canBook = await _bookingService.CanUserBookAsync(userId, vehicleId, bookingDate, timeSlot);

            return Ok(new
            {
                message = "Booking availability checked",
                data = new { canBook = canBook }
            });
        }

        [HttpGet("slot-available")]
        [AllowAnonymous]
        public async Task<IActionResult> IsTimeSlotAvailable([FromQuery] string stationId, [FromQuery] DateTime bookingDate, [FromQuery] string timeSlot)
        {
            var isAvailable = await _bookingService.IsTimeSlotAvailableAsync(stationId, bookingDate, timeSlot);

            return Ok(new
            {
                message = "Time slot availability checked",
                data = new { isAvailable = isAvailable }
            });
        }

        #endregion
    }
}
