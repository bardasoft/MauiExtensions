using System;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
    public class ObjectToTrueConverter : IValueConverter
    {
        public static readonly ObjectToTrueConverter Instance = new ObjectToTrueConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Equals(value, parameter);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
