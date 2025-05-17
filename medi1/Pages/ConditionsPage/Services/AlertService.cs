using System.Threading.Tasks;

namespace medi1.Pages.ConditionsPage.Services
{
    public class AlertService : IAlertService
    {
        public async Task ShowAlert(string title, string message, string cancel)
        {
            await Application.Current.MainPage.DisplayAlert(title, message, cancel);
        }
    }
}
