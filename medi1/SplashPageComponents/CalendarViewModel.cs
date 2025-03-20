using System;
using System.Collections.ObjectModel;

namespace medi1.SplashPageComponents
{
    public class CalendarViewModel
    {
        public ObservableCollection<DayModel> DaysInMonth { get; set; } = new();
        public string CurrentMonth { get; set; } = string.Empty; // current month not yet retrieved, so set as empty
        public string FullDateToday { get; set; } = string.Empty; // Full date string for today's date

        public CalendarViewModel()
        {
            LoadCalendar(DateTime.Now);
        }

        private void LoadCalendar(DateTime date)
        {
            CurrentMonth = date.ToString("MMMM yyyy");
            DaysInMonth.Clear();

            int days = DateTime.DaysInMonth(date.Year, date.Month);
            for (int i = 1; i <= days; i++)
            {
                DateTime currentDate = new DateTime(date.Year, date.Month, i);
                DaysInMonth.Add(new DayModel
                {
                    DayNumber = i,
                    DayOfWeek = currentDate.ToString("ddd"),
                    IsToday = (currentDate.Date == DateTime.Today)
                });
            }

            // Format and set the full date for today
            FullDateToday = DateTime.Today.ToString("dd'th' MMMM yyyy");
        }
    }

    public class DayModel
    {
        public int DayNumber { get; set; }
        public string DayOfWeek { get; set; } = string.Empty;
        public bool IsToday { get; set; }
    }
}

