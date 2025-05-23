using Microsoft.Azure.Cosmos.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace medi1.Data
{
    public class MedicalDbContext : DbContext, medi1.Pages.ConditionsPage.Interfaces.IMedicalDbContext
    {
        private readonly string _containerName;
        public DbSet<Data.Models.Condition> Conditions { get; set; }
        public DbSet<Data.Models.HealthEvent> HealthEvent { get; set; }
        public DbSet<Data.Models.Activity> Activities { get; set; }
        public DbSet<Data.Models.ActivityLog> ActivityEventLog { get; set; }
        public DbSet<Data.Models.User> Users { get; set; }
        public DbSet<Data.Models.CalendarTask> TaskLog { get; set; }   

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseCosmos(
                "https://medicalendar-data.documents.azure.com:443/", // cosmosDB link
                "Masked due to being sensitive. Can provide if needed.", // cosmosDB key 
                "MedicalDatabase"); 

            optionsBuilder.LogTo(Console.WriteLine); // logging database queries
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Condition>()
                .ToContainer("Conditions")
                .HasPartitionKey(c => c.Id)
                .HasNoDiscriminator();

            modelBuilder.Entity<Models.HealthEvent>()
                .ToContainer("HealthEvent")
                .HasPartitionKey(e => e.Id)
                .HasNoDiscriminator();

                modelBuilder.Entity<Models.Activity>()
            .ToContainer("Activities")
            .HasPartitionKey(a => a.ActivityId)
            .HasNoDiscriminator();
                
                modelBuilder.Entity<Models.ActivityLog>()
            .ToContainer("ActivityEventLog")
            .HasPartitionKey(al => al.ActivityLogId)
            .HasNoDiscriminator();

            modelBuilder.Entity<Models.User>()
            .ToContainer("Users")  
            .HasPartitionKey(u => u.Id) 
            .HasNoDiscriminator();

            modelBuilder.Entity<Models.CalendarTask>()
            .ToContainer("TaskLog")
            .HasPartitionKey(t => t.TaskId)
            .HasNoDiscriminator();
        
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                await Database.EnsureCreatedAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cosmos DB connection failed: {ex.Message}");
                return false;
            }
        }
    }
}
