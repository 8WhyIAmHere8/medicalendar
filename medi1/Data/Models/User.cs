using System;
using System.Collections.Generic;

namespace medi1.Data.Models
{
    public class User
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public string Email {get; set; } = string.Empty;

        public string DateOfBirth { get; set; } = string.Empty;

        public List<string> Activities { get; set; } = new List<string>(); 

        public List<string> ActivityLogs { get; set; } = new List<string>();

        public List<string> Conditions { get; set; } = new List<string>();

        public List<string> HealthEvents { get; set; } = new List<string>();

    }
}
