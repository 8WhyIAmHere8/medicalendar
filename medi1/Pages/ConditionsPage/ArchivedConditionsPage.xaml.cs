using medi1.Data;
using medi1.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace medi1.Pages.ConditionsPage;

    public partial class ArchivedConditionsPage : ContentPage
    {
        // EF Core context for reading stored conditions
        private readonly MedicalDbContext _dbContext = new MedicalDbContext();

        // Conditions list with notification on change
        public ObservableCollection<medi1.Data.Models.Condition> ArchivedConditions { get; set; } = new ObservableCollection<medi1.Data.Models.Condition>();
        public ObservableCollection<HealthEvent> HealthEvents { get; set; } = new ObservableCollection<HealthEvent>();
        public ObservableCollection<HealthEvent> RecentHealthEvents { get; set; } = new ObservableCollection<HealthEvent>();

        public ArchivedConditionsPage()
        {
            InitializeComponent();
            BindingContext = this;

            // Load archived conditions from the database
            LoadArchivedConditions();
        }

        private async Task LoadArchivedConditions()
        {
            try
            {
                var conditions = await _dbContext.Conditions
                    .Where(c => c.Archived) // Filter archived conditions
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
    }
