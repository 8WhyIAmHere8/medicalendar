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

    public ICommand ClosePopupCommand { get; }
    public ICommand ConfirmAddCommand { get; }

    public AddConditionPopupViewModel(
        IMedicalDbContext dbContext,
        IAlertService alertService,
        INavigationService navigationService)
    {
        _dbContext = dbContext;
        _alertService = alertService;
        _navigationService = navigationService;

        ClosePopupCommand = new Command(async () => await _navigationService.PopModalAsync());

        ConfirmAddCommand = new Command(async () => await AddConditionAsync());
    }

    private async Task AddConditionAsync()
    {
        if (string.IsNullOrWhiteSpace(NewConditionName))
        {
            await _alertService.ShowAlert("Validation", "Condition name cannot be empty.", "OK");
            return;
        }

        var newCondition = new medi1.Data.Models.Condition
        {
            Id = Guid.NewGuid().ToString(),
            Name = NewConditionName.Trim(),
            Archived = false,
            Description = string.Empty,
            Notes = string.Empty,
            Symptoms = new(),
            Medications = new(),
            Treatments = new()
        };

        try
        {
            _dbContext.Conditions.Add(newCondition);
            await _dbContext.SaveChangesAsync();

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
