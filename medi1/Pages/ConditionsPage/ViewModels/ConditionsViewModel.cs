using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using medi1.Data;
using medi1.Data.Models;
using medi1.Pages.ConditionsPage;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace medi1.ViewModels
{
    public partial class ConditionsViewModel : ObservableObject
    {
        private readonly MedicalDbContext _dbContext;

        public ObservableCollection<Data.Models.Condition> Conditions { get; } = new();
        public ObservableCollection<HealthEvent> HealthEvents { get; } = new();
        [ObservableProperty]
private ObservableCollection<HealthEvent> recentHealthEvents = new();


        [ObservableProperty]
        private Data.Models.Condition? selectedCondition;

        [ObservableProperty] private string? newMedication;
        [ObservableProperty] private string? newSymptom;
        [ObservableProperty] private string? newTreatment;
        [ObservableProperty] private string? newTrigger;
        [ObservableProperty] private string? newNote;

        public ObservableCollection<string> Medications { get; } = new();
        public ObservableCollection<string> Symptoms { get; } = new();
        public ObservableCollection<string> Treatments { get; } = new();

        public ConditionsViewModel()
        {
            _dbContext = new MedicalDbContext(); // Ensure proper initialization
            LoadConditionsCommand = new AsyncRelayCommand(LoadConditionsAsync);
            // ADDING AND UPDATING COND
            AddConditionCommand = new Command(OnAddConditionTapped);
            AddMedicationCommand = new AsyncRelayCommand(AddMedicationAsync);
            AddSymptomCommand = new AsyncRelayCommand(AddSymptomAsync);
            AddTreatmentCommand = new AsyncRelayCommand(AddTreatmentAsync);
            AddTriggerCommand = new AsyncRelayCommand(AddTriggerAsync);
            UpdateNoteCommand = new AsyncRelayCommand(UpdateNoteAsync);
            ArchiveConditionCommand = new AsyncRelayCommand(ArchiveConditionAsync);
            OpenArchivedConditionsCommand = new AsyncRelayCommand(OpenArchivedConditionsAsync);


            WeakReferenceMessenger.Default.Register<AddConditionMessage>(this, (r, m) =>
            {
                var condition = new Data.Models.Condition { Name = m.Value };
                Conditions.Add(condition);
                SelectedCondition = condition;
            });
        }

        public IAsyncRelayCommand LoadConditionsCommand { get; }
        public ICommand AddConditionCommand { get; }
        public IAsyncRelayCommand LoadHealthEventsCommand { get; }
        public IAsyncRelayCommand AddMedicationCommand { get; }
        public IAsyncRelayCommand AddSymptomCommand { get; }
        public IAsyncRelayCommand AddTreatmentCommand { get; }
        public IAsyncRelayCommand AddTriggerCommand { get; }
        public IAsyncRelayCommand UpdateNoteCommand { get; }
        public IAsyncRelayCommand ArchiveConditionCommand { get; }

        public IAsyncRelayCommand OpenArchivedConditionsCommand { get; }

        partial void OnSelectedConditionChanged(Data.Models.Condition? oldValue, Data.Models.Condition? newValue)
        {
            if (newValue != null)
            {
                LoadConditionDetailsAsync(newValue.Id);
            }
        }

        private async Task LoadConditionDetailsAsync(string conditionId)
        {
            await LoadHealthEvents(conditionId);
            await LoadRecentHealthEvents();
            UpdateCollections();    
        }

        private void UpdateCollections()
        {
            Medications.Clear();
            Symptoms.Clear();
            Treatments.Clear();

            if (SelectedCondition != null)
            {
                foreach (var m in SelectedCondition.Medications ?? []) Medications.Add(m);
                foreach (var s in SelectedCondition.Symptoms ?? []) Symptoms.Add(s);
                foreach (var t in SelectedCondition.Treatments ?? []) Treatments.Add(t);
            }
        }
      
        private async Task LoadConditionsAsync()
        {
            try
            {
                var list = await _dbContext.Conditions.Where(c => !c.Archived).ToListAsync();
                Conditions.Clear();
                foreach (var c in list) Conditions.Add(c);
                SelectedCondition = Conditions.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading conditions: {ex.Message}");
            }
        }

        private async Task LoadHealthEvents(string conditionId)
        {
            var list = await _dbContext.HealthEvents.Where(e => e.ConditionId == conditionId).ToListAsync();
            HealthEvents.Clear();
            foreach (var e in list) HealthEvents.Add(e);
        }

        private async Task LoadRecentHealthEvents()
        {
            if (SelectedCondition == null) return;

            var list = await _dbContext.HealthEvents
                .Where(e => e.ConditionId == SelectedCondition.Id)
                .OrderByDescending(e => e.StartDate)
                .Take(5)
                .ToListAsync();

            // Clear and update the existing collection
            RecentHealthEvents = new ObservableCollection<HealthEvent>(list);


            Debug.WriteLine($"[Chart Debug] Loaded {RecentHealthEvents.Count} recent events for condition: {SelectedCondition.Name}");
        }


// ADDING  AND UPDATING  CONDITIONS

        private async void OnAddConditionTapped()
        {
            await Shell.Current.Navigation.PushModalAsync(new AddConditionPopup());
        }
        private async Task AddMedicationAsync()
        {
            if (SelectedCondition == null || string.IsNullOrWhiteSpace(NewMedication)) return;

            SelectedCondition.Medications ??= new List<string>();
            SelectedCondition.Medications.Add(NewMedication);
            Medications.Add(NewMedication);
            await SaveCondition();
        }

        private async Task AddSymptomAsync()
        {
            if (SelectedCondition == null || string.IsNullOrWhiteSpace(NewSymptom)) return;

            SelectedCondition.Symptoms ??= new List<string>();
            SelectedCondition.Symptoms.Add(NewSymptom);
            Symptoms.Add(NewSymptom);
            await SaveCondition();
        }

        private async Task AddTreatmentAsync()
        {
            if (SelectedCondition == null || string.IsNullOrWhiteSpace(NewTreatment)) return;

            SelectedCondition.Treatments ??= new List<string>();
            SelectedCondition.Treatments.Add(NewTreatment);
            Treatments.Add(NewTreatment);
            await SaveCondition();
        }

        private async Task AddTriggerAsync()
        {
            if (SelectedCondition == null || string.IsNullOrWhiteSpace(NewTrigger)) return;

            SelectedCondition.Triggers ??= new List<string>();
            SelectedCondition.Triggers.Add(NewTrigger);
            await SaveCondition();
        }

        private async Task UpdateNoteAsync()
        {
            if (SelectedCondition == null || string.IsNullOrWhiteSpace(NewNote)) return;

            SelectedCondition.Notes = NewNote;
            await SaveCondition();
        }
// ARCHIVED CONDITIONS BIT
        private async Task ArchiveConditionAsync()
        {
            if (SelectedCondition == null) return;

            SelectedCondition.Archived = true;
            await SaveCondition();
            Conditions.Remove(SelectedCondition);
            SelectedCondition = null;
        }
         private async Task OpenArchivedConditionsAsync()
        {
            await Shell.Current.GoToAsync(nameof(ArchivedConditionsPage));
        }

        private async Task SaveCondition()
        {
            _dbContext.Conditions.Update(SelectedCondition!);
            await _dbContext.SaveChangesAsync();
        }
    }
}
