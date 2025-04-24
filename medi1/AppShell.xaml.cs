using medi1.Pages;
using Microsoft.Maui.Controls;

namespace medi1
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));

            this.FlyoutBehavior = FlyoutBehavior.Disabled;

            this.Navigated += (s,e) =>
            {
                if (CurrentPage is LoginPage || CurrentPage is RegisterPage)
                    this.FlyoutBehavior = FlyoutBehavior.Disabled;
                else
                    this.FlyoutBehavior = FlyoutBehavior.Flyout;
            };
            
        }
    }
}
