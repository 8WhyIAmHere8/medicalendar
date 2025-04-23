using medi1.Data; // Import database context
using medi1.Data.Models; // Import Condition model
using medi1.Services; //used to import current user session
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel; // Allows using ObservableCollection
using System.Threading.Tasks; // Allows using async/await
using System.Diagnostics;
using System.Linq.Expressions; // Allows using Debug.WriteLine for logging

namespace medi1.Services
{
    public class UserSession //Session class that keeps track of user login (Only 1 Instance exists)
    {
        private static UserSession _instance;
        
        private UserSession() { }
        public static UserSession Instance 
        {
            get 
            {
                if (_instance == null)
                {
                    _instance = new UserSession();
                }
                return _instance;
            }
        }
        public string Id { get; set; }
        public string UserName { get; set; }

        public string Password { get; set; }
        public string Name { get; set; }

        public string Email {get; set; }

        public string DateOfBirth { get; set; }

        public List<string> Conditions { get; set; }
        
        public List<string> Activities { get; set; }
        public List<string> Symptoms { get; set; }

        public bool IsLoggedIn => !string.IsNullOrEmpty(Id);

        public void LoginUser(User givenUser) //Sets all variables to info of given user
        {
            Id = givenUser.Id;
            UserName = givenUser.UserName;
            Password = givenUser.Password;
            Name = givenUser.Name;
            Email = givenUser.Email;
            DateOfBirth = givenUser.DateOfBirth;
            Conditions = givenUser.Conditions;
            Activities = givenUser.Activities;
            Symptoms = givenUser.Symptoms;
        }
        public void LogoutUser() // Clears user data
        {
            Id = null;
            UserName = null;
            Password = null;
            Name = null;
            Email = null;
            DateOfBirth = null;
            Conditions = null;
            Activities = null;
            Symptoms = null;            
        }
    }
}
