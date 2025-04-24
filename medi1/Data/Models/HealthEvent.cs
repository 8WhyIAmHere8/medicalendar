using System;
using System.Collections.Generic;

namespace medi1.Data.Models
{
    public class HealthEvent
    {
        public string Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Duration { get; set; }

        public string RelatedCondition { get; set; }

        public int Impact { get; set; }  // 0-10 scale

        public string Notes { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    }
}