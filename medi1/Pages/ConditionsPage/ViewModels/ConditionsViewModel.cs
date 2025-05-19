using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
using medi1.Services;

namespace medi1.ViewModels
{
    public partial class ConditionsViewModel : ObservableObject
    {
        private readonly MedicalDbContext _dbContext;

        public ObservableCollection<Data.Models.Condition> Conditions { get; } = new();
        
        public ObservableCollection<Data.Models.HealthEvent> HealthEvents { get; } = new();

        [ObservableProperty]
        private Data.Models.Condition? selectedCondition;

        [ObservableProperty] private string? newMedication;
        [ObservableProperty] private string? newSymptom;
        [ObservableProperty] private string? newTreatment;
        [ObservableProperty] private string? newTrigger;
        [ObservableProperty] private string? newNote;
        private SKCanvasView? _canvasView;

        public ObservableCollection<string> Medications { get; } = new();
        public ObservableCollection<string> Symptoms { get; } = new();
        public ObservableCollection<string> Treatments { get; } = new();

        public ConditionsViewModel(MedicalDbContext? dbContext = null)
        {
            _dbContext = dbContext;
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
            ExportConditionCommand = new AsyncRelayCommand(ExportConditionAsync);
            DeleteConditionCommand = new AsyncRelayCommand(DeleteConditionAsync); // <-- Add this

            WeakReferenceMessenger.Default.Register<AddConditionMessage>(this, async (r, m) =>
{
    await MainThread.InvokeOnMainThreadAsync(async () =>
    {
        await LoadConditionsAsync();
        var added = Conditions.FirstOrDefault(c => c.Name == m.Value);
        if (added != null)
            SelectedCondition = added;
    });
});

        }

        public IAsyncRelayCommand LoadConditionsCommand { get; }
        public ICommand AddConditionCommand { get; }
        public IAsyncRelayCommand LoadHealthEventCommand { get; }
        public IAsyncRelayCommand AddMedicationCommand { get; }
        public IAsyncRelayCommand AddSymptomCommand { get; }
        public IAsyncRelayCommand AddTreatmentCommand { get; }
        public IAsyncRelayCommand AddTriggerCommand { get; }
        public IAsyncRelayCommand UpdateNoteCommand { get; }
        public IAsyncRelayCommand ArchiveConditionCommand { get; }
        public IAsyncRelayCommand ExportConditionCommand { get; }

        public IAsyncRelayCommand OpenArchivedConditionsCommand { get; }
        public IAsyncRelayCommand DeleteConditionCommand { get; } // <-- Add this

        partial void OnSelectedConditionChanged(Data.Models.Condition? oldValue, Data.Models.Condition? newValue)
        {
            if (newValue != null)
            {
                LoadConditionDetailsAsync(newValue.Id);
                LoadHealthEventsAsync(newValue.Id);
                InvalidateChart();
            }
        }

        private async Task LoadConditionDetailsAsync(string conditionId)
        {
            await LoadHealthEventsAsync(conditionId); 
            UpdateCollections();
            InvalidateChart();
        }

        private void UpdateCollections()
        {
            Medications.Clear();
            Symptoms.Clear();
            Treatments.Clear();

            if (SelectedCondition != null)
            {
                // ensuring lists are initialised
                SelectedCondition.Medications ??= new List<string>();
                SelectedCondition.Symptoms ??= new List<string>();
                SelectedCondition.Treatments ??= new List<string>();

                foreach (var m in SelectedCondition.Medications) Medications.Add(m);
                foreach (var s in SelectedCondition.Symptoms) Symptoms.Add(s);
                foreach (var t in SelectedCondition.Treatments) Treatments.Add(t);
            }
            InvalidateChart();
        }

        private async Task LoadConditionsAsync()
        {
            try
            {
                using (var freshDbContext = new MedicalDbContext())
                {
                    var currentUser = await freshDbContext.Users.FirstOrDefaultAsync(u => u.Id == UserSession.Instance.Id);
                    var currentUserConditions = currentUser.Conditions;
                    var list = await freshDbContext.Conditions
                        .Where(c => !c.Archived && currentUserConditions.Contains(c.Id))
                        .ToListAsync();
                    Conditions.Clear();
                    foreach (var c in list) Conditions.Add(c);
                    SelectedCondition = Conditions.FirstOrDefault();
                    InvalidateChart();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading conditions: {ex.Message}");
            }
        }

        private async Task LoadHealthEventsAsync(string conditionId)
        {
            var list = await _dbContext.HealthEvent
                .Where(he => he.ConditionId == conditionId)
                .OrderByDescending(e => e.StartDate)
                .ToListAsync();

            HealthEvents.Clear();
            foreach (var item in list)
                HealthEvents.Add(item);

            Debug.WriteLine($"[Debug] HealthEvents count: {HealthEvents.Count}");
            InvalidateChart();
        }

        // ADDING AND UPDATING CONDITIONS

        private async void OnAddConditionTapped()
        {
            await Shell.Current.Navigation.PushModalAsync(new AddConditionPopup("", "")); // Pass empty strings
            InvalidateChart();
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
            // db is skipped for testing
            if (_dbContext == null) return;

            _dbContext.Conditions.Update(SelectedCondition!);
            await _dbContext.SaveChangesAsync();
            await LoadConditionDetailsAsync(SelectedCondition!.Id);
            UpdateCollections();
        }

        private async Task ExportConditionAsync()
        {
            if (SelectedCondition == null)
            {
                Debug.WriteLine("No condition selected to export.");
                return;
            }

            try
            {
                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"{SelectedCondition.Name}_Details.txt");
                using (var writer = new StreamWriter(filePath))
                {
                    await writer.WriteLineAsync($"Condition: {SelectedCondition.Name}");
                    await writer.WriteLineAsync($"Medications: {string.Join(", ", SelectedCondition.Medications ?? new List<string>())}");
                    await writer.WriteLineAsync($"Symptoms: {string.Join(", ", SelectedCondition.Symptoms ?? new List<string>())}");
                    await writer.WriteLineAsync($"Treatments: {string.Join(", ", SelectedCondition.Treatments ?? new List<string>())}");
                    await writer.WriteLineAsync($"Triggers: {string.Join(", ", SelectedCondition.Triggers ?? new List<string>())}");
                    await writer.WriteLineAsync($"Notes: {SelectedCondition.Notes}"); 
                }
                await Application.Current.MainPage.DisplayAlert("Export Successful", $"Condition details exported to {filePath}", "OK");
                Debug.WriteLine($"Condition details exported to {filePath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error exporting condition: {ex.Message}");
            }
        }

