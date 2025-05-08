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

        public async Task LoginUser(User givenUser) //Sets all variables to info of given user
        {
            List<Data.Models.Activity> Activitylist = new List<Data.Models.Activity>();
            List<Data.Models.ActivityLog> ActivityLoglist = new List<Data.Models.ActivityLog>();
            List<Data.Models.Condition> Conditionlist = new List<Data.Models.Condition>();
            List<Data.Models.HealthEvent> HealthEventlist = new List<Data.Models.HealthEvent>();
            var activityIds = givenUser.Activities;
            var activitylogIds = givenUser.ActivityLogs;
            var conditionIds = givenUser.Conditions;
            var healthEventIds = givenUser.HealthEvents;
            using (var _dbContext = new MedicalDbContext()) //fills Current session with user data from different containers in the database
            {
                Activitylist = await _dbContext.Activities
                .Where(activity => activityIds.Contains(activity.ActivityId))
                .ToListAsync();

                ActivityLoglist = await _dbContext.ActivityEventLog
                .Where(activitylog => activitylogIds.Contains(activitylog.id))
                .ToListAsync();

                Conditionlist = await _dbContext.Conditions
                .Where(condition => conditionIds.Contains(condition.Id))
                .ToListAsync();

                HealthEventlist = await _dbContext.HealthEvent
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

        public async Task<ObservableCollection<Data.Models.Condition>> LoadConditions() 
        {

            List<Data.Models.Condition> Conditionlist = new List<Data.Models.Condition>();
            using (var _dbContext = new MedicalDbContext())
            {
               var loadedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == _instance.Id);

               var conditionIds = loadedUser.Conditions;
               
                Conditionlist = await _dbContext.Conditions
               .Where(c => conditionIds.Contains(c.Id))
               .ToListAsync();

               Conditions = Conditionlist;

               ObservableCollection<Data.Models.Condition> returnlist = new ObservableCollection<Data.Models.Condition>(Conditionlist);

               return returnlist;

            }
        }

        public async void SaveNewHealthEvent (HealthEvent newHealthEvent)
        {
            HealthEvents.Add(newHealthEvent);

            using (var _dbContext = new MedicalDbContext()) 
            {
                var loadedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == _instance.Id);
                loadedUser.HealthEvents.Add(newHealthEvent.Id);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async void SaveNewActivityLog (ActivityLog newActivityLog)
        {
          ActivityLogs.Add(newActivityLog);

            using (var _dbContext = new MedicalDbContext()) 
            {
                var loadedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == _instance.Id);
                loadedUser.HealthEvents.Add(newActivityLog.id);
                await _dbContext.SaveChangesAsync();
            }  
        }

        public async void SaveNewCondition (Data.Models.Condition newCondition)
        {
          Conditions.Add(newCondition);

            using (var _dbContext = new MedicalDbContext()) 
            {
                var loadedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == _instance.Id);
                loadedUser.Conditions.Add(newCondition.Id);
                await _dbContext.SaveChangesAsync();
            }  
        }
    }
}
