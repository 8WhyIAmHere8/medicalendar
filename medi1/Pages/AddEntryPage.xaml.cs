using Microsoft.Maui.Controls;
using medi1.SplashPageComponents;
using System.Drawing.Text;

namespace medi1.Pages
{
    public partial class AddEntryPage : ContentPage
    {
        public AddEntryPage()
        {
            InitializeComponent();
        }

        private void OnEntrySelected(object sender, EventArgs e)
        {
            if (EntrySelecter.SelectedItem == null)
                return;

            string entryType = EntrySelecter.SelectedItem.ToString();

            if (entryType == "Log Health Event")
            {
                EventNameContainer.IsVisible = true;
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
                EventNameContainer.IsVisible = true;
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

        private void AggravatedConfirmed(object sender, CheckedChangedEventArgs e)
        {
            //If Checkbox is checked, make condition picker visible
            whichConditionContainer.IsVisible = e.Value;
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
            //------------------- Logging Name -----------------//
            string entryName = NameEntry.Text.ToString();
            //------------------- Entry Notes -----------------------//
            string entryNotes = NotesEntry.Text.ToString();

            if (entryType == "Log Health Event"){
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


            } else if (entryType == "Log Activity")
            {
                //-------------Date and Duration----------//
                string activityDate = activityDatePicker.Date.ToString("d");
                int hours = AHourSpinner.Value;  
                int minutes = AMinuteSpinner.Value;
                int activityDuration = minutes + (hours*60);

                //----------------Intensity-------------------//
                string activityIntensity = IntensitySelecter.SelectedItem?.ToString();
                
                //-----------------Aggravated Condition-----------//
                string aggedCondition = ConditionSelecter.SelectedItem?.ToString();
                //Log name of activity in "triggers" of the names health condition

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

