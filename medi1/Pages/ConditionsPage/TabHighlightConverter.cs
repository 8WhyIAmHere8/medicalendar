using System.Globalization;
namespace medi1.Pages.ConditionsPage;
public class TabHighlightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var selected = value as Condition;
        var current = parameter as Condition;

        return selected == current ? Color.FromArgb("#90CAF9") : Color.FromArgb("#E3F2FD");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
