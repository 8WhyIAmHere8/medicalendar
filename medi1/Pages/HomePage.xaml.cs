using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using medi1.SplashPageComponents;

namespace medi1.Pages
{
    public partial class HomePage : ContentPage
    {
        // Calendar properties
        public string CurrentMonth { get; set; }
        public string FullDateToday { get; set; }
        public ObservableCollection<DayItem> DaysInMonth { get; set; }

        // Collection of conditions for filtering
        public ObservableCollection<Condition> Conditions { get; set; }

        public HomePage()
        {
            InitializeComponent();

            // Dynamically set calendar properties to show today's date.
            DateTime today = DateTime.Now;
            CurrentMonth = today.ToString("MMMM yyyy");
            FullDateToday = today.ToString("MMMM dd, yyyy");
            DaysInMonth = new ObservableCollection<DayItem>();

            int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
            for (int i = 1; i <= daysInMonth; i++)
            {
                DaysInMonth.Add(new DayItem { DayNumber = i, IsToday = (i == today.Day) });
            }

            // Initialize conditions collection
            Conditions = new ObservableCollection<Condition>
            {
                new Condition { Name = "Diabetes", IsSelected = false },
                new Condition { Name = "Hypertension", IsSelected = false },
                new Condition { Name = "Asthma", IsSelected = false }
                // Add more conditions as needed
            };

            // Set the BindingContext to this page instance so that XAML bindings work.
            BindingContext = this;
        }

        // Existing task functionality

        private void OnAddClicked(object sender, EventArgs e)
        {
            // Show new task window
            AddTaskPopup.IsVisible = true;
        }

        private Label? _editingTaskLabel;

        private void OnConfirmClicked(object sender, EventArgs e)
        {
            string taskText = TaskInput.Text?.Trim();

            if (!string.IsNullOrEmpty(taskText))
            {
                // Create a horizontal StackLayout for the task item
                var taskItemLayout = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Spacing = 10
                };

                // Checkbox for task completion
                var taskCheckBox = new CheckBox();
                var taskLabel = new Label
                {
                    Text = taskText,
                    HorizontalOptions = LayoutOptions.StartAndExpand,
                    VerticalOptions = LayoutOptions.Center
                };

                // When checkbox is checked, strike through text
                taskCheckBox.CheckedChanged += (s, args) =>
                {
                    taskLabel.TextDecorations = taskCheckBox.IsChecked ? TextDecorations.Strikethrough : TextDecorations.None;
                };

                // Edit button
                var editButton = new Button
                {
                    Text = "✏️",
                    FontSize = 12,
                    BackgroundColor = Colors.Transparent,
                    WidthRequest = 40
                };
                editButton.Clicked += (s, args) =>
                {
                    _editingTaskLabel = taskLabel;  // Store reference to the label being edited
                    EditTaskInput.Text = taskLabel.Text;
                    EditTaskPopup.IsVisible = true;  // Show edit popup
                };

                // Delete button
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

                // Add elements to the task item layout
                taskItemLayout.Children.Add(taskCheckBox);
                taskItemLayout.Children.Add(taskLabel);
                taskItemLayout.Children.Add(editButton);
                taskItemLayout.Children.Add(deleteButton);

                // Add the task to the TaskListContainer
                TaskListContainer.Children.Add(taskItemLayout);

                // Close the pop-up 
                AddTaskPopup.IsVisible = false;

                // Clear the input field
                TaskInput.Text = string.Empty;
            }
        }

        private void OnCancelClicked(object sender, EventArgs e)
        {
            // Close the pop-up without doing anything
            AddTaskPopup.IsVisible = false;
            // Clear the input field
            TaskInput.Text = string.Empty;
        }

        // Edit Confirm
        private void OnEditConfirmClicked(object sender, EventArgs e)
        {
            if (_editingTaskLabel != null)
            {
                _editingTaskLabel.Text = EditTaskInput.Text;
            }
            EditTaskPopup.IsVisible = false;
        }

        // Edit Cancel
        private void OnEditCancelClicked(object sender, EventArgs e)
        {
            EditTaskPopup.IsVisible = false;
        }

        // Navigation functions

        private async void GoToConditions(object sender, EventArgs e)
        {
            try
            {
                var conditionsPage = new ConditionsPage();
                await Navigation.PushAsync(conditionsPage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Navigation error: " + ex.Message);
            }
        }

        private async void GoToAddEntry(object sender, EventArgs e)
        {
            try
            {
                var addEntryPage = new AddEntryPage();
                await Navigation.PushAsync(addEntryPage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Navigation error: " + ex.Message);
            }
        }

        private async void GoToCalendar(object sender, EventArgs e)
        {
            try
            {
                var calendarPage = new CalendarPage();
                await Navigation.PushAsync(calendarPage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Navigation error: " + ex.Message);
            }
        }

        // New: Event handler for condition CheckBox changes
        private void OnConditionCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            // Build a list of selected condition names
            List<string> selectedConditions = Conditions
                .Where(c => c.IsSelected)
                .Select(c => c.Name)
                .ToList();

            // Display selected conditions for debugging/demo purposes
            DisplayAlert("Selected Conditions", string.Join(", ", selectedConditions), "OK");

            // Update the calendar view based on the selected conditions
            UpdateCalendar(selectedConditions);
        }

        // Placeholder function to update the calendar view based on conditions.
        private void UpdateCalendar(List<string> selectedConditions)
        {
            // For demonstration, update the page title.
            if (selectedConditions.Count > 0)
            {
                Title = $"Dashboard - Filter: {string.Join(", ", selectedConditions)}";
            }
            else
            {
                Title = "Dashboard";
            }
            System.Diagnostics.Debug.WriteLine("Filtering calendar by: " + string.Join(", ", selectedConditions));
        }
    }

    // Simple model to represent a medical condition.
    public class Condition
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }

    // Simple model to represent a day item in the calendar.
    public class DayItem
    {
        public int DayNumber { get; set; }
        public bool IsToday { get; set; }
    }
}
