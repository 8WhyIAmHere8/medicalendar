using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using medi1.Data;
using medi1.Data.Models;
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
            if (!string.IsNullOrWhiteSpace(NewConditionName))
            {
                // Add to your collection logic here
                WeakReferenceMessenger.Default.Send(new AddConditionMessage(NewConditionName));
                await Shell.Current.Navigation.PopModalAsync();
            }
        });
    }
}
