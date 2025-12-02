using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Dtos
{
    public class ReviewRequest
    {
        public string? ReviewId { get; set; }
        public string UserId { get; set; }
        public string StationId { get; set; }
        public string SwapId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }

    public class ReviewResponse
    {
        public string ReviewId { get; set; }
        public string UserId { get; set; }
        public string StationId { get; set; }
        public string SwapId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
