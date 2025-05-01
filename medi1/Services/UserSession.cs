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

         public List<Data.Models.Activity> Activities { get; set; }

         public List<Data.Models.ActivityLog> ActivityLogs { get; set; }

        public List<Data.Models.Condition> Conditions { get; set; }

        public List<Data.Models.HealthEvent> HealthEvents { get; set; }


        public bool IsLoggedIn => !string.IsNullOrEmpty(Id);

        public async void LoginUser(User givenUser) //Sets all variables to info of given user
        {
            List<Data.Models.Activity> Activitylist = new List<Data.Models.Activity>();
            List<Data.Models.ActivityLog> ActivityLoglist = new List<Data.Models.ActivityLog>();
            List<Data.Models.Condition> Conditionlist = new List<Data.Models.Condition>();
            List<Data.Models.HealthEvent> HealthEventlist = new List<Data.Models.HealthEvent>();
            using (var _dbContext = new MedicalDbContext()) //fills Current session with user data from different containers in the database
            {
                var activityIds = givenUser.Activities;
                
                Activitylist = await _dbContext.Activities
                .Where(activity => activityIds.Contains(activity.ActivityId))
                .ToListAsync();

                var activitylogIds = givenUser.ActivityLogs;

                ActivityLoglist = await _dbContext.ActivityEventLog
                .Where(activitylog => activitylogIds.Contains(activitylog.id))
                .ToListAsync();

                var conditionIds = givenUser.Conditions;
                
                Conditionlist = await _dbContext.Conditions
                .Where(condition => conditionIds.Contains(condition.Id))
                .ToListAsync();
                
                var healthEventIds = givenUser.HealthEvents;

                HealthEventlist = await _dbContext.HealthEvents
                .Where(healthevent => healthEventIds.Contains(healthevent.Id))
                .ToListAsync();
            }
            
            Id = givenUser.Id;
            UserName = givenUser.UserName;
            Password = givenUser.Password;
            Name = givenUser.Name;
            Email = givenUser.Email;
            DateOfBirth = givenUser.DateOfBirth;
            Activities = Activitylist;
            ActivityLogs = ActivityLoglist;
            Conditions = Conditionlist;
            HealthEvents = HealthEventlist;

            Debug.WriteLine(Id);
            Debug.WriteLine(Conditions[1].Name);
        }
        public void LogoutUser() // Clears user data
        {
            Id = null;
            UserName = null;
            Password = null;
            Name = null;
            Email = null;
            DateOfBirth = null;
            Activities = new List<Data.Models.Activity>();
            ActivityLogs = new List<Data.Models.ActivityLog>();
            Conditions = new List<Data.Models.Condition>();
            HealthEvents = new List<Data.Models.HealthEvent>();    
        }
    }
}
