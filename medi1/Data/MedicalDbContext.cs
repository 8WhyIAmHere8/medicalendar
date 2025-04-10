using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace medi1.Data
{
    public class MedicalDbContext : DbContext
    {
        private readonly string _containerName;
        public DbSet<Data.Models.Condition> Conditions { get; set; }

        public DbSet<Data.Models.User> Users { get; set; }

         public MedicalDbContext(string containerName)
        : base(new DbContextOptionsBuilder<MedicalDbContext>()
            .UseCosmos("https://medicalendar-data.documents.azure.com:443/", "MedicalDatabase")
            .Options)
    {
        _containerName = containerName;
    }
       
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
            switch (_containerName)
            {
                case "Conditions":
                    modelBuilder.Entity<Models.Condition>()
                        .ToContainer("Conditions")  // Maps to the "Conditions" container
                        .HasPartitionKey(c => c.Id) // ✅ Keep partitioning by `Id` for now
                        .HasNoDiscriminator(); // ✅ This removes the discriminator requirement
                    break;
                case "Users":
                    modelBuilder.Entity<Models.User>()
                        .ToContainer("Users")  // Maps to the "Conditions" container
                        .HasPartitionKey(u => u.Id) // ✅ Keep partitioning by `Id` for now
                        .HasNoDiscriminator(); // ✅ This removes the discriminator requirement
                    break;
            }
            
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
