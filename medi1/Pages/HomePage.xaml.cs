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

        // For editing tasks
        private Label _editingTaskLabel;

        public HomePage()
        {
            InitializeComponent();
            BindingContext = this;

            // Initialize calendar
            _displayDate = DateTime.Now.Date;
            FullDateToday = _displayDate.ToString("MMMM dd, yyyy");
            LoadMonth(_displayDate);

            // Subscribe to new conditions from AddEntryPage
            MessagingCenter.Subscribe<AddEntryPage, ConditionViewModel>(
                this,
                "ConditionAdded",
                (sender, vm) => Conditions.Add(vm)
            );

            // Subscribe to new conditions from ConditionsPage (if used)
            MessagingCenter.Subscribe<ConditionsPage.ConditionsPage, Data.Models.Condition>(
                this,
                "ConditionAdded",
                (sender, entity) => Conditions.Add(MapToVm(entity))
            );
        }

        private async void AddNewEntry(object sender, EventArgs e)
            => await Navigation.PushModalAsync(new AddEntryPage());

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadConditionsFromDbAsync();
            await LoadIncompleteTasksAsync();
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

        // --- Task UI logic ---
        
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

        #region INotifyPropertyChanged
        public new event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion
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

