using medi1.Services;
using Microsoft.Maui.Controls;

namespace medi1.Pages
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();

            // Initialize switch state from current theme
            DarkModeSwitch.IsToggled = Application.Current.UserAppTheme == AppTheme.Dark;
        }

        private void DarkModeSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            // Set the app theme
            Application.Current.UserAppTheme = e.Value
                ? AppTheme.Dark
                : AppTheme.Light;
        }
        public async void OnLogoutClicked(object sender, EventArgs e)
        {
            UserSession.Instance.LogoutUser();
            await Navigation.PushAsync(new LoginPage());

        }
    }
}
