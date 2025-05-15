using medi1.Data; // Import database context
using medi1.Data.Models; // Import Condition model
using medi1.Services; //used to import current user session
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel; // Allows using ObservableCollection
using System.Threading.Tasks; // Allows using async/await
using System.Diagnostics;
using System.Linq.Expressions; // Allows using Debug.WriteLine for logging


namespace medi1.Pages
{
    public partial class LoginPage : ContentPage
    {

        public ObservableCollection<Data.Models.User> Users { get; set; } = new ObservableCollection<Data.Models.User>();

        public LoginPage()
        {
            InitializeComponent();
        }

        public async void OnLoginClicked(object sender, EventArgs e) //Runs when Login is clicked
        {   
            //Retrieves input
            string inputuser = UsernameEntry.Text;
            string inputpassword = PasswordEntry.Text;

            //Clears errors
            GeneralError.IsVisible = false;
            UsernameError.IsVisible = false;
            PasswordError.IsVisible = false;
            
            using (var _dbContext = new MedicalDbContext())
            {
                var userlist = await _dbContext.Users.ToListAsync();
                Users.Clear();

                if (string.IsNullOrWhiteSpace(inputuser) || string.IsNullOrWhiteSpace(inputpassword))
                {
                    GeneralError.IsVisible = true;
                }
                else
                {
                    var user = userlist.FirstOrDefault(u => u.UserName == inputuser);
                    if (user == null)
                    {
                        UsernameError.IsVisible = true;
                    }
                    else if (user.Password != inputpassword)
                    {
                        PasswordError.IsVisible = true;
                    }
                    else
                    {
                        await UserSession.Instance.LoginUser(user);
                        Application.Current.MainPage = new AppShell();
                    }
                }
            }
        }

        private async void OnNewClicked(object sender, EventArgs e) //Navigates to the register page
        {
            await Navigation.PushAsync(new RegisterPage());
        }
    }
}