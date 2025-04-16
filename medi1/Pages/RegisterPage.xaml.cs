using Microsoft.Maui.Controls;
using medi1.Data; // Import database context
using medi1.Data.Models; // Import Condition model
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel; // Allows using ObservableCollection
using System.Threading.Tasks; // Allows using async/await
using System.Diagnostics;
using Microsoft.Azure.Cosmos; // Allows using Debug.WriteLine for loggin

namespace medi1.Pages
{
    public partial class RegisterPage : ContentPage
    {
        private readonly MedicalDbContext _dbContext = new MedicalDbContext("Users"); //creates dbcontext With the users container

        public ObservableCollection<Data.Models.User> Users { get; set; } = new ObservableCollection<Data.Models.User>();
        public RegisterPage()
        {
            InitializeComponent();
        }      
            
            
            private async void OnRegisterClicked(object sender, EventArgs e) //Runs when Register button is clicked
        {   
            DateTime defaultDate = DateTime.Today; //Finds todays date
            
            //Takes content of the fields
            string inputname = NameEntry.Text;
            string inputemail = EmailEntry.Text;
            DateTime inputDOB = DatePicker.Date;
            string inputusername = UsernameEntry.Text;
            string inputpassword = PasswordEntry.Text;
            string confirminputpassword = ConfirmPasswordEntry.Text;

            //Resets currently displayed errors
            GeneralError.IsVisible = false;
            EmailError.IsVisible = false;
            DOBError.IsVisible = false;
            UsernameError.IsVisible = false;
            PasswordError.IsVisible = false;

            List<bool> fieldchecks = [string.IsNullOrWhiteSpace(inputname),
                                      string.IsNullOrWhiteSpace(inputemail),
                                      string.IsNullOrWhiteSpace(inputusername),
                                      string.IsNullOrWhiteSpace(inputpassword),
                                      string.IsNullOrWhiteSpace(confirminputpassword)]; //Each entry is checked
            
            var userlist = await _dbContext.Users.ToListAsync(); //Gets a list of all users
            Users.Clear();
            bool usernamematch = false;
            foreach (var user in userlist) //Goes through each user to find a matching username if it exists
            {
                if (user.UserName == inputusername) 
                {
                    usernamematch = true;
                }
                if (usernamematch == true) 
                {
                    break;
                }
            }

            if (fieldchecks.Contains(true)) //Checks id any fields are empty
            {
                GeneralError.IsVisible = true;
            }
            
            else if (!inputemail.Contains("@")) // Checks if email doesn't include @
            {
                EmailError.IsVisible = true;
            }
            
            else if (defaultDate == inputDOB) //Checks if user selected a date
            {
                DOBError.IsVisible = true;
            }

            else if (usernamematch) //Checks if username is already in use
            {
                UsernameError.IsVisible = true;
            }

            else if (inputpassword != confirminputpassword) //Checks if the passwords do not match
            {
                PasswordError.IsVisible = true;
            }

            else
            {
                var newUser = new Data.Models.User //Creates a new database entry
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = inputusername,
                    Password = inputpassword,
                    Name = inputname,
                    Email = inputemail,
                    DateOfBirth = inputDOB.ToString(),
                    Conditions = null,
                    Activities = null,
                    Symptoms = null
                };
                try
                {
                    _dbContext.Users.Add(newUser); //Tries to add user to database does not work currently
                    Debug.WriteLine("New user added");
                }
                catch (CosmosException ex)
                {
                    Debug.WriteLine($"Cosmos DB Error: {ex.StatusCode} - {ex.Message}");
                }

                await Navigation.PopAsync(); //Navigates back to login page
            }





            
        }
    }
}