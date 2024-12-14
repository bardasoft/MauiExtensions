using System;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
    public class IsTypeConverter : IValueConverter
    {
        public static readonly IsTypeConverter Instance = new IsTypeConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value != null && Equals(value.GetType(), parameter);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GetTypeConverter : IValueConverter
    {
        public static readonly GetTypeConverter Instance = new GetTypeConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value?.GetType();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
