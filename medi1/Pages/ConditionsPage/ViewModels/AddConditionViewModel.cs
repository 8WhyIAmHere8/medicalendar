using System;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using medi1.Data.Models;
using medi1.Services;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using medi1.Pages.ConditionsPage.Interfaces;

public class AddConditionPopupViewModel : INotifyPropertyChanged
{
    private readonly IMedicalDbContext _dbContext;
    private readonly IAlertService _alertService;
    private readonly INavigationService _navigationService;
    private readonly string RelatedSymptom;

    public event PropertyChangedEventHandler PropertyChanged;

    private string newConditionName;
    public string NewConditionName
    {
        get => newConditionName;
        set
        {
            if (newConditionName != value)
            {
                newConditionName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NewConditionName)));
            }
        }
    }

    private string symptomsInput;
    public string SymptomsInput
    {
        get => symptomsInput;
        set
        {
            if (symptomsInput != value)
            {
                symptomsInput = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SymptomsInput)));
            }
        }
    }

    public ICommand ClosePopupCommand { get; }
    public ICommand ConfirmAddCommand { get; }

    public AddConditionPopupViewModel(string relatedSymptom, string healthEventID,
        IMedicalDbContext dbContext,
        IAlertService alertService,
        INavigationService navigationService)
    {
        _dbContext = dbContext;
        _alertService = alertService;
        _navigationService = navigationService;

        ClosePopupCommand = new Command(async () => await _navigationService.PopModalAsync());

        if (!string.IsNullOrWhiteSpace(relatedSymptom))
        {
            SymptomsInput = relatedSymptom;
            RelatedSymptom = relatedSymptom;
        }

        ConfirmAddCommand = new Command(async () => await AddConditionAsync(healthEventID));
    }

    public async Task AddConditionAsync(string healthEventID)
    {
        if (string.IsNullOrWhiteSpace(NewConditionName))
        {
            await _alertService.ShowAlert("Validation", "Condition name cannot be empty.", "OK");
            return;
        }

        // checking for duplicates
        var currentUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == UserSession.Instance.Id);
        if (currentUser == null)
        {
            await _alertService.ShowAlert("Error", "Current user not found.", "OK");
            return;
        }
        var currentUserConditions = currentUser.Conditions;
        var list = await _dbContext.Conditions
                .Where(c => !c.Archived && currentUserConditions.Contains(c.Id))
                .ToListAsync();

        var duplicate = list.Any(c => c.Name.Equals(NewConditionName.Trim(), StringComparison.OrdinalIgnoreCase));

        if (duplicate)
        {
            await _alertService.ShowAlert("Duplicate", "A condition with this name already exists.", "OK");
            return;
        }

        var newCondition = new medi1.Data.Models.Condition
        {
            Id = Guid.NewGuid().ToString(),
            Name = NewConditionName.Trim(),
            Archived = false,
            Description = string.Empty,
            Notes = string.Empty,
            Medications = new(),
            Treatments = new(),
            Triggers = new()
        };

        if (!string.IsNullOrWhiteSpace(SymptomsInput))
        {
            newCondition.Symptoms = SymptomsInput.Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }
        else
        {
            newCondition.Symptoms = new();
        }

        try
        {
            _dbContext.Conditions.Add(newCondition);

    
            if (!currentUser.Conditions.Contains(newCondition.Id))
            {
                currentUser.Conditions.Add(newCondition.Id);
                _dbContext.Users.Update(currentUser);
            }

            await _dbContext.SaveChangesAsync();

            if (!string.IsNullOrEmpty(healthEventID))
            {
                var evt = await _dbContext.HealthEvent.FindAsync(healthEventID);
                if (evt != null)
                {
                    evt.ConditionId = newCondition.Id;
                    await _dbContext.SaveChangesAsync();
                }
            }

            WeakReferenceMessenger.Default.Send(new AddConditionMessage(newCondition.Name));
            await _navigationService.PopModalAsync();
            UserSession.Instance.SaveNewCondition(newCondition);
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding condition: {ex.Message}");
            await _alertService.ShowAlert("Error", "Failed to save condition.", "OK");
        }
    }
}
