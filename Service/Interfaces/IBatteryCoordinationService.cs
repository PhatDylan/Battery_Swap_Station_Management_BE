using BusinessObject.DTOs;

namespace Service.Interfaces
{
    public interface IBatteryCoordinationService
    {
        Task<DispatchPlanResponse> PlanRebalanceAsync(RebalanceRequest request, CancellationToken ct = default);
        Task<ExecuteDispatchResult> ExecuteMovesAsync(ExecuteDispatchRequest request, CancellationToken ct = default);
    }
}
