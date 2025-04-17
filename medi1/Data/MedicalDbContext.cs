using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace medi1.Data
{
    public class MedicalDbContext : DbContext
    {
        public DbSet<Data.Models.Condition> Conditions { get; set; }
        public DbSet<Data.Models.HealthEvent> HealthEvents { get; set; }
        public DbSet<Data.Models.Activity> Activities { get; set; }
        public DbSet<Data.Models.ActivityLog> ActivityEventLog { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseCosmos(
                "https://medicalendar-data.documents.azure.com:443/", // Cosmos DB endpoint
                "ukEwRy20KzAics3MYQfmnzwXC0IxPQMGd8MfvPCQthLpkW691AMAqS1cSPz5aS6z77WAz3Sgy9I8ACDbywHjig==", // Cosmos DB key
                "MedicalDatabase"); // Database name 

            optionsBuilder.LogTo(Console.WriteLine); // Log database queries
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Condition>()
        .ToContainer("Conditions")  // Maps to the "Conditions" container
        .HasPartitionKey(c => c.Id) // ✅ Keep partitioning by `Id` for now
        .HasNoDiscriminator(); // ✅ This removes the discriminator requirement
       
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
