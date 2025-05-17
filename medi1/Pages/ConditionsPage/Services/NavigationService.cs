using System.Threading.Tasks;

namespace medi1.Pages.ConditionsPage.Services
{
    public class NavigationService : INavigationService
    {
        public async Task PopModalAsync()
        {
            await Shell.Current.Navigation.PopModalAsync();
        }
    }
}