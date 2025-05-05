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
        private readonly MedicalDbContext _dbContext = new();

        // ── Calendar state ──
        private DateTime _displayDate;
        public ObservableCollection<DayItem> DaysInMonth { get; private set; } = new();
        public string CurrentMonth  { get; private set; }
        public string FullDateToday { get; private set; }

        // ── Data collections ──
        public ObservableCollection<ConditionViewModel> Conditions    { get; } = new();
        public ObservableCollection<HealthEventViewModel> HealthEvents { get; } = new();
        public ObservableCollection<ActivityLogViewModel> ActivityLogs { get; } = new();

        public HomePage()
        {
            InitializeComponent();
            BindingContext = this;

            _displayDate  = DateTime.Today;
            FullDateToday = _displayDate.ToString("MMMM dd, yyyy");
            LoadMonth(_displayDate);
        }

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

        // ── Load Conditions (no date filter, add one if you extend your model) ──
        private async Task LoadConditionsFromDbAsync()
        {
            var list = await _dbContext.Conditions.ToListAsync();
            Conditions.Clear();
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
            foreach (var e in raw)
            {
                HealthEvents.Add(new HealthEventViewModel {
                    Id        = e.Id,
                    Title     = e.Title,
                    StartDate = e.StartDate!.Value,
                    EndDate   = e.EndDate!.Value,
                    Duration  = e.Duration,
                    Impact    = e.Impact ?? 0,
                    Notes     = e.Notes
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

            // Health Events
            foreach (var ev in HealthEvents)
            {
                if (ev.StartDate.Year == year && ev.StartDate.Month == month)
                {
                    var d = DaysInMonth.FirstOrDefault(x => x.DayNumber == ev.StartDate.Day);
                    d?.Entries.Add(new CalendarEntry {
                        Text     = ev.Title,
                        DotColor = null
                    });
                }
            }

            // Activity Logs
            foreach (var act in ActivityLogs)
            {
                if (act.Date.Year == year && act.Date.Month == month)
                {
                    var d = DaysInMonth.FirstOrDefault(x => x.DayNumber == act.Date.Day);
                    d?.Entries.Add(new CalendarEntry {
                        Text     = act.Name,
                        DotColor = null
                    });
                }
            }

            // Conditions (if you later add a date property to your model)
            // foreach (var cond in Conditions) { ... }

            // Notify UI
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
                    IsToday   = date.Year  == DateTime.Today.Year &&
                                date.Month == DateTime.Today.Month &&
                                d           == DateTime.Today.Day
                });
            }

            DaysInMonth  = items;
            CurrentMonth = date.ToString("MMMM yyyy");
            OnPropertyChanged(nameof(DaysInMonth));
            OnPropertyChanged(nameof(CurrentMonth));
        }

        // ── Helpers & navigation ──
        private Color GenerateColor(int idx) {
            const float φ = 0.618033988749895f;
            return Color.FromHsla((idx * φ) % 1f, 0.5f, 0.7f);
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
        public string   Id        { get; set; }
        public string   Title     { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate   { get; set; }
        public string   Duration  { get; set; }
        public int      Impact    { get; set; }
        public string   Notes     { get; set; }
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
