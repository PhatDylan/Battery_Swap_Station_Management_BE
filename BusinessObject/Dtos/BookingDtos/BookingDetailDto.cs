using BusinessObject.DTOs.BatterySwap;

namespace BusinessObject.DTOs.Booking
{
    public class BookingDetailDto : BookingResponseDto
    {
        public List<BatterySwapSummaryDto> RelatedSwaps { get; set; } = new List<BatterySwapSummaryDto>();
        public bool HasSwaps => RelatedSwaps.Any();
        public DateTime? LastSwapTime => RelatedSwaps.OrderByDescending(s => s.SwappedAt).FirstOrDefault()?.SwappedAt;
    }
}
