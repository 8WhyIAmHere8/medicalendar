// ViewModel/AddEntryPageViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using medi1.Data;
using medi1.Data.Models;
using medi1.Pages;
using medi1.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace medi1.Pages.AddEntryPageFolder
{
    public partial class AddEntryViewModel : ObservableObject
    {
        private readonly MedicalDbContext _dbContext;

        public AddEntryViewModel(MedicalDbContext dbContext = null)
        {
            _dbContext = dbContext ?? new MedicalDbContext(); // Use passed mock or create real one
            LoadActivitiesCommand.Execute(null);
            Conditions = new ObservableCollection<medi1.Data.Models.Condition>(UserSession.Instance.Conditions ?? new List<medi1.Data.Models.Condition>());
        }


        [ObservableProperty] ObservableCollection<medi1.Data.Models.Activity> activities = new();
        [ObservableProperty] ObservableCollection<medi1.Data.Models.Condition> conditions;

        [ObservableProperty] string selectedEntryType;
        [ObservableProperty] string eventName;
        [ObservableProperty] string entryNotes;
        [ObservableProperty] string selectedHealthRelation;
        [ObservableProperty] medi1.Data.Models.Condition selectedAssociatedCondition;
        [ObservableProperty] string selectedDateRange;

        [ObservableProperty] DateTime singleDate = DateTime.Today;
        [ObservableProperty] DateTime startDate = DateTime.Today;
        [ObservableProperty] DateTime endDate = DateTime.Today;
        [ObservableProperty] bool isFullDay;


        [ObservableProperty] int hourDuration;
        [ObservableProperty] int minuteDuration;
        [ObservableProperty] string selectedImpact;
        [ObservableProperty] medi1.Data.Models.Activity selectedActivity;
        [ObservableProperty] string selectedIntensity;
        [ObservableProperty] DateTime activityDate = DateTime.Today;
        [ObservableProperty] int activityHourDuration;
        [ObservableProperty] int activityMinuteDuration;
        [ObservableProperty] bool aggravated;
        [ObservableProperty] medi1.Data.Models.Condition selectedAggravatedCondition;

        [ObservableProperty] bool isHealthEventFormVisible;
        [ObservableProperty] bool isActivityFormVisible;
        [ObservableProperty] bool isDateSectionVisible = false;
        [ObservableProperty] bool isNotesSectionVisible = false;


        public bool ShowDurationSpinners =>
            SelectedDateRange == "Single Date" && !IsFullDay;

        
        partial void OnSelectedDateRangeChanged(string value)
        {
            OnPropertyChanged(nameof(ShowDurationSpinners));
        }
                partial void OnIsFullDayChanged(bool value)
        {
            OnPropertyChanged(nameof(ShowDurationSpinners));
        }

        public ObservableCollection<string> HealthRelationOptions { get; } = new()
        {
            "Independent Health Event",
            "Related to Condition",
            "Part of a New Condition"
        };


        public bool ShowAssociatedCondition => SelectedHealthRelation == "Related to Condition";
        partial void OnSelectedHealthRelationChanged(string value)
        {
            OnPropertyChanged(nameof(ShowAssociatedCondition));
        }

        public bool ShowAggravatedConditionPicker => Aggravated;

        partial void OnAggravatedChanged(bool value)
        {
            OnPropertyChanged(nameof(ShowAggravatedConditionPicker));
        }




        public ObservableCollection<string> EntryTypes { get; } = new()
        {
            "Log Health Event",
            "Log Activity"
        };

        partial void OnSelectedEntryTypeChanged(string value)
        {
            IsHealthEventFormVisible = value == "Log Health Event";
            IsActivityFormVisible = value == "Log Activity";
            IsDateSectionVisible = true;
            IsNotesSectionVisible = true;
        }

        [RelayCommand]
        async Task LoadActivities()
        {
            try
            {
                var list = await _dbContext.Activities.ToListAsync();
                Activities.Clear();
                foreach (var item in list)
                    Activities.Add(item);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading activities: {ex.Message}");
            }
        }

        [RelayCommand]
        async Task ConfirmEntry()
        {
            if (SelectedEntryType == "Log Health Event")
            {
                if (string.IsNullOrEmpty(EventName))
                {
                    await Shell.Current.DisplayAlert("Missing Name", "Please complete the event name", "OK");
                    return;
                }

                if (string.IsNullOrEmpty(SelectedImpact))
                {
                    await Shell.Current.DisplayAlert("Missing Impact Score", "Please select an impact score", "OK");
                    return;
                }

                DateTime start, end;
                string durationStr;

                if (SelectedDateRange == "Single Date")
                {
                    start = end = SingleDate;
                    if (IsFullDay)
                        durationStr = "24 hours";
                    else
                    {
                        double totalHours = HourDuration + (MinuteDuration / 60.0);
                        durationStr = $"{totalHours:F1} hours";
                    }
                }
                else
                {
                    start = StartDate;
                    end = EndDate;
                    durationStr = (end - start).Days + " days";
                }

                var healthEvent = new HealthEvent
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = EventName,
                    StartDate = start,
                    EndDate = end,
                    Duration = durationStr,
                    HealthRelationship = SelectedHealthRelation ?? "",
                    ConditionId = SelectedAssociatedCondition?.Id,
                    Impact = int.Parse(SelectedImpact),
                    Notes = EntryNotes
                };

                try
                {
                    await _dbContext.HealthEvent.AddAsync(healthEvent);
                    await _dbContext.SaveChangesAsync();

                    if (SelectedHealthRelation == "Related to Condition")
                    {
                        var cond = await _dbContext.Conditions.FindAsync(healthEvent.ConditionId);
                        if (cond != null)
                        {
                            cond.Symptoms.Add(EventName);
                            await _dbContext.SaveChangesAsync();
                        }
                    }

                    UserSession.Instance.SaveNewHealthEvent(healthEvent);
                    await PostSaveNavigation();

                    if (SelectedHealthRelation == "Part of a New Condition")
                        await Shell.Current.Navigation.PushModalAsync(new ConditionsPage.AddConditionPopup());
                    else
                        await Shell.Current.Navigation.PopModalAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error saving event: {ex.Message}");
                    await ShowError();
                }

            }
            else if (SelectedEntryType == "Log Activity")
            {
                if (SelectedActivity == null)
                {
                    await Shell.Current.DisplayAlert("Missing Activity", "Please select an activity.", "OK");
                    return;
                }

                if (string.IsNullOrEmpty(SelectedIntensity))
                {
                    await Shell.Current.DisplayAlert("Missing Intensity", "Please select an intensity.", "OK");
                    return;
                }

                var totalDuration = ActivityHourDuration * 60 + ActivityMinuteDuration;

                var activityLog = new ActivityLog
                {
                    id = Guid.NewGuid().ToString(),
                    ActivityLogId = Guid.NewGuid().ToString(),
                    Name = SelectedActivity.Name,
                    Intensity = SelectedIntensity,
                    Date = ActivityDate,
                    Duration = totalDuration.ToString(),
                    AggravatedCondition = SelectedAggravatedCondition?.Id,
                    Notes = EntryNotes
                };

                try
                {
                    await _dbContext.ActivityEventLog.AddAsync(activityLog);
                    await _dbContext.SaveChangesAsync();

                    if (Aggravated && SelectedAggravatedCondition != null)
                    {
                        var cond = await _dbContext.Conditions.FindAsync(SelectedAggravatedCondition.Id);
                        if (cond != null)
                        {
                            cond.Triggers.Add(SelectedActivity.Name);
                            await _dbContext.SaveChangesAsync();
                        }
                    }

                    UserSession.Instance.SaveNewActivityLog(activityLog);
                    await PostSaveNavigation();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error saving activity: {ex.Message}");
                    await ShowError();
                }
            }
        }
        protected virtual async Task PostSaveNavigation()
        {
            if (SelectedHealthRelation == "Part of a New Condition")
                await Shell.Current.Navigation.PushModalAsync(new ConditionsPage.AddConditionPopup());
            else
                await Shell.Current.Navigation.PopModalAsync();
        }
        protected virtual async Task ShowError()
        {
            await Shell.Current.DisplayAlert("Error", "Failed to save health event.", "OK");
        }

        [RelayCommand]
        async Task CancelEntry()
        {
            await Shell.Current.Navigation.PopModalAsync();
        }
    }
}

