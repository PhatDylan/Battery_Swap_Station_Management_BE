using System.Net;
using BusinessObject;
using BusinessObject.DTOs;
using BusinessObject.Entities;
using BusinessObject.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Net.payOS;
using Net.payOS.Types;
using Service.Exceptions;
using Service.Interfaces;
using Service.Utils;

namespace Service.Implementations
{
    public class PaymentService(IConfiguration configuration, ApplicationDbContext context, IHttpContextAccessor accessor) : IPaymentService
    {
        private readonly PayOS _payOs = new(configuration["PayOSSetting:ClientId"] ?? "",
            configuration["PayOSSetting:ApiKey"] ?? "",
            configuration["PayOSSetting:ChecksumKey"] ?? "");

        public async Task<CreatePaymentResponse> CreatePaymentAsync(CreatePaymentRequest request)
        {
            var booking = await context.Bookings.Include(b => b.Station)
                .Include(b => b.BatteryBookingSlots).ThenInclude(bbs => bbs.Battery)
                .Include(booking => booking.Vehicle)
                .ThenInclude(vehicle => vehicle.Batteries)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.BookingId == request.BookingId) ?? throw new ValidationException
                {
                    ErrorMessage = "Booking not found",
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400"
                };
            var isPaymentBefore = await context.Payments.FirstOrDefaultAsync(p => p.BookingId == request.BookingId
                && p.Status == PayStatus.Completed
                && p.PaymentMethod == request.PaymentMethod);
            if (isPaymentBefore != null)
            {
                return new CreatePaymentResponse
                {
                    PayId = isPaymentBefore.PayId,
                    OrderCode = isPaymentBefore.OrderCode,
                    Amount = isPaymentBefore.Amount,
                    PaymentMethod = isPaymentBefore.PaymentMethod,
                    Status = isPaymentBefore.Status,
                    PaymentUrl = isPaymentBefore.PaymentUrl,
                    CreatedAt = isPaymentBefore.CreatedAt,
                };
            }

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var station = booking.Station;
            var electricPrice = station.ElectricityRate;
            var totalPrice = 0;
            var payOsItemData = new List<ItemData>();
            var vehicleBatteries = booking.Vehicle.Batteries.ToList();

            if (vehicleBatteries.Count < booking.BatteryBookingSlots.Count)
            {
                throw new ValidationException
                {
                    ErrorMessage = "Not enough batteries in vehicle",
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400"
                };
            }

