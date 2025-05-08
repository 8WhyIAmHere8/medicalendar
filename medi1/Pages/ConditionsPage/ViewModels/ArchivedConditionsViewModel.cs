using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using medi1.Data;
using medi1.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Diagnostics;
using medi1.Services;

namespace medi1.ViewModels;

public partial class ArchivedConditionsViewModel : ObservableObject
{
    private readonly MedicalDbContext _dbContext;

    public ObservableCollection<Data.Models.Condition> ArchivedConditions { get; } = new();
    
    public ArchivedConditionsViewModel()
    {
        _dbContext = new MedicalDbContext();
        UnarchiveCommand = new AsyncRelayCommand<Data.Models.Condition>(UnarchiveCondition);
        LoadArchivedConditionsCommand = new AsyncRelayCommand(LoadArchivedConditions);
    }

    public IAsyncRelayCommand<Data.Models.Condition> UnarchiveCommand { get; }
    public IAsyncRelayCommand LoadArchivedConditionsCommand { get; }

    private async Task LoadArchivedConditions()
    {
        try
        {   var currentUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == UserSession.Instance.Id);
            var currentUserConditions = currentUser.Conditions;
        
            var conditions = await _dbContext.Conditions
                 .Where(c => c.Archived && currentUserConditions.Contains(c.Id))
                .ToListAsync();

            ArchivedConditions.Clear();
            foreach (var condition in conditions)
            {
                ArchivedConditions.Add(condition);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ArchivedVM] Load failed: {ex.Message}");
        }
    }

    private async Task UnarchiveCondition(Data.Models.Condition condition)
    {
        try
        {
            condition.Archived = false;
            _dbContext.Conditions.Update(condition);
            await _dbContext.SaveChangesAsync();

            ArchivedConditions.Remove(condition);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ArchivedVM] Unarchive failed: {ex.Message}");
        }
    }
}
