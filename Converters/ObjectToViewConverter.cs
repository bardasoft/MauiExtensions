using System;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
    public class ObjectToViewConverter : IValueConverter
    {
        public static readonly ObjectToViewConverter Instance = new ObjectToViewConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && parameter is DataTemplateSelector selector)
            {
                return selector.SelectTemplate(value, null).CreateContent();
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}