            if (request.PaymentMethod == PayMethod.Cash)
            {
                throw new ValidationException
                {
                    ErrorMessage = "Platform not support using cash",
                    Code = "400",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            if (request.PaymentMethod == PayMethod.Subscription_Plan)
            {
                var activeSubscription = await context.Subscriptions
                    .Include(s => s.SubscriptionPlan)
                    .FirstOrDefaultAsync(s =>
                        s.UserId == booking.UserId &&
                        s.Status == SubscriptionStatus.Active &&
                        s.EndDate > DateTime.UtcNow) ?? throw new ValidationException
                        {
                            ErrorMessage = "No active subscription plan found",
                            StatusCode = HttpStatusCode.BadRequest,
                            Code = "400"
                        };
                var plan = activeSubscription.SubscriptionPlan;

                var numberOfBatteries = booking.BatteryBookingSlots.Count;
                if (plan.SwapAmount - activeSubscription.NumberOfSwaps < numberOfBatteries)
                {
                    throw new ValidationException
                    {
                        ErrorMessage = $"Not enough swaps in subscription. Remaining: {activeSubscription.NumberOfSwaps}, Required: {numberOfBatteries}",
                        StatusCode = HttpStatusCode.BadRequest,
                        Code = "400"
                    };
                }

                activeSubscription.NumberOfSwaps += numberOfBatteries;
                var subscriptionPayment = new Payment
                {
                    PayId = Guid.NewGuid().ToString(),
                    UserId = booking.UserId,
                    OrderCode = timestamp.ToString(),
                    PaymentMethod = PayMethod.Subscription_Plan,
                    Currency = "VND",
                    Amount = 1,
                    BookingId = request.BookingId,
                    Status = PayStatus.Completed,
                    CreatedAt = DateTime.UtcNow
                };
                var vehicle = booking.Vehicle;
                var vehicleBattery = vehicle.Batteries.FirstOrDefault() ?? throw new ValidationException
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Code = "404",
                    ErrorMessage = "Vehicle not found"
                };
                await using var subscriptionTransaction = await context.Database.BeginTransactionAsync();

                try
                {
                    context.Payments.Add(subscriptionPayment);
                    context.Subscriptions.Update(activeSubscription);
                    await context.SaveChangesAsync();

                    var batterySwapList = (from batteryBookingSlot in booking.BatteryBookingSlots
                                           let battery = batteryBookingSlot.Battery
                                           select new BatterySwap
                                           {
                                               BatteryId = vehicleBattery.BatteryId,
                                               ToBatteryId = batteryBookingSlot.BatteryId,
                                               StationId = booking.StationId,
                                               UserId = booking.UserId,
                                               StationStaffId = null,
                                               VehicleId = vehicle.VehicleId,
                                               PaymentId = subscriptionPayment.PayId,
                                               Status = BBRStatus.Confirmed,
                                               CreatedAt = DateTime.UtcNow
                                           }).ToList();

                    context.BatterySwaps.AddRange(batterySwapList);

                    foreach (var batteryBookingSlot in booking.BatteryBookingSlots)
                    {
                        batteryBookingSlot.Battery.VehicleId = vehicle.VehicleId;
                        batteryBookingSlot.Battery.Status = BatteryStatus.InUse;
                    }

                    var newBatteryVehicle = booking.BatteryBookingSlots.Select(bbs => bbs.Battery).ToList();
                    context.Batteries.UpdateRange(newBatteryVehicle);

                    booking.Status = BBRStatus.Completed;
                    context.Bookings.Update(booking);

                    vehicleBattery.Status = BatteryStatus.QualityCheck;
                    vehicleBattery.StationId = booking.StationId;
                    vehicleBattery.VehicleId = null;
                    context.Batteries.Update(vehicleBattery);
                    await context.SaveChangesAsync();
                    await subscriptionTransaction.CommitAsync();
                    return new CreatePaymentResponse
                    {
                        PayId = subscriptionPayment.PayId,
                        OrderCode = subscriptionPayment.OrderCode,
                        Amount = subscriptionPayment.Amount,
                        PaymentMethod = subscriptionPayment.PaymentMethod,
                        Status = subscriptionPayment.Status,
                        PaymentUrl = subscriptionPayment.PaymentUrl,
                        CreatedAt = subscriptionPayment.CreatedAt,
                    };
                }
                catch (Exception ex)
                {
                    await subscriptionTransaction.RollbackAsync();
                    throw new ValidationException
                    {
                        ErrorMessage = ex.Message,
                        Code = "500",
                        StatusCode = HttpStatusCode.InternalServerError
                    };
                }
            }

            var batteryPercentageMin = float.Parse(configuration["Booking:BatteryPercentageMin"]!);
            var batterySurcharge = int.Parse(configuration["Booking:Surcharge"]!);
            for (var i = 0; i < booking.BatteryBookingSlots.Count; i++)
            {
                var battery = vehicleBatteries[i];
                var consume = battery.CapacityWh - battery.CurrentCapacityWh;
                var currentBatteryPercentage = battery.CapacityWh > 0 ? (double)battery.CurrentCapacityWh / battery.CapacityWh : 0.0;
                var itemPrice = 0;

                if (currentBatteryPercentage > batteryPercentageMin)
                {
                    totalPrice += batterySurcharge;
                    itemPrice = batterySurcharge;
                }
                else
                {
                    var calculatedPrice = consume * electricPrice / 1000;
                    if (double.IsInfinity(calculatedPrice) || double.IsNaN(calculatedPrice))
                        calculatedPrice = 0;
                    totalPrice += calculatedPrice;
                    itemPrice = calculatedPrice;
                }
                payOsItemData.Add(new ItemData(
                    $"Battery Id: {battery.BatteryId}, Capacity: {battery.CapacityWh} Wh", 1,
                    itemPrice
                ));
            }


            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var redirectUrl = configuration["Booking:RedirectUrl"]!;
                var paymentData = new PaymentData(timestamp, (int)totalPrice, "Payment for battery swap", payOsItemData,
                    $"{redirectUrl}", $"{redirectUrl}");
                var createPayment = await _payOs.createPaymentLink(paymentData);
                var payment = new Payment
                {
                    PayId = Guid.NewGuid().ToString(),
                    UserId = booking.UserId,
                    OrderCode = timestamp.ToString(),
                    PaymentMethod = request.PaymentMethod,
                    Currency = "VND",
                    Amount = totalPrice,
                    PaymentUrl = createPayment.checkoutUrl,
                    BookingId = request.BookingId,
                    Status = PayStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                context.Payments.Add(payment);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new CreatePaymentResponse
                {
                    PayId = payment.PayId,
                    OrderCode = payment.OrderCode,
                    Amount = payment.Amount,
                    PaymentMethod = payment.PaymentMethod,
                    Status = payment.Status,
                    PaymentUrl = createPayment.checkoutUrl,
                    CreatedAt = payment.CreatedAt,
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new ValidationException
                {
                    ErrorMessage = ex.Message,
                    Code = "500",
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
        }

        public async Task<PaymentResponse> GetPaymentDetailAsync(string paymentId)
        {
            var payment = await context.Payments
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PayId == paymentId);

            return payment == null ? throw new KeyNotFoundException("Payment not found") : MapToPaymentResponse(payment);
        }

        public async Task<PaymentResponse> GetPaymentDetailByDriverAsync(string userId)
        {
            var user = await context.Payments
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            return user == null ? throw new KeyNotFoundException("Payment by user not found") : MapToPaymentResponse(user);
        }

        public async Task<List<PaymentResponse>> GetMyPaymentsAsync()
        {
            var userId = JwtUtils.GetUserId(accessor);

            if (string.IsNullOrEmpty(userId))
            {
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    ErrorMessage = "Unauthorized",
                    Code = "401"
                };
            }

            var payments = await context.Payments
                .Include(p => p.User)
                .Include(p => p.Booking)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return payments.Select(MapToPaymentResponse).ToList();
        }

        public Task<List<PaymentResponse>> GetPaymentDetailByBookingIdAsync(string bookingId)
        {
            throw new NotImplementedException();
        }


        public bool ValidatePayOsSignature(WebhookType payloadJson)
        {
            try
            {
                _payOs.verifyPaymentWebhookData(payloadJson);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task ProcessPayOsPaymentAsync(WebhookData data, bool isSuccess)
        {
            var orderId = data.orderCode;
            if (data.description.Contains("Payment for battery swap"))
            {
                await HandlePayOsPaymentBatterySwapAsync(orderId, isSuccess);
            }
            else
            {
                await HandlePayOsSubscriptionPaymentAsync(orderId, isSuccess);
            }
        }

        private async Task HandlePayOsPaymentBatterySwapAsync(long orderId, bool isSuccess)
        {
            var paymentEntity = await context.Payments
                .Include(p => p.Booking)
                .ThenInclude(b => b.BatteryBookingSlots)
                .ThenInclude(bbs => bbs.Battery).Include(payment => payment.Booking)
                .ThenInclude(booking => booking.Vehicle).ThenInclude(vehicle => vehicle.Batteries)
                .FirstOrDefaultAsync(p => p.OrderCode == orderId.ToString()) ?? throw new ValidationException
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Code = "404",
                    ErrorMessage = "Payment not found"
                };
            await using var transaction = await context.Database.BeginTransactionAsync();
            paymentEntity.Status = isSuccess ? PayStatus.Completed : PayStatus.Cancelled;
            try
            {
                context.Payments.Update(paymentEntity);
                await context.SaveChangesAsync();
                var booking = paymentEntity.Booking;
                var vehicle = booking.Vehicle;
                var vehicleBattery = vehicle.Batteries.FirstOrDefault() ?? throw new ValidationException
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Code = "404",
                    ErrorMessage = "Vehicle not found"
                };
                var batteryBookingSlots = booking.BatteryBookingSlots;
                var batterySwapList = (from batteryBookingSlot in batteryBookingSlots
                                       let battery = batteryBookingSlot.Battery
                                       select new BatterySwap
                                       {
                                           BatteryId = vehicleBattery.BatteryId,
                                           ToBatteryId = batteryBookingSlot.BatteryId,
                                           StationId = booking.StationId,
                                           UserId = booking.UserId,
                                           StationStaffId = null,
                                           VehicleId = vehicle.VehicleId,
                                           PaymentId = paymentEntity.PayId,
                                           Status = BBRStatus.Confirmed,
                                           CreatedAt = DateTime.UtcNow
                                       }).ToList();

                context.BatterySwaps.AddRange(batterySwapList);
                await context.SaveChangesAsync();
                foreach (var batteryBookingSlot in batteryBookingSlots)
                {
                    batteryBookingSlot.Battery.Status = BatteryStatus.InUse;
                    batteryBookingSlot.Battery.VehicleId = vehicle.VehicleId;
                }
                var newBatteryVehicle = batteryBookingSlots.Select(bbs => bbs.Battery).ToList();
                context.Batteries.UpdateRange(newBatteryVehicle);
                vehicleBattery.Status = BatteryStatus.QualityCheck;
                vehicleBattery.StationId = booking.StationId;
                vehicleBattery.VehicleId = null;
                context.Batteries.Update(vehicleBattery);
                await context.SaveChangesAsync();
                booking.Status = BBRStatus.Completed;
                context.Bookings.Update(booking);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
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

        private async Task HandlePayOsSubscriptionPaymentAsync(long orderId, bool isSuccess)
        {
            var paymentEntity = await context.SubscriptionPayments
                .Include(sp => sp.Subscription)
                .ThenInclude(s => s.SubscriptionPlan)
                .FirstOrDefaultAsync(sp => sp.OrderCode == orderId.ToString()) ?? throw new ValidationException
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Code = "404",
                    ErrorMessage = "Subscription payment not found"
                };
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                paymentEntity.Status = isSuccess ? PayStatus.Completed : PayStatus.Cancelled;
                var subscription = paymentEntity.Subscription;
                if (isSuccess)
                {
                    subscription.Status = SubscriptionStatus.Active;
                    subscription.StartDate = DateTime.UtcNow;
                    subscription.EndDate = DateTime.UtcNow.AddMonths(1);
                }
                else
                {
                    subscription.Status = SubscriptionStatus.Cancelled;
                }
                context.Subscriptions.Update(subscription);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
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

        private PaymentResponse MapToPaymentResponse(Payment payment)
        {
            return new PaymentResponse
            {
                PayId = payment.PayId,
                BookingId = payment.BookingId,
                UserId = payment.UserId,
                UserName = payment.User.FullName,
                OrderCode = payment.OrderCode,
                Amount = payment.Amount,
                PaymentUrl = payment.PaymentUrl,
                Currency = payment.Currency,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status,
                CreatedAt = payment.CreatedAt,
            };
        }
    }
}
