using medi1.Data;
using medi1.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace medi1.Pages.ConditionsPage
{
    public partial class ConditionsPage : ContentPage, INotifyPropertyChanged
    {
        private readonly MedicalDbContext _dbContext = new MedicalDbContext();

        public ObservableCollection<Data.Models.Condition> Conditions { get; set; } = new ObservableCollection<Data.Models.Condition>();
        public ObservableCollection<HealthEvent> HealthEvents { get; set; } = new ObservableCollection<HealthEvent>();
        public ObservableCollection<HealthEvent> RecentHealthEvents { get; set; } = new ObservableCollection<HealthEvent>();

        private Data.Models.Condition? _selectedCondition;
        public Data.Models.Condition SelectedCondition
        {
            get => _selectedCondition;
            set
            {
                if (_selectedCondition != value)
                {
                    _selectedCondition = value;
                    OnPropertyChanged(nameof(SelectedCondition));
                    UpdateCollections();

                    if (_selectedCondition != null)
                    {
                        _ = LoadDataAsync(_selectedCondition.Id);
                    }
                }
            }
        }

        public new event PropertyChangedEventHandler? PropertyChanged;

        protected override void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            base.OnPropertyChanged(propertyName);
        }

        public ObservableCollection<string> Medications { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> Symptoms { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> Treatments { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> HETitles { get; set; } = new ObservableCollection<string>();

        public string NewSymptom { get; set; }
        public string NewTreatment { get; set; }

        private string? _newMedication;
        public string NewMedication
        {
            get => _newMedication;
            set
            {
                if (_newMedication != value)
                {
                    _newMedication = value;
                    OnPropertyChanged(nameof(NewMedication));
                }
            }
        }

        public Command AddConditionCommand { get; }
        public Command SaveNoteCommand { get; }
        public Command AddMedicationCommand { get; set; }
        public Command AddSymptomCommand { get; }
        public Command AddTreatmentCommand { get; }
        public Command TestCommand { get; set; }

        public ConditionsPage()
        {
            InitializeComponent();

            SaveNoteCommand = new Command(async () => await SaveCondition());
            AddMedicationCommand = new Command(async () => await AddMedication());
            AddSymptomCommand = new Command(async () => await AddSymptom());
            AddTreatmentCommand = new Command(() => AddTreatment());
            TestCommand = new Command(async () => await ButtonTest());
            AddConditionCommand = new Command(() => OnAddConditionTapped());

            BindingContext = this;

            var dbContext = new MedicalDbContext();
            TestDatabaseConnection(dbContext);
            LoadConditions();

            WeakReferenceMessenger.Default.Register<AddConditionMessage>(this, (recipient, message) =>
            {
                var newCondition = new Data.Models.Condition { Name = message.Value };
                Conditions.Add(newCondition);
                SelectedCondition = newCondition;
            });

        }
        private async void OnAddConditionTapped()
        {
            await Shell.Current.Navigation.PushModalAsync(new AddConditionPopup());
        }


        private async Task LoadDataAsync(string conditionId)
        {
            await LoadHealthEvents(conditionId);
            await LoadRecentHealthEvents();
        }

        private async Task<bool> TestDatabaseConnection(MedicalDbContext dbContext)
        {
            try
            {
                bool isConnected = await _dbContext.TestConnectionAsync();
                if (isConnected)
                {
                    Console.WriteLine("Database connected.");
                    await DisplayAlert("Success", "Connected to Cosmos DB!", "OK");
                    return true;
                }
                else
                {
                    await DisplayAlert("Error", "Failed to connect to Cosmos DB.", "OK");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database connection error: {ex.Message}");
                await DisplayAlert("Error", $"Database connection error: {ex.Message}", "OK");
                return false;
            }
        }

        private async Task ButtonTest()
        {
            Console.WriteLine("ButtonTest method called");
            await DisplayAlert("Info", "ButtonTest clicked", "OK");
        }

        private async Task LoadConditions()
        {
            try
            {
                var conditions = await _dbContext.Conditions.ToListAsync();

                if (conditions == null || conditions.Count == 0)
                {
                    await DisplayAlert("Info", "No conditions found in the database.", "OK");
                    return;
                }

                Conditions.Clear();
                foreach (var condition in conditions)
                {
                    Console.WriteLine($"Loaded condition: {condition.Name}, ID: {condition.Id}");
                    Conditions.Add(condition);
                }

                SelectedCondition = Conditions.Count > 0 ? Conditions[0] : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load conditions: {ex.Message}");
                await DisplayAlert("Error", $"Failed to load conditions: {ex.Message}", "OK");
            }
        }

        private async Task SaveCondition()
        {
            if (SelectedCondition != null)
            {
                try
                {
                    _dbContext.Conditions.Update(SelectedCondition);
                    await _dbContext.SaveChangesAsync();
                    await DisplayAlert("Success", "Condition updated successfully!", "OK");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to update condition: {ex.Message}");
                    await DisplayAlert("Error", $"Failed to update condition: {ex.Message}", "OK");
                }
            }
        }

        private void UpdateCollections()
        {
            Medications.Clear();
            Symptoms.Clear();
            Treatments.Clear();

            if (SelectedCondition != null)
            {
                if (SelectedCondition.Medications != null)
                {
                    foreach (var med in SelectedCondition.Medications)
                        Medications.Add(med);
                }

                if (SelectedCondition.Symptoms != null)
                {
                    foreach (var sym in SelectedCondition.Symptoms)
                        Symptoms.Add(sym);
                }

                if (SelectedCondition.Treatments != null)
                {
                    foreach (var treat in SelectedCondition.Treatments)
                        Treatments.Add(treat);
                }
            }
        }

        private async Task AddMedication()
        {
            await DisplayAlert("Info", $"Condition is {(SelectedCondition != null ? SelectedCondition.Name : "not selected")}", "OK");

            if (SelectedCondition != null && !string.IsNullOrWhiteSpace(NewMedication))
            {
                if (SelectedCondition.Medications == null)
                {
                    SelectedCondition.Medications = new List<string>();
                }

                SelectedCondition.Medications.Add(NewMedication);
                Medications.Add(NewMedication);

                try
                {
                    _dbContext.Conditions.Update(SelectedCondition);
                    await _dbContext.SaveChangesAsync();
                    await DisplayAlert("Success", "Medication added successfully!", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to update condition: {ex.Message}", "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", "Please select a condition and enter a valid medication.", "OK");
            }
        }

        private async Task<bool> AddSymptom()
        {
            Console.WriteLine("AddSymptom method called");
            await DisplayAlert("Info", "Clicked", "OK");
            return true;
        }

        private void AddTreatment()
        {
            if (SelectedCondition != null && !string.IsNullOrWhiteSpace(NewTreatment))
            {
                SelectedCondition.Treatments.Add(NewTreatment);
                Treatments.Add(NewTreatment);
                NewTreatment = string.Empty;
                OnPropertyChanged(nameof(NewTreatment));
            }
        }

        private async Task LoadHealthEvents(string conditionId)
        {
            try
            {
                var healthEvents = await _dbContext.HealthEvents
                    .Where(he => he.ConditionId == conditionId)
                    .ToListAsync();

                HealthEvents.Clear();
                foreach (var healthEvent in healthEvents)
                {
                    HealthEvents.Add(healthEvent);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load health events: {ex.Message}");
                await DisplayAlert("Error", $"Failed to load health events: {ex.Message}", "OK");
            }
        }

        private async Task LoadRecentHealthEvents()
        {
            if (SelectedCondition != null)
            {
                try
                {
                    var recentEvents = await _dbContext.HealthEvents
                        .Where(he => he.ConditionId == SelectedCondition.Id)
                        .OrderByDescending(he => he.StartDate)
                        .Take(5)
                        .ToListAsync();

                    RecentHealthEvents.Clear();
                    foreach (var healthEvent in recentEvents)
                    {
                        RecentHealthEvents.Add(healthEvent); // Length is calculated automatically
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load recent health events: {ex.Message}");
                    await DisplayAlert("Error", $"Failed to load recent health events: {ex.Message}", "OK");
                }
            }
        }
    }
}
