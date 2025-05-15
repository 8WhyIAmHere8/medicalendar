using medi1.Services;
using Microsoft.Maui.Controls;

namespace medi1.Pages
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }
        public async void OnLogoutClicked(object sender, EventArgs e)
        {
            UserSession.Instance.LogoutUser();
            await Navigation.PushAsync(new LoginPage());

        }
    }
}
