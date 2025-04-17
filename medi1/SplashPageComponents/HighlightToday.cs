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
            if (value is bool isToday && isToday) 
            {
                // Highlight today with primary palette color
                return Color.FromArgb("#007AFF"); 
            }
            // Non-today dates use light gray
            return Color.FromArgb("#D1D5DB"); 
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // One-way binding only
            throw new NotImplementedException();
        }
    }
}
