using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Dtos
{
    public class BatteryTypeRequest
    {
        public string? BatteryTypeId { get; set; }
        public string BatteryTypeName { get; set; } = string.Empty;
    }

    public class BatteryTypeResponse
    {
        public string BatteryTypeId { get; set; } = string.Empty;
        public string BatteryTypeName { get; set; } = string.Empty;
    }
}

