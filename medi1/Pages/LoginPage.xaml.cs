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
        private readonly MedicalDbContext _dbContext = new MedicalDbContext("Users");

        public ObservableCollection<Data.Models.User> Users { get; set; } = new ObservableCollection<Data.Models.User>();

        public LoginPage()
        {
            InitializeComponent();
 
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {   
            string inputuser = UsernameEntry.Text;
            string inputpassword = PasswordEntry.Text;

            UsernameError.IsVisible = false;
            PasswordError.IsVisible = false;
            GeneralError.IsVisible = false;

            var userlist = await _dbContext.Users.ToListAsync();
            Users.Clear();

            if (string.IsNullOrWhiteSpace(inputuser) || string.IsNullOrWhiteSpace(inputpassword)) 
                {
                    GeneralError.IsVisible = true;
                }
            
            else {

                foreach (var user in userlist)
                {
                    if (user.UserName == inputuser)
                    {
                        if (user.Password == inputpassword){
                            UserSession.Instance.Id = user.Id; //Accesses and changes session info
                            UserSession.Instance.UserName = user.UserName;
                            Application.Current.MainPage = new AppShell();
                        }
                        else {
                            PasswordError.IsVisible = true;
                        }
                    }
                    else {
                        UsernameError.IsVisible = true;
                    }
                }
            }
        }

        private async void OnNewClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage());
        }
    }
}