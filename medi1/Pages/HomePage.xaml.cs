using medi1.Data;
using medi1.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using medi1.Services;
using System.Diagnostics; // Import used login data

namespace medi1.Pages
{
    public partial class HomePage : ContentPage, INotifyPropertyChanged
    {
        // EF Core context for reading stored conditions
        private readonly MedicalDbContext _dbContext = new MedicalDbContext();

        // Calendar data with notification on change
        private ObservableCollection<DayItem> _daysInMonth = new();
        public ObservableCollection<DayItem> DaysInMonth
        {
            get => _daysInMonth;
            set { _daysInMonth = value; OnPropertyChanged(); }
        }

        private DateTime _displayDate;
        private string _currentMonth;
        public string CurrentMonth
        {
            get => _currentMonth;
            set { _currentMonth = value; OnPropertyChanged(); }
        }

        public string FullDateToday {get; set; }

        // Conditions list with notification on change
        private ObservableCollection<ConditionViewModel> _conditions = new();
        public ObservableCollection<ConditionViewModel> Conditions
        {
            get => _conditions;
            set { _conditions = value; OnPropertyChanged(); }
        }

        public HomePage()
        {
            InitializeComponent();
            BindingContext = this;

            // Initialize calendar
            _displayDate = DateTime.Now.Date;
            FullDateToday = _displayDate.ToString("MMMM dd, yyyy");
            LoadMonth(_displayDate);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadConditionsFromDbAsync();
        }

        private async Task LoadConditionsFromDbAsync()
        {
            var list = await _dbContext.Conditions.ToListAsync();
            Conditions.Clear();

            for (int i = 0; i < list.Count; i++)
            {
                var entity = list[i];
                var color = GenerateColor(i);
                Conditions.Add(MapToVm(entity, color));
            }
        }

        // Generate a unique color for each condition using the golden ratio
        private Color GenerateColor(int index)
        {
            const float goldenRatioConjugate = 0.618033988749895f;
            float hue = (index * goldenRatioConjugate) % 1f;
            return Color.FromHsla(hue, 0.5f, 0.7f);
        }

        // Map entity to ViewModel with generated color
        private ConditionViewModel MapToVm(Data.Models.Condition e, Color color)
            => new ConditionViewModel
            {
                Name = e.Name,
                Description = e.Description,
                Color = color,
                IsSelected = false
            };

        // Fallback mapping using dynamic color based on current count
        private ConditionViewModel MapToVm(Data.Models.Condition e)
            => MapToVm(e, GenerateColor(Conditions.Count));

        // --- Calendar logic ---
        private void LoadMonth(DateTime date)
        {
            var items = new ObservableCollection<DayItem>();
            int offset = (int)new DateTime(date.Year, date.Month, 1).DayOfWeek;
            for (int i = 0; i < offset; i++)
                items.Add(new DayItem { DayNumber = 0, IsToday = false });

            int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            for (int i = 1; i <= daysInMonth; i++)
            {
                items.Add(new DayItem
                {
                    DayNumber = i,
                    IsToday = date.Year == DateTime.Now.Year
                              && date.Month == DateTime.Now.Month
                              && i == DateTime.Now.Day
                });
            }

            DaysInMonth = items;
            CurrentMonth = date.ToString("MMMM yyyy");
        }

        private void OnPrevMonthClicked(object sender, EventArgs e)
        {
            _displayDate = _displayDate.AddMonths(-1);
            LoadMonth(_displayDate);
        }

        private void OnNextMonthClicked(object sender, EventArgs e)
        {
            _displayDate = _displayDate.AddMonths(1);
            LoadMonth(_displayDate);
        }

        // --- Navigation ---
        private async void GoToConditions(object sender, EventArgs e)
            => await Navigation.PushAsync(new ConditionsPage.ConditionsPage());

        private async void GoToReports(object sender, EventArgs e)
            => await Navigation.PushAsync(new ReportsPage());

        // --- Condition tap handler ---
        private async void OnConditionTapped(object sender, EventArgs e)
        {
            if (sender is StackLayout sl && sl.BindingContext is ConditionViewModel cvm)
            {
                await DisplayAlert(
                    "Condition Details",
                    $"Name: {cvm.Name}\nDescription: {cvm.Description}",
                    "OK"
                );
            }
        }
    }

    public class ConditionViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsSelected { get; set; }
        public Color Color { get; set; }
    }

    public class DayItem
    {
        public int DayNumber { get; set; }
        public bool IsToday { get; set; }
    }

}

