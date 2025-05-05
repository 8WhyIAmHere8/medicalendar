using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using medi1.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
// alias your data model so it doesn’t conflict with MAUI’s Condition
using DCondition = medi1.Data.Models.Condition;

namespace medi1.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage : ContentPage, INotifyPropertyChanged
    {
        readonly MedicalDbContext _dbContext = new();

        // ── Calendar fields ──
        public ObservableCollection<DayItem> DaysInMonth { get; set; } = new();
        private DateTime _displayDate;
        public string CurrentMonth { get; set; }
        public string FullDateToday { get; set; }

        // ── Conditions & Events ──
        public ObservableCollection<ConditionViewModel> Conditions { get; set; } = new();
        public ObservableCollection<HealthEventViewModel> HealthEvents { get; set; } = new();

        public HomePage()
        {
            InitializeComponent();
            BindingContext = this;

            _displayDate     = DateTime.Today;
            FullDateToday    = _displayDate.ToString("MMMM dd, yyyy");
            LoadMonth(_displayDate);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                await LoadConditionsFromDbAsync();
                await LoadHealthEventsFromDbAsync();
            }
            catch (Exception ex)
            {
                // show full exception so we can iterate if anything remains
                await DisplayAlert("Startup Error", ex.ToString(), "OK");
                Debug.WriteLine(ex);
            }
        }

        // ── Load Conditions ──
        async Task LoadConditionsFromDbAsync()
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

        // ── Load HealthEvents safely ──
        async Task LoadHealthEventsFromDbAsync()
        {
            HealthEvents.Clear();

            // fetch all into memory as DTOs to avoid EF re-materialization errors
            var raw = await _dbContext.HealthEvent
                                      .Select(e => new {
                                          e.Id,
                                          e.Title,
                                          e.StartDate,
                                          e.EndDate,
                                          e.Duration,
                                          e.Impact,
                                          e.Notes
                                      })
                                      .AsNoTracking()
                                      .ToListAsync();

            // now filter/map on the client, skipping any nulls or bad data
            foreach (var e in raw.AsEnumerable())
            {
                try
                {
                    if (e.StartDate == null || e.EndDate == null || e.Impact == null)
                        continue;

                    HealthEvents.Add(new HealthEventViewModel {
                        Id        = e.Id,
                        Title     = e.Title,
                        StartDate = e.StartDate.Value,
                        EndDate   = e.EndDate.Value,
                        Duration  = e.Duration,
                        Impact    = e.Impact.Value,
                        Notes     = e.Notes
                    });
                }
                catch (Exception itemEx)
                {
                    // skip bad record, but log
                    Debug.WriteLine($"Skipping bad HealthEvent {e.Id}: {itemEx}");
                }
            }
        }

        // ── Calendar logic ──
        void LoadMonth(DateTime date)
        {
            var items = new ObservableCollection<DayItem>();
            int offset = (int)new DateTime(date.Year, date.Month, 1).DayOfWeek;

            for (int i = 0; i < offset; i++)
                items.Add(new DayItem { DayNumber = 0, IsToday = false });

            int dim = DateTime.DaysInMonth(date.Year, date.Month);
            for (int d = 1; d <= dim; d++)
                items.Add(new DayItem {
                    DayNumber = d,
                    IsToday   = date.Year  == DateTime.Today.Year &&
                                date.Month == DateTime.Today.Month &&
                                d           == DateTime.Today.Day
                });

            DaysInMonth  = items;
            CurrentMonth = date.ToString("MMMM yyyy");
            OnPropertyChanged(nameof(DaysInMonth));
            OnPropertyChanged(nameof(CurrentMonth));
        }

        void OnPrevMonthClicked(object s, EventArgs e) {
            _displayDate = _displayDate.AddMonths(-1);
            LoadMonth(_displayDate);
        }
        void OnNextMonthClicked(object s, EventArgs e) {
            _displayDate = _displayDate.AddMonths( 1);
            LoadMonth(_displayDate);
        }

        // ── Navigation ──
        void GoToConditions(object s, EventArgs e) => 
            _ = Navigation.PushAsync(new ConditionsPage.ConditionsPage());
        void GoToAddEntry   (object s, EventArgs e) => 
            _ = Navigation.PushAsync(new AddEntryPage());
        void GoToReports    (object s, EventArgs e) => 
            _ = Navigation.PushAsync(new ReportsPage());

        // ── Condition tap ──
        async void OnConditionTapped(object s, EventArgs e)
        {
            if (s is StackLayout sl && sl.BindingContext is ConditionViewModel cvm)
            {
                await DisplayAlert("Condition Details",
                    $"Id: {cvm.Id}\nName: {cvm.Name}\nDesc: {cvm.Description}\n" +
                    $"Symptoms: {cvm.Symptoms}\nMeds: {cvm.Medications}\nTreatments: {cvm.Treatments}\nNotes: {cvm.Notes}",
                    "OK");
            }
        }

        // ── Helpers ──
        Color GenerateColor(int idx) {
            const float φ = 0.618033988749895f;
            return Color.FromHsla((idx * φ) % 1f, 0.5f, 0.7f);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName]string n = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    // ── View models ──
    public class ConditionViewModel {
        public string Id          { get; set; }
        public string Name        { get; set; }
        public string Description { get; set; }
        public string Symptoms    { get; set; }
        public string Medications { get; set; }
        public string Treatments  { get; set; }
        public string Notes       { get; set; }
        public Color  Color       { get; set; }
    }

    public class HealthEventViewModel {
        public string   Id        { get; set; }
        public string   Title     { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate   { get; set; }
        public string   Duration  { get; set; }
        public int      Impact    { get; set; }
        public string   Notes     { get; set; }
    }

    public class DayItem {
        public int  DayNumber { get; set; }
        public bool IsToday   { get; set; }
    }
}
