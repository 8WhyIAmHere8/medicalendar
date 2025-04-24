using System;
using System.Collections.Generic;

namespace medi1.Data.Models
{
    public class Condition
    {
        public string Id { get; set; } = Guid.NewGuid().ToString(); // Auto-assign unique ID
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Archived { get; set; } = false;

        public List<string> Symptoms { get; set; } = new();
        public List<string> Medications { get; set; } = new();
        public List<string> Treatments { get; set; } = new();

        public string Notes { get; set; } = string.Empty;
    }
}
