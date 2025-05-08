using System;
using System.Collections.Generic;

namespace medi1.Data.Models
{
    public class HealthEvent
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }    
    public DateTime? EndDate { get; set; }      
    public string Duration { get; set; } = string.Empty;
    public string HealthRelationship { get; set; } = string.Empty;
    public string ConditionId { get; set; } = string.Empty;
    public int Impact { get; set; }            
    public string Notes { get; set; } = string.Empty;
    public DateTime? Timestamp { get; set; } = DateTime.UtcNow; 
}


}