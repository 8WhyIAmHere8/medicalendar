using Microsoft.Maui.Controls;

namespace medi1.Pages
{
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

            private async void OnRegisterClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}