using System;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
    public class BoolToObjectConverter : IValueConverter
    {
        public object TrueObject { set; get; }

        public object FalseObject { set; get; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (parameter is Predicate<object> predicate ? predicate(value) : Equals(value, parameter ?? true)) ? TrueObject : FalseObject;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value.Equals(TrueObject);
    }
}
