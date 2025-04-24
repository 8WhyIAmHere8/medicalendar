using medi1.Pages;
using Microsoft.Maui.Controls;

namespace medi1
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new LoginPage());
        }
    }
}
