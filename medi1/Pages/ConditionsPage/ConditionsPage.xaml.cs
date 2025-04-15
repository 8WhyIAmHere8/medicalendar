using medi1.Data; // Import database context
using medi1.Data.Models; // Import Condition model
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel; // Allows using ObservableCollection
using System.Threading.Tasks; // Allows using async/await
using System.Diagnostics; // Allows using Console.WriteLine for logging
using System.ComponentModel;
namespace medi1.Pages.ConditionsPage
{
    public partial class ConditionsPage : ContentPage, INotifyPropertyChanged
    {
    
        private readonly MedicalDbContext _dbContext = new MedicalDbContext(); // Create an instance of the database context

        public ObservableCollection<Data.Models.Condition> Conditions { get; set; } = new ObservableCollection<Data.Models.Condition>(); // List of conditions

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
                }
            }
        }
        public new event PropertyChangedEventHandler? PropertyChanged;

        protected override void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            base.OnPropertyChanged(propertyName); // Call the base class implementation
        }

        public ObservableCollection<string> Medications { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> Symptoms { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> Treatments { get; set; } = new ObservableCollection<string>();

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
        public string NewSymptom { get; set; }
        public string NewTreatment { get; set; }

        public Command AddConditionCommand { get; }
        public Command SaveNoteCommand { get; }
        public Command AddMedicationCommand { get; set; }
        public Command AddSymptomCommand { get; }
        public Command AddTreatmentCommand { get; }

        public Command TestCommand { get; set; } 

        public ConditionsPage()
        {
            InitializeComponent();
            // Ensure BindingContext is set to the current instance

            // Initialize commands
            AddConditionCommand = new Command(async () => await AddCondition());
            SaveNoteCommand = new Command(async () => await SaveCondition());
            AddMedicationCommand = new Command(async () => await AddMedication());
            AddSymptomCommand = new Command(async () => await AddSymptom());
            AddTreatmentCommand = new Command(() => AddTreatment());
            TestCommand = new Command(async () => await ButtonTest());
            BindingContext = this;
            // Test database connection and load conditions
            var dbContext = new MedicalDbContext();
            TestDatabaseConnection(dbContext);
            LoadConditions();
        }
        
        private async Task ButtonTest()
        {
            Console.WriteLine("ButtonTest method called");
            await DisplayAlert("Info", "ButtonTest clicked", "OK");
            // You can add more logic here if needed
            // For example, you can call another method or perform some action
            // await SomeOtherMethod();
        }
        
        private async Task<bool> TestDatabaseConnection(MedicalDbContext dbContext)
        {
            try
            {
                bool isConnected = await _dbContext.TestConnectionAsync();
                if (isConnected)
                {   Console.WriteLine("here");
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

        private async Task AddCondition()
        {
            try
            {
                var newCondition = new Data.Models.Condition
                {
                    Name = "Asthma",
                    Description = "A condition that affects the airways.",
                    Symptoms = new List<string> { "Shortness of breath", "Coughing" },
                    Medications = new List<string> { "Ventolin", "Claritine" },
                    Treatments = new List<string> { "Daily inhaler", "Allergy shots" },
                    Notes = "Keep track of symptoms daily."
                };

                _dbContext.Conditions.Add(newCondition);
                await _dbContext.SaveChangesAsync();

                Conditions.Add(newCondition);
                SelectedCondition = newCondition;
                await DisplayAlert("Success", "Condition added successfully!", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Failed to add condition: {ex.Message}");
                await DisplayAlert("Error", $"Failed to add condition: {ex.Message}", "OK");
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
                foreach (var med in SelectedCondition.Medications)
                    Medications.Add(med);
                foreach (var sym in SelectedCondition.Symptoms)
                    Symptoms.Add(sym);
                foreach (var treat in SelectedCondition.Treatments)
                    Treatments.Add(treat);
            }
        }

        private async Task AddMedication()
        {
           await DisplayAlert("Info", $"Condition is {(SelectedCondition != null ? SelectedCondition.Name : "not selected")}", "OK");
           

            if (SelectedCondition != null && !string.IsNullOrWhiteSpace(NewMedication))
            {
                SelectedCondition.Medications.Add(NewMedication);
                Medications.Add(NewMedication);
                try
                {
                    _dbContext.Conditions.Update(SelectedCondition);
                    await _dbContext.SaveChangesAsync();
                    await DisplayAlert("Yepp", "Medication added successfully!", "OK");
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
    }
}
