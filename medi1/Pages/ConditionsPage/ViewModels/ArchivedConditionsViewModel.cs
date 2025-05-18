using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using medi1.Data;
using medi1.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Diagnostics;
using medi1.Services;
using System.Linq;

namespace medi1.ViewModels;

// Wrapper for display in UI
public class ConditionDisplay
{
    public medi1.Data.Models.Condition Model { get; }
    public string Name => Model.Name;
    public string Description => Model.Description;
    public string SymptomsDisplay => Model.Symptoms != null && Model.Symptoms.Any() ? string.Join(", ", Model.Symptoms) : "None";
    public string MedicationsDisplay => Model.Medications != null && Model.Medications.Any() ? string.Join(", ", Model.Medications) : "None";
    public bool Archived => Model.Archived;
    public ConditionDisplay( medi1.Data.Models.Condition model) => Model = model;
}

public partial class ArchivedConditionsViewModel : ObservableObject
{
    private readonly MedicalDbContext _dbContext;

    public ObservableCollection<ConditionDisplay> ArchivedConditions { get; } = new();

    public ArchivedConditionsViewModel()
    {
        _dbContext = new MedicalDbContext();
        UnarchiveCommand = new AsyncRelayCommand<ConditionDisplay>(UnarchiveCondition);
        LoadArchivedConditionsCommand = new AsyncRelayCommand(LoadArchivedConditions);
    }

    public IAsyncRelayCommand<ConditionDisplay> UnarchiveCommand { get; }
    public IAsyncRelayCommand LoadArchivedConditionsCommand { get; }

    private async Task LoadArchivedConditions()
    {
        try
        {
            var currentUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == UserSession.Instance.Id);
            var currentUserConditions = currentUser.Conditions;

            var conditions = await _dbContext.Conditions
                .Where(c => c.Archived && currentUserConditions.Contains(c.Id))
                .ToListAsync();

            ArchivedConditions.Clear();
            foreach (var condition in conditions)
            {
                ArchivedConditions.Add(new ConditionDisplay(condition));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ArchivedVM] Load failed: {ex.Message}");
        }
    }

    private async Task UnarchiveCondition(ConditionDisplay conditionDisplay)
    {
        try
        {
            var condition = conditionDisplay.Model;
            condition.Archived = false;
            _dbContext.Conditions.Update(condition);
            await _dbContext.SaveChangesAsync();

            ArchivedConditions.Remove(conditionDisplay);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ArchivedVM] Unarchive failed: {ex.Message}");
        }
    }
}
