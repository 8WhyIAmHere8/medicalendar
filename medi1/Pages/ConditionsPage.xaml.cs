using medi1.Data; // Import database context
using medi1.Data.Models; // Import Condition model
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel; // Allows using ObservableCollection
using System.Threading.Tasks; // Allows using async/await
using System.Diagnostics; // Allows using Debug.WriteLine for logging

namespace medi1.Pages
{
    public partial class ConditionsPage : ContentPage
    {
        private readonly MedicalDbContext _dbContext = new MedicalDbContext(); // Create an instance of the database context

        public ObservableCollection<Data.Models.Condition> Conditions { get; set; } = new ObservableCollection<Data.Models.Condition>(); // List of conditions

        private Data.Models.Condition _selectedCondition;
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

        public ObservableCollection<string> Medications { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> Symptoms { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> Treatments { get; set; } = new ObservableCollection<string>();

        public string NewMedication { get; set; }
        public string NewSymptom { get; set; }
        public string NewTreatment { get; set; }

        public Command AddConditionCommand { get; }
        public Command SaveNoteCommand { get; }
        public Command AddMedicationCommand { get; }
        public Command AddSymptomCommand { get; }
        public Command AddTreatmentCommand { get; }

        public ConditionsPage()
        {
            InitializeComponent();
            BindingContext = this;

            AddConditionCommand = new Command(async () => await AddCondition());
            SaveNoteCommand = new Command(async () => await SaveCondition());
            AddMedicationCommand = new Command(AddMedication);
            AddSymptomCommand = new Command(AddSymptom);
            AddTreatmentCommand = new Command(AddTreatment);

            var dbContext = new MedicalDbContext();
            TestDatabaseConnection(dbContext);
            LoadConditions();
        }

        private async Task<bool> TestDatabaseConnection(MedicalDbContext dbContext)
        {
            try
            {
                bool isConnected = await _dbContext.TestConnectionAsync();
                if (isConnected)
                {
                    await DisplayAlert("✅ Success", "Connected to Cosmos DB!", "OK");
                    return true;
                }
                else
                {
                    await DisplayAlert("❌ Error", "Failed to connect to Cosmos DB.", "OK");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Database connection error: {ex.Message}");
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
                    Debug.WriteLine($"Loaded condition: {condition.Name}, ID: {condition.Id}");
                    Conditions.Add(condition);
                }

                SelectedCondition = Conditions.Count > 0 ? Conditions[0] : null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Failed to load conditions: {ex.Message}");
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
                Debug.WriteLine($"❌ Failed to add condition: {ex.Message}");
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
                    Debug.WriteLine($"❌ Failed to update condition: {ex.Message}");
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

        private void AddMedication()
        {
            if (SelectedCondition != null && !string.IsNullOrWhiteSpace(NewMedication))
            {
                SelectedCondition.Medications.Add(NewMedication);
                Medications.Add(NewMedication);
                NewMedication = string.Empty;
                OnPropertyChanged(nameof(NewMedication));
            }
        }

        private void AddSymptom()
        {
            if (SelectedCondition != null && !string.IsNullOrWhiteSpace(NewSymptom))
            {
                SelectedCondition.Symptoms.Add(NewSymptom);
                Symptoms.Add(NewSymptom);
                NewSymptom = string.Empty;
                OnPropertyChanged(nameof(NewSymptom));
            }
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
