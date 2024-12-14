using System;
using System.Collections;
using System.Globalization;
using System.Linq;

namespace Microsoft.Maui.Controls
{
    public class ReverseListConverter : IValueConverter
    {
        public static readonly ReverseListConverter Instance = new ReverseListConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (value is IEnumerable items) ? items.OfType<object>().Reverse() : value;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Convert(value, targetType, parameter, culture);
    }
}