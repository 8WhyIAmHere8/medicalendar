using System;
using System.Collections.Generic;

namespace medi1.Data.Models
{
    public class HealthEvent
    {
        public string Id { get; set; }
        public string ConditionId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int Severity { get; set; }  // 0-10 scale
        public List<string> Symptoms { get; set; } = new();
        public List<string> MedicationsTaken { get; set; } = new();
        public List<string> Triggers { get; set; } = new();
        public string Notes { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    }
}