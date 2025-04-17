using Microsoft.Maui.Controls;
using medi1.Data;
using medi1.Data.Models;
using medi1.SplashPageComponents;
using System.Drawing.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace medi1.Pages
{
    public partial class AddEntryPage : ContentPage, INotifyPropertyChanged
    {
        public AddEntryPage()
        {
            InitializeComponent();
            BindingContext = this;

            LoadActivities();
            LoadConditions();
        }


        //Set up collection of activities from database
        public ObservableCollection<Data.Models.Activity> Activities { get; set; } = new();
        public ObservableCollection<Data.Models.Condition> Conditions { get; set; } = new();

        
        //Load activities to bind to the picker
        private async Task LoadActivities()
        {
            try
            {
                using var dbContext = new MedicalDbContext();
                var activitiesFromDb = await Task.Run(() => dbContext.Activities.ToListAsync());

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Activities.Clear();
                    foreach (var activity in activitiesFromDb)
                    {
                        Activities.Add(activity);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading activities: {ex.Message}");
            }
        }

        private async Task LoadConditions()
        {
            try
            {
                using var dbContext = new MedicalDbContext();
                var conditionsFromDb = await Task.Run(() => dbContext.Conditions.ToListAsync());

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Conditions.Clear();
                    foreach (var condition in conditionsFromDb)
                    {
                        Conditions.Add(condition);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading conditions: {ex.Message}");
            }
        }

        private async void OnEntrySelected(object sender, EventArgs e)
        {
            if (EntrySelecter.SelectedItem == null)
                return;

            string entryType = EntrySelecter.SelectedItem.ToString();

            if (entryType == "Log Health Event")
            {
                EventNameContainer.IsVisible = true;
                ActivityNameContainer.IsVisible = false;
                DateContainer.IsVisible = true;
                HealthRelationContainer.IsVisible = true;
                ImpactSelecter.IsVisible = true;
                activityTimingContainer.IsVisible = false;
                IntensitySelecter.IsVisible = false;
                aggravationContainer.IsVisible = false; 
                NotesContainer.IsVisible = true;
                ConfirmEntryButton.IsVisible = true;
            }
            else if (entryType == "Log Activity")
            {
                ActivityNameContainer.IsVisible = true;
                EventNameContainer.IsVisible = false;
                DateContainer.IsVisible = false;
                HealthRelationContainer.IsVisible = false;
                ImpactSelecter.IsVisible = false;
                activityTimingContainer.IsVisible = true;
                IntensitySelecter.IsVisible = true;
                aggravationContainer.IsVisible = true;
                NotesContainer.IsVisible = true;
                ConfirmEntryButton.IsVisible = true;
            }
        }

        //private void OnActivitySelected(object sender, EventArgs e)
       // {
            //var selectedActivity = ActivityNameSelecter.SelectedItem as Data.Models.Activity;
       // }
                
        private void OnDateSelected(object sender, EventArgs e)
        {
            string dateRange = DateSelecter.SelectedItem?.ToString();
            
            if(dateRange == "Single Date")
            {
                singleDateContainer.IsVisible = true;
                dateRangeContainer.IsVisible = false;
            }
            else{
                dateRangeContainer.IsVisible = true;
                singleDateContainer.IsVisible = false;
            }
        }
        
        private void TemporaryEventConfirmed(object sender, CheckedChangedEventArgs e)
        {
            // If the CheckBox is checked, make the spinners visible
            DurationSpinnerContainer.IsVisible = !e.Value;

        }

        private async void AggravatedConfirmed(object sender, CheckedChangedEventArgs e)
        {
            //If Checkbox is checked, make condition picker visible
            whichConditionContainer.IsVisible = e.Value;
            await LoadConditions();
        }

        private void HealthEventPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            string healthEvent = HealthEventPicker.SelectedItem?.ToString();

            
            if (healthEvent == "Related to Condition")
            {
                AssociatedConditionSelecter.IsVisible = true;
            }
            else
            {
                AssociatedConditionSelecter.IsVisible = false;
            }
        }

        private async void OnConfirmEntryClicked(object sender, EventArgs e)
        {
            // COMMON DATA //
            //--------------- Entry Type ----------------------//
            string entryType = EntrySelecter.SelectedItem.ToString();

            if (entryType == "Log Health Event"){
                
                //------------------- Logging Name -----------------//
                string eventName = NameEntry.Text.ToString();
                // ------------- Logging Duration ------------- //
                string dateRange = DateSelecter.SelectedItem?.ToString();
                 //------------------ Health Event Relation -----------//
                string healthEvent = HealthEventPicker.SelectedItem?.ToString();
                //------------ Event Impact -----------------//
                string eventImpact = ImpactSelecter.SelectedItem?.ToString();

                if (dateRange == "Single Date" && FullDayCheck.IsChecked == true)
                {
                    string eventDate = entryDatePicker.Date.ToString("d");
                    string eventDuration = "24";

                } 
                else if (dateRange == "Single Date" && FullDayCheck.IsChecked == false) 
                {
                    string eventDate = entryDatePicker.Date.ToString("d");

                    int hours = HourSpinner.Value;   // assuming your NumericSpinner has a Value property
                    int minutes = MinuteSpinner.Value;
                    double totalTime = hours + (minutes/60);
                    int totalTimeHours = (int)Math.Floor(totalTime);
                    string eventDuration = totalTimeHours.ToString();
                    //save duration to ConditionLog in database

                }
                else
                {
                    DateTime eventStartDate = StartDatePicker.Date;
                    DateTime eventEndDate = EndDatePicker.Date;
                    TimeSpan durationDays = eventEndDate - eventStartDate;
                    int totalDuration = durationDays.Days;
                    string eventDurationDays = totalDuration.ToString();
                    //save to ConditionLog in database
                }

                // ---------------- LOGGING RELATIONSHIP TO EXISTING HEALTH ---------------//

                if (healthEvent == "Independent Health Evemt")
                {
                    //Add to database container ConditionLogs
                }
                else if (healthEvent == "Related to Condition")
                {
                    AssociatedConditionSelecter.IsVisible = true;
                    //Once condition selected and entry confirmed, entry is added to that Condition in the database
                }
                else if (healthEvent == "Related to Menstrual Cycle")
                {
                    //Add to Menstrual Cycle database contrainer
                }
                else if (healthEvent == "Part of a New Condition")
                {
                    //Open new condition page
                }

                string entryNotes = NotesEntry.Text.ToString();

            } else if (entryType == "Log Activity")
            {
                //----------Activity Name---------//
                string activityName = ActivityNameSelecter.SelectedItem?.ToString();
                
                //-------------Date and Duration----------//
                DateTime activityDate = activityDatePicker.Date;
                int hours = AHourSpinner.Value;  
                int minutes = AMinuteSpinner.Value;
                int tempDuration = minutes + (hours*60);
                string activityDuration = tempDuration.ToString();

                //----------------Intensity-------------------//
                string activityIntensity = IntensitySelecter.SelectedItem?.ToString();
                
                //-----------------Aggravated Condition-----------//
                string aggedCondition = ConditionSelecter.SelectedItem?.ToString();
                //Log name of activity in "triggers" of the names health condition

                string entryNotes = NotesEntry.Text.ToString();

                //INFORMATION UPLOAD
                var newLog = new ActivityLog
                {
                    id = Guid.NewGuid().ToString(),
                    ActivityLogId = Guid.NewGuid().ToString(),
                    Name = activityName,
                    Intensity = activityIntensity,
                    Date = activityDate,
                    Duration = activityDuration,
                    AggravatedCondition = aggedCondition,
                    Notes = entryNotes
                };
                try
                {
                    using var dbContext = new MedicalDbContext();
                    await dbContext.ActivityEventLog.AddAsync(newLog);
                    await dbContext.SaveChangesAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    Console.WriteLine($"Database Update Error: {dbEx.Message}");
                    if (dbEx.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {dbEx.InnerException.Message}");
                    }
                    await DisplayAlert("Error", "Failed to save your activity log.", "OK");
                }

            }
            // Handle task addition logic here
                await Navigation.PopModalAsync(); // Close the popup page
        } 


        private async void OnCancelEntryClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync(); // Close the popup page without adding task
        }
    }
}

