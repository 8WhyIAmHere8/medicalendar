using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;  // For Color support.
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

            // Set today's date
            DateTime today = DateTime.Now;
            CurrentMonth = today.ToString("MMMM yyyy");
            FullDateToday = today.ToString("MMMM dd, yyyy");
            DaysInMonth = new ObservableCollection<DayItem>();

            int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
            for (int i = 1; i <= daysInMonth; i++)
            {
                DaysInMonth.Add(new DayItem { DayNumber = i, IsToday = (i == today.Day) });
            }

            // Initialize conditions collection with default entries
            Conditions = new ObservableCollection<Condition>
            {
                new Condition { Name = "Diabetes", Description = "Type II Diabetes", IsSelected = false, Color = Colors.Orange },
                new Condition { Name = "Hypertension", Description = "High blood pressure", IsSelected = false, Color = Colors.Red },
                new Condition { Name = "Asthma", Description = "Chronic inflammatory disease of the airways", IsSelected = false, Color = Colors.Blue }
            };

            // Set the BindingContext so that XAML bindings work
            BindingContext = this;

            // Subscribe for new condition entries from AddEntryPage
            MessagingCenter.Subscribe<Condition>(this, "ConditionAdded", (newCondition) =>
            {
                Conditions.Add(newCondition);
            });
        }

        // Task section event handlers
        private void OnAddClicked(object sender, EventArgs e)
        {
            // Show the new task pop-up.
            AddTaskPopup.IsVisible = true;
        }

        private Label _editingTaskLabel;

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

                // Strike through text when the checkbox is checked
                taskCheckBox.CheckedChanged += (s, args) =>
                {
                    taskLabel.TextDecorations = taskCheckBox.IsChecked ? TextDecorations.Strikethrough : TextDecorations.None;
                };

                // Edit button for the task
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

                // Delete button for the task
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

                // Add elements to the task layout
                taskItemLayout.Children.Add(taskCheckBox);
                taskItemLayout.Children.Add(taskLabel);
                taskItemLayout.Children.Add(editButton);
                taskItemLayout.Children.Add(deleteButton);

                // Add the task layout to the TaskListContainer
                TaskListContainer.Children.Add(taskItemLayout);

                // Hide the pop-up and clear the input field
                AddTaskPopup.IsVisible = false;
                TaskInput.Text = string.Empty;
            }
        }

        private void OnCancelClicked(object sender, EventArgs e)
        {
            // Hide the task entry pop-up and clear the input
            AddTaskPopup.IsVisible = false;
            TaskInput.Text = string.Empty;
        }

        private void OnEditConfirmClicked(object sender, EventArgs e)
        {
            if (_editingTaskLabel != null)
            {
                _editingTaskLabel.Text = EditTaskInput.Text;
            }
            EditTaskPopup.IsVisible = false;
        }

        private void OnEditCancelClicked(object sender, EventArgs e)
        {
            EditTaskPopup.IsVisible = false;
        }

        // Navigation event handlers
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

        private async void GoToReports(object sender, EventArgs e)
        {
            try
            {
                var reportsPage = new ReportsPage();
                await Navigation.PushAsync(reportsPage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Navigation error: " + ex.Message);
            }
        }

        // Condition event handlers
        // Called when a condition's checkbox state changes
        private void OnConditionCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            List<string> selectedConditions = Conditions
                .Where(c => c.IsSelected)
                .Select(c => c.Name)
                .ToList();

            DisplayAlert("Selected Conditions", string.Join(", ", selectedConditions), "OK");

            // Update the calendar title based on filtering
            UpdateCalendar(selectedConditions);
        }

        private void UpdateCalendar(List<string> selectedConditions)
        {
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

        // Called when a condition is tapped, showing its details in a pop-up
        private async void OnConditionTapped(object sender, EventArgs e)
        {
            if (sender is StackLayout layout && layout.BindingContext is Condition condition)
            {
                string message = $"Name: {condition.Name}\nDescription: {condition.Description}";
                await DisplayAlert("Condition Details", message, "OK");
            }
        }
    }

    // Models
    // Updated Condition model with a Color property
    public class Condition
    {
        public string Name { get; set; }
        public string Description { get; set; }  
        public bool IsSelected { get; set; }
        public Color Color { get; set; }       
    }

    // Model to represent a day in the calendar
    public class DayItem
    {
        public int DayNumber { get; set; }
        public bool IsToday { get; set; }
    }
}
