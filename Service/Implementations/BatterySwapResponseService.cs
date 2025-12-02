using BusinessObject;
using BusinessObject.DTOs;
using BusinessObject.DTOs.BatterySwap;
using BusinessObject.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implementations
{
    public class BatterySwapResponseService(ApplicationDbContext context, IHttpContextAccessor accessor) : IBatterySwapResponseService
    {
        public async Task<PaginationWrapper<List<CompletedBatterySwapResponseDto>, CompletedBatterySwapResponseDto>> GetCompletedSwapsByStationStaffIdAsync(string stationStaffId, int page, int pageSize)
        {
            // Query cơ bản
            var query = context.BatterySwaps
                .Include(bs => bs.Battery)
                    .ThenInclude(b => b.BatteryType)
                .Include(bs => bs.ToBattery)
                    .ThenInclude(b => b.BatteryType)
                .Where(bs => bs.StationStaffId == stationStaffId && bs.Status == BBRStatus.Completed) // Status = 4
                .OrderByDescending(bs => bs.SwappedAt);

            // Đếm tổng số records
            var totalCount = await query.CountAsync();

            // Áp dụng phân trang
            var completedSwaps = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map sang DTO
            var result = completedSwaps.Select(swap => new CompletedBatterySwapResponseDto
            {
                SwapId = swap.SwapId,
                VehicleId = swap.VehicleId,
                StationStaffId = swap.StationStaffId,
                UserId = swap.UserId,
                StationId = swap.StationId,
                BatteryId = swap.BatteryId,
                ToBatteryId = swap.ToBatteryId,
                Reason = swap.Reason,
                PaymentId = swap.PaymentId,
                Status = (int)swap.Status,
                SwappedAt = swap.SwappedAt,
                CreatedAt = swap.CreatedAt,

                BatteryInfo = new BatteryInfoDto
                {
                    BatteryId = swap.Battery.BatteryId,
                    SerialNo = swap.Battery.SerialNo,
                    Voltage = swap.Battery.Voltage,
                    CapacityWh = swap.Battery.CapacityWh,
                    CurrentCapacityWh = swap.Battery.CurrentCapacityWh,
                    ImageUrl = swap.Battery.ImageUrl,
                    Status = (int)swap.Battery.Status,
                    BatteryTypeId = swap.Battery.BatteryTypeId,
                    BatteryTypeName = swap.Battery.BatteryType.BatteryTypeName
                },

                ToBatteryInfo = new BatteryInfoDto
                {
                    BatteryId = swap.ToBattery.BatteryId,
                    SerialNo = swap.ToBattery.SerialNo,
                    Voltage = swap.ToBattery.Voltage,
                    CapacityWh = swap.ToBattery.CapacityWh,
                    CurrentCapacityWh = swap.ToBattery.CurrentCapacityWh,
                    ImageUrl = swap.ToBattery.ImageUrl,
                    Status = (int)swap.ToBattery.Status,
                    BatteryTypeId = swap.ToBattery.BatteryTypeId,
                    BatteryTypeName = swap.ToBattery.BatteryType.BatteryTypeName
                }
            }).ToList();

            // Trả về PaginationWrapper
            return new PaginationWrapper<List<CompletedBatterySwapResponseDto>, CompletedBatterySwapResponseDto>(
                result,
                page,
                totalCount,
                pageSize);
        }
    }
}
