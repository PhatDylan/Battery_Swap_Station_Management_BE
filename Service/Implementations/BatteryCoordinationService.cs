using BusinessObject;
using BusinessObject.DTOs;
using BusinessObject.Entities;
using BusinessObject.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Service.Exceptions;
using Service.Interfaces;
using Service.Utils;
using System.Net;

namespace Service.Implementations
{
    public class BatteryCoordinationService(ApplicationDbContext context, IHttpContextAccessor accessor) : IBatteryCoordinationService
    {
        public async Task<DispatchPlanResponse> PlanRebalanceAsync(RebalanceRequest request, CancellationToken ct = default)
        {
            var adminUserId = JwtUtils.GetUserId(accessor);
            if (string.IsNullOrEmpty(adminUserId))
                throw new ValidationException { StatusCode = HttpStatusCode.Unauthorized, Code = "401", ErrorMessage = "Unauthorized" };

            // Validate role
            var admin = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == adminUserId, ct);
            if (admin is null || admin.Role != UserRole.Admin)
                throw new ValidationException { StatusCode = HttpStatusCode.Forbidden, Code = "403", ErrorMessage = "Only admin can plan dispatch" };

            // Lấy danh sách station (mặc định: thuộc admin)
            var stationsQuery = context.Stations.AsNoTracking().AsQueryable();
            if (request.RestrictToMyStations) stationsQuery = stationsQuery.Where(s => s.UserId == adminUserId);
            if (request.StationIds != null && request.StationIds.Count > 0) stationsQuery = stationsQuery.Where(s => request.StationIds.Contains(s.StationId));
            var stations = await stationsQuery.ToListAsync(ct);
            if (stations.Count == 0) return new DispatchPlanResponse { BatteryTypeId = request.BatteryTypeId };

            // Đếm số pin Available theo trạm + type
            var readyByStation = await context.Batteries
                .AsNoTracking()
                .Where(b => b.Status == BatteryStatus.Available
                            && b.BatteryTypeId == request.BatteryTypeId
                            && b.StationId != null
                            && stations.Select(s => s.StationId).Contains(b.StationId!))
                .GroupBy(b => b.StationId!)
                .Select(g => new { StationId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.StationId, x => x.Count, ct);

            // Tính target
            var totalReady = readyByStation.Values.Sum();
            var n = stations.Count;
            var target = request.TargetReadyPerStation ?? (n == 0 ? 0 : (int)Math.Floor(totalReady / (double)n));

            // Tạo list dư/thiếu
            var surplus = new Queue<(string stationId, int amount)>();
            var deficit = new Queue<(string stationId, int amount)>();
            foreach (var s in stations)
            {
                var current = readyByStation.TryGetValue(s.StationId, out var c) ? c : 0;
                if (current > target) surplus.Enqueue((s.StationId, current - target));
                else if (current < target) deficit.Enqueue((s.StationId, target - current));
            }

            var plan = new List<DispatchPlanItem>();
            while (surplus.Count > 0 && deficit.Count > 0)
            {
                var (fromId, fromAmt) = surplus.Dequeue();
                var (toId, toAmt) = deficit.Dequeue();
                var move = Math.Min(fromAmt, toAmt);
                if (request.MaxTransferPerPair.HasValue && request.MaxTransferPerPair.Value > 0)
                    move = Math.Min(move, request.MaxTransferPerPair.Value);
                if (move > 0)
                {
                    plan.Add(new DispatchPlanItem
                    {
                        FromStationId = fromId,
                        ToStationId = toId,
                        BatteryTypeId = request.BatteryTypeId,
                        Quantity = move
                    });
                }
                var remFrom = fromAmt - move;
                var remTo = toAmt - move;
                if (remFrom > 0) surplus.Enqueue((fromId, remFrom));
                if (remTo > 0) deficit.Enqueue((toId, remTo));
            }

            // Dự báo số lượng sau plan
            var after = stations.ToDictionary(s => s.StationId, s => readyByStation.GetValueOrDefault(s.StationId, 0));
            foreach (var p in plan)
            {
                after[p.FromStationId] -= p.Quantity;
                after[p.ToStationId] += p.Quantity;
            }

            return new DispatchPlanResponse
            {
                BatteryTypeId = request.BatteryTypeId,
                Items = plan,
                StationReadyAfterPlan = after
            };
        }

