using Microsoft.Maui.Controls;
using medi1.SplashPageComponents;
using medi1.Services;
using System.Diagnostics; //Import used login data

namespace medi1.Pages  
{
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
            BindingContext = new CalendarViewModel();
        }

        private void OnAddClicked(object sender, EventArgs e)
        {
            //Show new task window
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

                //clear the input field
                TaskInput.Text = string.Empty;
            }

        }
        private void OnCancelClicked(object sender, EventArgs e)
        {
            // Close the pop-up without doing anything
            AddTaskPopup.IsVisible = false;

            //clear the input field
            TaskInput.Text = string.Empty;
        }

        //Edit Confirm
        private void OnEditConfirmClicked(object sender, EventArgs e)
        {
            if (_editingTaskLabel != null)
            {
                _editingTaskLabel.Text = EditTaskInput.Text;
            }
            EditTaskPopup.IsVisible = false;
        }

        //Edit Cancel
        private void OnEditCancelClicked(object sender, EventArgs e)
        {
            EditTaskPopup.IsVisible = false;
        }

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
            Debug.WriteLine((UserSession.Instance.Id,UserSession.Instance.UserName,UserSession.Instance.Password)); // Check for data
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



    }
}
