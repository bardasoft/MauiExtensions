using System;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
    public class NegateBoolConverter : IValueConverter
    {
        public static readonly NegateBoolConverter Instance = new NegateBoolConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is bool b ? !b : (object)null;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Convert(value, targetType, parameter, culture);
    }
}
