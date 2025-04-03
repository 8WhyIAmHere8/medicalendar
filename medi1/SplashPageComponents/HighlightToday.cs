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
                return Colors.Blue;
            }
            return Colors.Gray; //change to pallette hexadecimal
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();  // Not needed for one-way binding
        }
    }
}


