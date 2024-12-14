using System;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
    public class ObjectToFalseConverter : IValueConverter
    {
        public static readonly ObjectToFalseConverter Instance = new ObjectToFalseConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => !Equals(value, parameter);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