        private async Task DeleteConditionAsync()
        {
            if (SelectedCondition == null || _dbContext == null) return;

          
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Delete Condition",
                $"Are you sure you want to delete \"{SelectedCondition.Name}\"? This cannot be undone.",
                "Yes", "No");

            if (!confirm)
                return;

            try
            {
                _dbContext.Conditions.Remove(SelectedCondition);
                await _dbContext.SaveChangesAsync();
                Conditions.Remove(SelectedCondition);
                SelectedCondition = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting condition: {ex.Message}");
            }
        }

        // Chart Stuff

        private float _scale = 1f;
        public void ZoomIn()
        {
            _scale = Math.Min(_scale + 0.1f, 5f);
            InvalidateChart();
        }

        public void ZoomOut()
        {
            _scale = Math.Max(_scale - 0.1f, 0.5f);
            InvalidateChart();
        }
        [ObservableProperty]
        private DateTime startDate = DateTime.Today.AddDays(-30);

        partial void OnStartDateChanged(DateTime value)
        {
            InvalidateChart();
        }

        [ObservableProperty]
        private DateTime endDate = DateTime.Today;

        partial void OnEndDateChanged(DateTime value)
        {
            InvalidateChart();
        }

        public void AttachCanvas(SKCanvasView canvasView)
        {
            _canvasView = canvasView;
        }

        private void InvalidateChart()
        {
            _canvasView?.InvalidateSurface();
        } 

        public float GetScale() => _scale;
        public void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var recentEvents = HealthEvents;
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.White);

            if (StartDate >= EndDate)
                return;

            float canvasWidth = e.Info.Width;
            float canvasHeight = e.Info.Height;

            float startX = 60;
            float startY = canvasHeight - 80;
            float baseBarWidth = 20;
            float baseSpace = 10;
            float scale = GetScale();

            float barWidth = baseBarWidth * scale;
            float space = baseSpace * scale;

            var barPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
            var gridPaint = new SKPaint
            {
                Color = SKColors.LightGray,
                StrokeWidth = 1,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };
            var textPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 14,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            };

            //  horizontal lines
            for (int y = 0; y <= 10; y++)
            {
                float yLine = startY - y * 10;
                canvas.DrawLine(startX - 20, yLine, canvasWidth - 20, yLine, gridPaint);
            }

            // x-axis
            canvas.DrawLine(startX - 20, startY, canvasWidth - 20, startY, new SKPaint
            {
                Color = SKColors.Gray,
                StrokeWidth = 2
            });

            var dateRange = Enumerable.Range(0, (EndDate - StartDate).Days + 1)
                .Select(i => StartDate.AddDays(i))
                .ToList();

            for (int i = 0; i < dateRange.Count; i++)
            {
                var date = dateRange[i];
                float centerX = startX + i * (barWidth + space);

                // drawign event bar
                var ev = recentEvents.FirstOrDefault(e => e.StartDate <= date && e.EndDate >= date);
                if (ev != null)
                {
                    int impactHeight = ev.Impact * 10;
                    float barHeight = Math.Max(impactHeight, 10);
                    float top = startY - barHeight;

                    barPaint.Color = ev.Impact switch
                    {
                        <= 3 => SKColors.LightGreen,
                        <= 6 => SKColors.Gold,
                        <= 9 => SKColors.OrangeRed,
                        10 => SKColors.DarkRed,
                        _ => SKColors.Gray
                    };

                    var rect = new SKRect(centerX - barWidth / 2, top, centerX + barWidth / 2, startY);
                    canvas.DrawRoundRect(rect, 4, 4, barPaint);

                    // Draw short title label
                    string label = Truncate(ev.Title, 8);
                    canvas.DrawText(label, centerX, startY + 20, textPaint);
                }

                // date labels 
                int labelInterval = (int)(1 / scale);
                if (labelInterval < 1) labelInterval = 1;

                if (i % labelInterval == 0)
                {
                    canvas.Save();
                    canvas.RotateDegrees(-45, centerX, startY + 40);
                    canvas.DrawText(date.ToString("MM/dd"), centerX, startY + 40, textPaint);
                    canvas.Restore();
                }
            }
        }

        private string Truncate(string value, int maxLength)
        {
            return value.Length <= maxLength ? value : value.Substring(0, maxLength - 1) + "…";
        }



    }
}
