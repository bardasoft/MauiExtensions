using System;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
    public class NullIfEmptyConverter : IValueConverter
    {
        public static readonly NullIfEmptyConverter Instance = new NullIfEmptyConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is string str && str.Length == 0 ? null : value;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
