using Microsoft.EntityFrameworkCore;
using medi1.Data.Models;

namespace medi1.Pages.ConditionsPage.Interfaces
{
    public interface IMedicalDbContext
    {
        DbSet<medi1.Data.Models.Condition> Conditions { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
