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
        private readonly MedicalDbContext _dbContext = new MedicalDbContext("Users"); //creates dbcontext With the users container

        public ObservableCollection<Data.Models.User> Users { get; set; } = new ObservableCollection<Data.Models.User>();

        public LoginPage()
        {
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e) //Runs when Login is clicked
        {   
            //Retrieves input
            string inputuser = UsernameEntry.Text;
            string inputpassword = PasswordEntry.Text;

            //Resets Currently displayed errors
            UsernameError.IsVisible = false;
            PasswordError.IsVisible = false;
            GeneralError.IsVisible = false;

            var userlist = await _dbContext.Users.ToListAsync();
            Users.Clear();

            if (string.IsNullOrWhiteSpace(inputuser) || string.IsNullOrWhiteSpace(inputpassword)) //Checks if both username and password were filled
                {
                    GeneralError.IsVisible = true;
                }
            
            else {

                foreach (var user in userlist) //Goes through each user
                {
                    if (user.UserName == inputuser)
                    {
                        if (user.Password == inputpassword) {
                            UserSession.Instance.Id = user.Id; //Accesses and changes Current logged in user
                            UserSession.Instance.UserName = user.UserName;
                            Application.Current.MainPage = new AppShell(); //Navigates to the mainpage
                        }
                        else {
                            PasswordError.IsVisible = true; //Displays password error if it doesn't match the username given
                        }
                    }
                    else {
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