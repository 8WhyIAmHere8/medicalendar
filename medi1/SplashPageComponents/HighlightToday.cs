using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Globalization;

namespace medi1.SplashPageComponents 
{
    public class HighlightToday : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isToday = value is bool today && today;
            // Determine the current theme: prefer user preference, fallback to system
            AppTheme theme = Application.Current.UserAppTheme != AppTheme.Unspecified
                ? Application.Current.UserAppTheme
                : Application.Current.RequestedTheme;

            if (isToday)
            {
                // Highlight today with primary palette color in both themes
                return Color.FromArgb("#007AFF");
            }
            else
            {
                // Non-today dates: light grey in light mode, custom grey in dark mode
                return theme == AppTheme.Dark
                    ? Color.FromArgb("#4B2E53")  // dark-mode grey
                    : Color.FromArgb("#eff1f4");  // light-mode grey
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // One-way binding only
            throw new NotImplementedException();
        }
    }
}