        public async Task<ExecuteDispatchResult> ExecuteMovesAsync(ExecuteDispatchRequest request, CancellationToken ct = default)
        {
            var adminUserId = JwtUtils.GetUserId(accessor);
            if (string.IsNullOrEmpty(adminUserId))
                throw new ValidationException { StatusCode = HttpStatusCode.Unauthorized, Code = "401", ErrorMessage = "Unauthorized" };

            var admin = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == adminUserId, ct);
            if (admin is null || admin.Role != UserRole.Admin)
                throw new ValidationException { StatusCode = HttpStatusCode.Forbidden, Code = "403", ErrorMessage = "Only admin can execute dispatch" };

            var warnings = new List<string>();
            var moved = 0;
            var affected = new List<string>();

            await using var trx = await context.Database.BeginTransactionAsync(ct);
            try
            {
                foreach (var mv in request.Moves)
                {
                    // Stations phải thuộc admin
                    var fromOk = await context.Stations.AnyAsync(s => s.StationId == mv.FromStationId && s.UserId == adminUserId, ct);
                    var toOk = await context.Stations.AnyAsync(s => s.StationId == mv.ToStationId && s.UserId == adminUserId, ct);
                    if (!fromOk || !toOk)
                    {
                        warnings.Add($"Skip move {mv.FromStationId}->{mv.ToStationId}: station not owned by admin.");
                        continue;
                    }

                    // Lấy danh sách pin cần chuyển
                    List<Battery> pick;
                    if (mv.BatteryIds is { Count: > 0 })
                    {
                        pick = await context.Batteries
                            .Where(b => mv.BatteryIds.Contains(b.BatteryId)
                                    && b.Status == BatteryStatus.Available
                                    && b.BatteryTypeId == mv.BatteryTypeId
                                    && b.StationId == mv.FromStationId)
                            .ToListAsync(ct);
                    }
                    else
                    {
                        pick = await context.Batteries
                            .Where(b => b.Status == BatteryStatus.Available
                                    && b.BatteryTypeId == mv.BatteryTypeId
                                    && b.StationId == mv.FromStationId)
                            .OrderBy(b => b.UpdatedAt ?? b.CreatedAt)
                            .Take(mv.Quantity)
                            .ToListAsync(ct);
                    }

                    if (pick.Count == 0)
                    {
                        warnings.Add($"No available batteries to move from {mv.FromStationId}.");
                        continue;
                    }

                    foreach (var b in pick)
                    {
                        // Gỡ khỏi slot nếu đang gắn slot
                        if (b.StationBatterySlot != null)
                        {
                            // Attach slot để cập nhật nhanh
                            var slot = await context.StationBatterySlots.FirstOrDefaultAsync(s => s.BatteryId == b.BatteryId, ct);
                            if (slot != null)
                            {
                                slot.BatteryId = null;
                                slot.Status = SBSStatus.Available;
                                slot.LastUpdated = DateTime.UtcNow;
                            }
                        }

                        b.StationId = mv.ToStationId;
                        b.Owner = BatteryOwner.Station;
                        b.Status = BatteryStatus.Available;
                        b.UpdatedAt = DateTime.UtcNow;

                        affected.Add(b.BatteryId);
                        moved++;
                    }

                    context.Batteries.UpdateRange(pick);
                    await context.SaveChangesAsync(ct);

                    // Optional: auto-assign slot trống ở trạm đích (nếu cần)
                    // Có thể thêm logic tìm slot Available và gán BatteryId, set status Full_slot.
                }

                await trx.CommitAsync(ct);
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync(ct);
                throw new ValidationException
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Code = "400",
                    ErrorMessage = ex.Message
                };
            }

            return new ExecuteDispatchResult { Moved = moved, AffectedBatteryIds = affected, Warnings = warnings };
        }
    }
}