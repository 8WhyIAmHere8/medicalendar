using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel; // For INotifyPropertyChanged
using System.Runtime.CompilerServices;
using System.Linq;
using medi1.SplashPageComponents;

namespace medi1.Pages
{
    public partial class HomePage : ContentPage, INotifyPropertyChanged
    {
        // Backing field for the displayed month
        private DateTime _displayDate;

        private string _currentMonth;
        public string CurrentMonth
        {
            get => _currentMonth;
            set { _currentMonth = value; OnPropertyChanged(); }
        }

        public string FullDateToday { get; set; }

        // Backing field + full property for DaysInMonth
        private ObservableCollection<DayItem> _daysInMonth = new();
        public ObservableCollection<DayItem> DaysInMonth
        {
            get => _daysInMonth;
            set
            {
                _daysInMonth = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Condition> Conditions { get; set; }

        public HomePage()
        {
            InitializeComponent();

            _displayDate = DateTime.Now.Date;
            FullDateToday = _displayDate.ToString("MMMM dd, yyyy");

            // Initialize via property so UI sees it
            DaysInMonth = new ObservableCollection<DayItem>();

            // Start with an empty list of conditions
            Conditions = new ObservableCollection<Condition>();

            BindingContext = this;

            LoadMonth(_displayDate);

            MessagingCenter.Subscribe<Condition>(this, "ConditionAdded", (newCondition) =>
            {
                Conditions.Add(newCondition);
            });
        }

        // Load days for the given month by replacing the entire collection
        private void LoadMonth(DateTime date)
        {
            var items = new ObservableCollection<DayItem>();

            int offset = (int)new DateTime(date.Year, date.Month, 1).DayOfWeek;
            for (int i = 0; i < offset; i++)
            {
                items.Add(new DayItem { DayNumber = 0, IsToday = false });
            }

            int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            for (int i = 1; i <= daysInMonth; i++)
            {
                items.Add(new DayItem
                {
                    DayNumber = i,
                    IsToday = (date.Year == DateTime.Now.Year && date.Month == DateTime.Now.Month && i == DateTime.Now.Day)
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

        public new event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnAddClicked(object sender, EventArgs e)
        {
            AddTaskPopup.IsVisible = true;
        }

        private Label _editingTaskLabel;

        private void OnConfirmClicked(object sender, EventArgs e)
        {
            string taskText = TaskInput.Text?.Trim();
            if (!string.IsNullOrEmpty(taskText))
            {
                var taskItemLayout = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Spacing = 10
                };

                // Tick/Untick checkbox for task
                var taskCheckBox = new CheckBox();
                var taskLabel = new Label
                {
                    Text = taskText,
                    HorizontalOptions = LayoutOptions.StartAndExpand,
                    VerticalOptions = LayoutOptions.Center
                };
                taskCheckBox.CheckedChanged += (s, args) =>
                {
                    taskLabel.TextDecorations = taskCheckBox.IsChecked
                        ? TextDecorations.Strikethrough
                        : TextDecorations.None;
                };

                var editButton = new Button
                {
                    Text = "✏️",
                    FontSize = 12,
                    BackgroundColor = Colors.Transparent,
                    WidthRequest = 40
                };
                editButton.Clicked += (s, args) =>
                {
                    _editingTaskLabel = taskLabel;
                    EditTaskInput.Text = taskLabel.Text;
                    EditTaskPopup.IsVisible = true;
                };

                var deleteButton = new Button
                {
                    Text = "🗑️",
                    FontSize = 12,
                    BackgroundColor = Colors.Transparent,
                    WidthRequest = 40
                };
                deleteButton.Clicked += (s, args) =>
                {
                    TaskListContainer.Children.Remove(taskItemLayout);
                };

                taskItemLayout.Children.Add(taskCheckBox);
                taskItemLayout.Children.Add(taskLabel);
                taskItemLayout.Children.Add(editButton);
                taskItemLayout.Children.Add(deleteButton);

                TaskListContainer.Children.Add(taskItemLayout);

                AddTaskPopup.IsVisible = false;
                TaskInput.Text = string.Empty;
            }
        }

        private void OnCancelClicked(object sender, EventArgs e)
        {
            AddTaskPopup.IsVisible = false;
            TaskInput.Text = string.Empty;
        }

        private void OnEditConfirmClicked(object sender, EventArgs e)
        {
            if (_editingTaskLabel != null)
                _editingTaskLabel.Text = EditTaskInput.Text;
            EditTaskPopup.IsVisible = false;
        }

        private void OnEditCancelClicked(object sender, EventArgs e)
        {
            EditTaskPopup.IsVisible = false;
        }

        private async void GoToConditions(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ConditionsPage());
        }

        private async void GoToAddEntry(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddEntryPage());
        }

        private async void GoToReports(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ReportsPage());
        }

        private void OnConditionCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var selectedConditions = Conditions
                .Where(c => c.IsSelected)
                .Select(c => c.Name)
                .ToList();

            DisplayAlert("Selected Conditions", string.Join(", ", selectedConditions), "OK");
            UpdateCalendar(selectedConditions);
        }

        private void UpdateCalendar(System.Collections.Generic.List<string> selectedConditions)
        {
            Title = selectedConditions.Any()
                ? $"Dashboard - Filter: {string.Join(", ", selectedConditions)}"
                : "Dashboard";
        }

        private async void OnConditionTapped(object sender, EventArgs e)
        {
            if (sender is StackLayout layout && layout.BindingContext is Condition condition)
            {
                string message = $"Name: {condition.Name}\nDescription: {condition.Description}";
                await DisplayAlert("Condition Details", message, "OK");
            }
        }
    }

    public class Condition
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
