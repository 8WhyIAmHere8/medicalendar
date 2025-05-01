using System;
using System.Collections.Generic;

namespace medi1.Data.Models
{
    public class CalendarTask
    {
        public string id { get; set; }
        public string TaskId { get; set; }
        public string Description { get; set; }
        public Boolean CompletionStatus { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;


        
    }
}