using System;
using System.Collections.Generic;

namespace medi1.Data.Models
{
    public class ActivityLog
    {
        
        public string id { get; set; }  = string.Empty;
        public string ActivityLogId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Intensity {get; set; } = string.Empty;
        public DateTime? Date { get; set; }

        public string Duration { get; set; } = string.Empty;

        public string AggravatedCondition {get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;
        public DateTime? Timestamp { get; set; } = DateTime.UtcNow;

    }
}