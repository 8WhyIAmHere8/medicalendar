using medi1.Data;
using medi1.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Windows.Input;
using medi1.Services;

namespace medi1.Pages.ConditionsPage;

    public partial class ArchivedConditionsPage : ContentPage
    {
        // EF Core context for reading stored conditions
        private readonly MedicalDbContext _dbContext = new MedicalDbContext();

        // Conditions list with notification on change
        public ObservableCollection<medi1.Data.Models.Condition> ArchivedConditions { get; set; } = new ObservableCollection<medi1.Data.Models.Condition>();
        public ObservableCollection<HealthEvent> HealthEvents { get; set; } = new ObservableCollection<HealthEvent>();
        public ObservableCollection<HealthEvent> RecentHealthEvents { get; set; } = new ObservableCollection<HealthEvent>();

        public ICommand UnarchiveCommand { get; }

        public ArchivedConditionsPage()
        {
            InitializeComponent();
            BindingContext = this;

            UnarchiveCommand = new Command<medi1.Data.Models.Condition>(async (condition) => await UnarchiveCondition(condition));

            // Load archived conditions from the database
            LoadArchivedConditions();
        }

        private async Task LoadArchivedConditions()
        {
            try
            {
                var currentUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == UserSession.Instance.Id);
                var currentUserConditions = currentUser.Conditions;
                var conditions = await _dbContext.Conditions
                    .Where(c => c.Archived && currentUserConditions.Contains(c.Id)) // Filter archived conditions
                    .ToListAsync();

                if (conditions == null || conditions.Count == 0)
                {
                    await DisplayAlert("Info", "No active conditions found in the database.", "OK");
                    return;
                }

                ArchivedConditions.Clear();
                foreach (var condition in conditions)
                {
                    Console.WriteLine($"Loaded condition: {condition.Name}, ID: {condition.Id}");
                    ArchivedConditions.Add(condition);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load conditions: {ex.Message}");
                await DisplayAlert("Error", $"Failed to load conditions: {ex.Message}", "OK");
            }
        }

        private async Task UnarchiveCondition(medi1.Data.Models.Condition condition)
        {
            try
            {
                condition.Archived = false;
                _dbContext.Conditions.Update(condition);
                await _dbContext.SaveChangesAsync();

                ArchivedConditions.Remove(condition);
                await DisplayAlert("Success", $"{condition.Name} has been unarchived.", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to unarchive condition: {ex.Message}");
                await DisplayAlert("Error", $"Failed to unarchive condition: {ex.Message}", "OK");
            }
        }
    }
