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

                if (string.IsNullOrWhiteSpace(inputuser) || string.IsNullOrWhiteSpace(inputpassword)) //Checks if both username and password were filled
                    {
                        GeneralError.IsVisible = true;
                    }
            
                else {

                    bool userfound = false; //used to check if user is in database
                    foreach (var user in userlist) //Goes through each user
                    {
                        if (user.UserName == inputuser) // Checks users password if a user is found
                        {
                            userfound = true;
                            if (user.Password == inputpassword) {
                                UserSession.Instance.LoginUser(user); //Logs in user if password is correct
                                Application.Current.MainPage = new AppShell(); //Navigates to the mainpage
                            }
                            else {
                                PasswordError.IsVisible = true; //Displays password error if it doesn't match the username given
                            }
                        }
                    }
                    if (!userfound) {
                        UsernameError.IsVisible = true; //Displays an error if after checking each user no match is found
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