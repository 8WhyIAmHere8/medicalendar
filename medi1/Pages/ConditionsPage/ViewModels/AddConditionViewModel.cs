using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using medi1.Data;
using medi1.Data.Models;
using medi1.Services;
using Microsoft.EntityFrameworkCore;

public class AddConditionPopupViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private string newConditionName;
    public string NewConditionName
    {
        get => newConditionName;
        set
        {
            newConditionName = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NewConditionName)));
        }
    }

    public ICommand ClosePopupCommand { get; }
    public ICommand ConfirmAddCommand { get; }
 
    public AddConditionPopupViewModel()
    {
        ClosePopupCommand = new Command(async () => await Shell.Current.Navigation.PopModalAsync());

        ConfirmAddCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(NewConditionName))
            {
                await Application.Current.MainPage.DisplayAlert("Validation", "Condition name cannot be empty.", "OK");
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
                using var dbContext = new MedicalDbContext();
                dbContext.Conditions.Add(newCondition);
                await dbContext.SaveChangesAsync();

                WeakReferenceMessenger.Default.Send(new AddConditionMessage(newCondition.Name));
                await Shell.Current.Navigation.PopModalAsync();
                UserSession.Instance.SaveNewCondition(newCondition);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding condition: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to save condition.", "OK");
            }
        });
    }
}
