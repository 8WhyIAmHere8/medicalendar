using Microsoft.Maui.Controls;
using medi1.Data;
using medi1.Data.Models;
using medi1.SplashPageComponents;
using System.Drawing.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Net;
using medi1.Services;

namespace medi1.Pages
{
    public partial class AddEntryPage : ContentPage, INotifyPropertyChanged
    {
        public AddEntryPage()
        {
            InitializeComponent();
            BindingContext = this;

            LoadActivities();
            //LoadConditions();
        }


        //Set up collection of activities from database
        public ObservableCollection<Data.Models.Activity> Activities { get; set; } = new ();
        public ObservableCollection<Data.Models.Condition> Conditions { get; set; } = new ObservableCollection<Data.Models.Condition>(UserSession.Instance.Conditions);

        
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

        // private async Task LoadConditions()
        // {
        //     try
        //     {
        //         using var dbContext = new MedicalDbContext();
        //         var conditionsFromDb = await Task.Run(() => dbContext.Conditions.ToListAsync());

        //         MainThread.BeginInvokeOnMainThread(() =>
        //         {
        //             Conditions.Clear();
        //             foreach (var condition in conditionsFromDb)
        //             {
        //                 Conditions.Add(condition);
        //             }
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"Error loading conditions: {ex.Message}");
        //     }
        // }

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
            Conditions = await UserSession.Instance.LoadConditions();
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

            //------------------ Entry Notes -----------------//
            string entryNotes = NotesEntry.Text.ToString();

            //------------------ Health Event Relation -----------//
            string healthEvent = HealthEventPicker.SelectedItem?.ToString();

            if (entryType == "Log Health Event"){
                
                //------------------- Logging Name -----------------//
                string eventName = NameEntry.Text.ToString();

                //-------------Declare start and end dates and duration -------//

                DateTime eventStartDate;
                DateTime eventEndDate;
                string eventDuration;

                // ------------- Logging Duration ------------- //
                string dateRange = DateSelecter.SelectedItem?.ToString();
                //------------ Event Impact -----------------//
                string eventImpactScore = ImpactSelecter.SelectedItem?.ToString();
                int eventImpact = int.Parse(eventImpactScore);

                //---------------- Associated Condition -------------//
                var associatedConditionSelected = AssociatedConditionSelecter.SelectedItem as medi1.Data.Models.Condition;
                string associatedCondition = associatedConditionSelected?.Id;


                if (dateRange == "Single Date" && FullDayCheck.IsChecked == true)
                {
                    eventStartDate = entryDatePicker.Date;
                    eventEndDate = entryDatePicker.Date;
                    eventDuration = "24 hours";

                } 
                else if (dateRange == "Single Date" && FullDayCheck.IsChecked == false) 
                {
                    eventStartDate = entryDatePicker.Date; 
                    eventEndDate = entryDatePicker.Date;

                    int hours = HourSpinner.Value;   // assuming your NumericSpinner has a Value property
                    int minutes = MinuteSpinner.Value;
                    double totalTime = hours + (minutes/60);
                    int totalTimeHours = (int)Math.Floor(totalTime);
                    eventDuration = totalTimeHours.ToString() + "hours";
                    //save duration to ConditionLog in database

                }
                else
                {
                    eventStartDate = StartDatePicker.Date;
                    eventEndDate = EndDatePicker.Date;
                    TimeSpan durationDays = eventEndDate - eventStartDate;
                    int totalDuration = durationDays.Days;
                    eventDuration = totalDuration.ToString() + "days";
                    //save to ConditionLog in database
                }

                //--------------INFORMATION UPLOAD ---------------------//
                var newHealthEvent = new HealthEvent
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = eventName,
                    StartDate = eventStartDate,
                    EndDate = eventEndDate,
                    Duration = eventDuration,
                    HealthRelationship = healthEvent,
                    ConditionId = associatedCondition,
                    Impact = eventImpact,
                    Notes = entryNotes
                };

                 try
                {
                    using var dbContext = new MedicalDbContext();
                    await dbContext.HealthEvent.AddAsync(newHealthEvent);
                    await dbContext.SaveChangesAsync();

                    if (healthEvent == "Related to Condition")
                    {
                        var selectedCondition = await dbContext.Conditions
                        .FirstOrDefaultAsync(c => c.Name == associatedCondition);

                        if (associatedCondition != null)
                            {
                                selectedCondition.Symptoms.Add(eventName);
                                await dbContext.SaveChangesAsync();
                            } else {
                                Console.WriteLine($"{associatedCondition} condition not found");
                            }
                    }
                    UserSession.Instance.SaveNewHealthEvent(newHealthEvent);

                    //else if (healthEvent == "Related to Menstrual Cycle"){Add to Menstrual Cycle database contrainer}
                    
                }
                catch (DbUpdateException dbEx)
                {
                    Console.WriteLine($"Database Update Error: {dbEx.Message}");
                    if (dbEx.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {dbEx.InnerException.Message}");
                    }
                    await DisplayAlert("Error", "Failed to save your health event.", "OK");
                }
                // ---------------- LOGGING RELATIONSHIP TO EXISTING HEALTH ---------------//


            } else if (entryType == "Log Activity")
            {
                //----------Activity Name---------//
                var activitySelected = ActivityNameSelecter.SelectedItem as medi1.Data.Models.Activity;
                string activityName = activitySelected?.Name;
                
                //-------------Date and Duration----------//
                DateTime activityDate = activityDatePicker.Date;
                int hours = AHourSpinner.Value;  
                int minutes = AMinuteSpinner.Value;
                int tempDuration = minutes + (hours*60);
                string activityDuration = tempDuration.ToString();

                //----------------Intensity-------------------//
                string activityIntensity = IntensitySelecter.SelectedItem?.ToString();
                
                //-----------------Aggravated Condition-----------//
                var aggedConditionSelected = ConditionSelecter.SelectedItem as medi1.Data.Models.Condition;
                string aggedConditionName = aggedConditionSelected?.Name;
                //Log name of activity in "triggers" of the names health condition


                //INFORMATION UPLOAD
                var newLog = new ActivityLog
                {
                    id = Guid.NewGuid().ToString(),
                    ActivityLogId = Guid.NewGuid().ToString(),
                    Name = activityName,
                    Intensity = activityIntensity,
                    Date = activityDate,
                    Duration = activityDuration,
                    AggravatedCondition = aggedConditionName,
                    Notes = entryNotes
                };
                try
                {
                    using var dbContext = new MedicalDbContext();
                    await dbContext.ActivityEventLog.AddAsync(newLog);
                    await dbContext.SaveChangesAsync();

                    if (AggravatedCheck.IsChecked) {
                            var selectedCondition = await dbContext.Conditions
                            .FirstOrDefaultAsync(c => c.Name == aggedConditionName);

                            if (selectedCondition != null)
                            {
                                selectedCondition.Triggers.Add(activityName);
                                await dbContext.SaveChangesAsync();
                            } else {
                                Console.WriteLine($"{aggedConditionName} condition not found");
                            }
                    }
                    UserSession.Instance.SaveNewActivityLog(newLog);
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
            if (healthEvent == "Part of a New Condition")
            {
                await Navigation.PopModalAsync();

                await System.Threading.Tasks.Task.Delay(250);

                await Shell.Current.Navigation.PushModalAsync(new ConditionsPage.AddConditionPopup());
            }
            else
            {
                await Navigation.PopModalAsync(); 
            }

        } 


        private async void OnCancelEntryClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync(); // Close the popup page without adding task
        }
    }
}

