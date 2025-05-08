using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using medi1.Data;
using medi1.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
// alias to avoid conflict with MAUI.Controls.Condition
using DCondition = medi1.Data.Models.Condition;

namespace medi1.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage : ContentPage, INotifyPropertyChanged
    {
        // EF Core context for reading stored conditions
        private readonly MedicalDbContext _dbContext = new MedicalDbContext();

        public List<Data.Models.Condition> InitializedConditions { get; set; } = new();

        // ── Calendar state ──
        private DateTime _displayDate;
        public ObservableCollection<DayItem> DaysInMonth { get; private set; } = new();
        
        public string CurrentMonth  { get; private set; }
        public string FullDateToday { get; private set; }

        // ── Data collections ──
        public ObservableCollection<ConditionViewModel> Conditions    { get; } = new();
        public ObservableCollection<HealthEventViewModel> HealthEvents { get; } = new();
        public ObservableCollection<ActivityLogViewModel> ActivityLogs { get; } = new();
                // For editing tasks
        private Label _editingTaskLabel;

        public HomePage()
        {
            InitializeComponent();
            BindingContext = this;

            _displayDate  = DateTime.Today;
            FullDateToday = _displayDate.ToString("MMMM dd, yyyy");
            LoadMonth(_displayDate);

        }
        private async void AddNewEntry(object sender, EventArgs e)
            => await Navigation.PushModalAsync(new AddEntryPage());
 
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                await ReloadAllAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Startup Error", ex.ToString(), "OK");
                Debug.WriteLine(ex);
            }
        }

        // ── Prev/Next month ──
        private async void OnPrevMonthClicked(object sender, EventArgs e)
        {
            _displayDate = _displayDate.AddMonths(-1);
            LoadMonth(_displayDate);
            await ReloadAllAsync();
        }

        private async void OnNextMonthClicked(object sender, EventArgs e)
        {
            _displayDate = _displayDate.AddMonths(1);
            LoadMonth(_displayDate);
            await ReloadAllAsync();
        }

        // ── Reload all data, then map into calendar ──
        private async Task ReloadAllAsync()
        {
            await LoadConditionsFromDbAsync();
            await LoadHealthEventsFromDbAsync();
            await LoadActivityLogsFromDbAsync();
            MapDataIntoCalendar();
        }

        // ── Load Conditions (no date filter) ──
        private async Task LoadConditionsFromDbAsync()
        {
            InitializedConditions = UserSession.Instance.Conditions;
            Conditions.Clear();
            if (InitializedConditions == null)
            {
                return;
            }

            for (int i = 0; i < InitializedConditions.Count; i++)
            for (int i = 0; i < list.Count; i++)
            {
                var e     = list[i];
                var color = GenerateColor(i);
                Conditions.Add(new ConditionViewModel {
                    Id          = e.Id,
                    Name        = e.Name,
                    Description = e.Description,
                    Symptoms    = string.Join(", ", e.Symptoms),
                    Medications = string.Join(", ", e.Medications),
                    Treatments  = string.Join(", ", e.Treatments),
                    Notes       = e.Notes,
                    Color       = color
                });
            }
        }

        // ── Load Health Events filtered by month/year ──
        private async Task LoadHealthEventsFromDbAsync()
        {
            var raw = await _dbContext.HealthEvent
                .Where(h => h.StartDate.HasValue
                         && h.StartDate.Value.Month == _displayDate.Month
                         && h.StartDate.Value.Year  == _displayDate.Year)
                .Select(h => new {
                    h.Id, h.Title, h.StartDate, h.EndDate, h.Duration, h.Impact, h.Notes
                })
                .AsNoTracking()
                .ToListAsync();

            HealthEvents.Clear();
            for (int i = 0; i < raw.Count; i++)
            {
                var e     = raw[i];
                var color = GenerateColor(i);

                HealthEvents.Add(new HealthEventViewModel {
                    Id         = e.Id,
                    Title      = e.Title,
                    StartDate  = e.StartDate!.Value,
                    EndDate    = e.EndDate!.Value,
                    Duration   = e.Duration,
                    Impact     = e.Impact,
                    Notes      = e.Notes,
                    EventColor = color
                });
            }
        }

        // ── Load Activity Logs filtered by month/year ──
        private async Task LoadActivityLogsFromDbAsync()
        {
            var raw = await _dbContext.ActivityEventLog
                .Where(a => a.Date.HasValue
                         && a.Date.Value.Month == _displayDate.Month
                         && a.Date.Value.Year  == _displayDate.Year)
                .Select(a => new {
                    a.ActivityLogId, a.Name, a.Intensity, a.Date,
                    a.Duration, a.AggravatedCondition, a.Notes
                })
                .AsNoTracking()
                .ToListAsync();

            ActivityLogs.Clear();
            foreach (var a in raw)
            {
                ActivityLogs.Add(new ActivityLogViewModel {
                    Id                  = a.ActivityLogId,
                    Name                = a.Name,
                    Intensity           = a.Intensity,
                    Date                = a.Date!.Value,
                    Duration            = a.Duration,
                    AggravatedCondition = a.AggravatedCondition,
                    Notes               = a.Notes
                });
            }
        }

        // ── Map loaded data into each DayItem’s Entries ──
        private void MapDataIntoCalendar()
        {
            foreach (var day in DaysInMonth)
                day.Entries.Clear();

            int year  = _displayDate.Year;
            int month = _displayDate.Month;
            int daysInMonth = DateTime.DaysInMonth(year, month);

            // Health Events: span range or single day
            foreach (var ev in HealthEvents)
            {
                if (ev.StartDate.Date == ev.EndDate.Date)
                {
                    var cell = DaysInMonth.FirstOrDefault(x => x.DayNumber == ev.StartDate.Day);
                    cell?.Entries.Add(new CalendarEntry {
                        Text     = ev.Title,
                        DotColor = ev.EventColor
                    });
                }
                else
                {
                    int from = Math.Max(ev.StartDate.Day, 1);
                    int to   = Math.Min(ev.EndDate.Day, daysInMonth);
                    for (int d = from; d <= to; d++)
                    {
                        var cell = DaysInMonth.FirstOrDefault(x => x.DayNumber == d);
                        cell?.Entries.Add(new CalendarEntry {
                            Text     = ev.Title,
                            DotColor = ev.EventColor
                        });
                    }
                }
            }

            // Activity Logs
            foreach (var act in ActivityLogs)
            {
                if (act.Date.Year == year && act.Date.Month == month)
                {
                    var cell = DaysInMonth.FirstOrDefault(x => x.DayNumber == act.Date.Day);
                    cell?.Entries.Add(new CalendarEntry {
                        Text     = act.Name,
                        DotColor = null
                    });
                }
            }

            OnPropertyChanged(nameof(DaysInMonth));
        }

        // ── Calendar month cells ──
        private void LoadMonth(DateTime date)
        {
            var items = new ObservableCollection<DayItem>();
            int offset = (int)new DateTime(date.Year, date.Month, 1).DayOfWeek;
            for (int i = 0; i < offset; i++)
                items.Add(new DayItem { DayNumber = 0, IsToday = false });

            int days = DateTime.DaysInMonth(date.Year, date.Month);
            for (int d = 1; d <= days; d++)
            {
                items.Add(new DayItem {
                    DayNumber = d,
                    IsToday   = date.Year == DateTime.Today.Year &&
                                date.Month == DateTime.Today.Month &&
                                d == DateTime.Today.Day
                });
            }

            DaysInMonth  = items;
            CurrentMonth = date.ToString("MMMM yyyy");
            OnPropertyChanged(nameof(DaysInMonth));
            OnPropertyChanged(nameof(CurrentMonth));
        }

        // ── Helpers & navigation ──
        private Color GenerateColor(int idx)
        {
            const float φ = 0.618033988749895f;
            // Light pastel colors: moderate saturation, high lightness
            return Color.FromHsla((idx * φ) % 1f, 0.4f, 0.85f);
        }

        private async void GoToConditions(object s, EventArgs e)
            => await Navigation.PushAsync(new ConditionsPage.ConditionsPage());
        private async void GoToAddEntry(object s, EventArgs e)
            => await Navigation.PushAsync(new AddEntryPage());
        private async void GoToReports(object s, EventArgs e)
            => await Navigation.PushAsync(new ReportsPage());

        private async void OnConditionTapped(object s, EventArgs e)
        {
            if (s is StackLayout sl && sl.BindingContext is ConditionViewModel cvm)
            {
                await DisplayAlert("Condition Details",
                    $"Id: {cvm.Id}\nName: {cvm.Name}\nDescription: {cvm.Description}\n" +
                    $"Symptoms: {cvm.Symptoms}\nMedications: {cvm.Medications}\n" +
                    $"Treatments: {cvm.Treatments}\nNotes: {cvm.Notes}",
                    "OK");
            }
        }

        // ── INotifyPropertyChanged ──
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName]string n = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    
        private StackLayout CreateTaskLayout(CalendarTask task)
    {
        var localTaskId = task.TaskId;
 
        var taskLayout = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            Spacing     = 10
        };
 
        var checkBox = new CheckBox
        {
            IsChecked = task.CompletionStatus
        };
 
        var label = new Label
        {
            Text            = task.Description,
            VerticalOptions = LayoutOptions.Center,
            BindingContext  = task.TaskId,
            TextDecorations = task.CompletionStatus
                ? TextDecorations.Strikethrough
                : TextDecorations.None
        };
 
        checkBox.CheckedChanged += async (s, a) =>
        {
            label.TextDecorations = checkBox.IsChecked
                ? TextDecorations.Strikethrough
                : TextDecorations.None;
 
            try
            {
                using var dbContext = new MedicalDbContext();
                var taskToUpdate = await dbContext.TaskLog
                    .FirstOrDefaultAsync(t => t.TaskId == localTaskId);
 
                if (taskToUpdate != null)
                {
                    taskToUpdate.CompletionStatus = checkBox.IsChecked;
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update error: {ex.Message}");
            }
        };
 
        var editBtn = new Button
        {
            Text            = "✏️",
            FontSize        = 12,
            BackgroundColor = Colors.Transparent,
            WidthRequest    = 40
        };
 
        editBtn.Clicked += (s, a) =>
        {
            _editingTaskLabel = label;
            EditTaskInput.Text = label.Text;
            EditTaskPopup.IsVisible = true;
        };
 
        var deleteBtn = new Button
        {
            Text            = "🗑️",
            FontSize        = 12,
            BackgroundColor = Colors.Transparent,
            WidthRequest    = 40
        };
 
        deleteBtn.Clicked += async (s, a) =>
        {
            TaskListContainer.Children.Remove(taskLayout);
 
            try
            {
                using var dbContext = new MedicalDbContext();
                var taskToDelete = await dbContext.TaskLog
                    .FirstOrDefaultAsync(t => t.TaskId == localTaskId);
 
                if (taskToDelete != null)
                {
                    dbContext.TaskLog.Remove(taskToDelete);
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete error: {ex.Message}");
            }
        };
 
        taskLayout.Children.Add(checkBox);
        taskLayout.Children.Add(label);
        taskLayout.Children.Add(editBtn);
        taskLayout.Children.Add(deleteBtn);
 
        return taskLayout;
    }

      //Load Tasks when Home Page is loaded
        private async Task LoadIncompleteTasksAsync()
        {
            TaskListContainer.Children.Clear();
 
            try
            {
                using var dbContext = new MedicalDbContext();
                var tasks = await dbContext.TaskLog
                    .Where(t => !t.CompletionStatus)
                    .ToListAsync();
 
                foreach (var task in tasks)
                {
                    var taskLayout = CreateTaskLayout(task);
                    TaskListContainer.Children.Add(taskLayout);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load tasks: {ex.Message}");
            }
        }
          private void OnAddTaskClicked(object sender, EventArgs e)
            => AddTaskPopup.IsVisible = true;
 
        private async void OnConfirmTaskClicked(object sender, EventArgs e)
        {
            var taskText = TaskInput.Text?.Trim();
            var taskId = Guid.NewGuid().ToString();
            if (string.IsNullOrEmpty(taskText))
                return;
            else {
                var newTask = new CalendarTask
                {
                    id = Guid.NewGuid().ToString(),
                    TaskId = taskId,
                    Description = taskText,
                    CompletionStatus = false
                };
                try
                {
                    using var dbContext = new MedicalDbContext();
                    await dbContext.TaskLog.AddAsync(newTask);
                    await dbContext.SaveChangesAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    Console.WriteLine($"Database Update Error: {dbEx.Message}");
                    if (dbEx.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {dbEx.InnerException.Message}");
                    }
                    await DisplayAlert("Error", "Failed to save your task.", "OK");
                }
 
                TaskListContainer.Children.Add(CreateTaskLayout(newTask));
 
                AddTaskPopup.IsVisible = false;
                TaskInput.Text = string.Empty;
 
            }
        }
 
        private void OnCancelTaskClicked(object sender, EventArgs e)
        {
            AddTaskPopup.IsVisible = false;
            TaskInput.Text = string.Empty;
        }
 
        //Edit Confirm
        private async void TaskEditConfirmClicked(object sender, EventArgs e)
        {
            if (_editingTaskLabel != null)
            {
                _editingTaskLabel.Text = EditTaskInput.Text;
                var taskId = _editingTaskLabel.BindingContext?.ToString();
 
                if (!string.IsNullOrEmpty(taskId))
                {
                    try
                    {
                        using var dbContext = new MedicalDbContext();
                        var taskToUpdate = await dbContext.TaskLog
                            .FirstOrDefaultAsync(t => t.TaskId == taskId);
 
                        if (taskToUpdate != null)
                        {
                            taskToUpdate.Description = EditTaskInput.Text;
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    catch (DbUpdateException dbEx)
                    {
                        Console.WriteLine($"Database Update Error: {dbEx.Message}");
                        if (dbEx.InnerException != null)
                        {
                            Console.WriteLine($"Inner Exception: {dbEx.InnerException.Message}");
                        }
                        await DisplayAlert("Error", "Failed to update the task.", "OK");
                    }
                }
            }
 
            EditTaskPopup.IsVisible = false;
        }
 
 
 
        private void TaskEditCancelClicked(object sender, EventArgs e)
        {
            EditTaskPopup.IsVisible = false;
        }    
    }



    // ── Converters ──
    public class NullToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value != null;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    // ── ViewModel and calendar types ──
    public class ConditionViewModel
    {
        public string Id          { get; set; }
        public string Name        { get; set; }
        public string Description { get; set; }
        public string Symptoms    { get; set; }
        public string Medications { get; set; }
        public string Treatments  { get; set; }
        public string Notes       { get; set; }
        public Color  Color       { get; set; }
    }

    public class HealthEventViewModel
    {
        public string   Id         { get; set; }
        public string   Title      { get; set; }
        public DateTime StartDate  { get; set; }
        public DateTime EndDate    { get; set; }
        public string   Duration   { get; set; }
        public int      Impact     { get; set; }
        public string   Notes      { get; set; }
        public Color    EventColor { get; set; }
    }

    public class ActivityLogViewModel
    {
        public string   Id                  { get; set; }
        public string   Name                { get; set; }
        public string   Intensity           { get; set; }
        public DateTime Date                { get; set; }
        public string   Duration            { get; set; }
        public string   AggravatedCondition { get; set; }
        public string   Notes               { get; set; }
    }

    public class CalendarEntry
    {
        public string Text { get; set; }
        public Color? DotColor { get; set; }
    }

    public class DayItem
    {
        public int DayNumber { get; set; }
        public bool IsToday  { get; set; }
        public ObservableCollection<CalendarEntry> Entries { get; } = new();
    }


}

