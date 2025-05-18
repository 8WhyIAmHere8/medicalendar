using Microsoft.EntityFrameworkCore;
using medi1.Data.Models;
using medi1.Services;

namespace medi1.Pages.ConditionsPage.Interfaces
{
    public interface IMedicalDbContext
    {   
        DbSet<Data.Models.User> Users { get; }
        DbSet<medi1.Data.Models.Condition> Conditions { get; }
        

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
