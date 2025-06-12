using System;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
    [BindingValueConverter]
    public class FalseIfNullConverter : IValueConverter
    {
        public static readonly FalseIfNullConverter Instance = new FalseIfNullConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value != null;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
