using System;
using System.Collections.Generic;

namespace medi1.Data.Models
{
    public class Condition
    {
        public string Id { get; set; } = Guid.NewGuid().ToString(); // Auto-assign unique ID
        public string Name { get; set; } = string.Empty;
        public DateTime? Date { get; set; }  
        public string Description { get; set; } = string.Empty;
        public bool Archived { get; set; } = false;

        public List<string> Symptoms { get; set; } = new List<string>();
        public List<string> Medications { get; set; } = new List<string>();
        public List<string> Treatments { get; set; } = new List<string>();
        public List<string> Triggers {get; set; } = new List<string>();

        public string Notes { get; set; } = string.Empty;
    }
}
