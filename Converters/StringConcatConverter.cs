using System;
using System.Collections;
using System.Globalization;
using System.Linq;

namespace Microsoft.Maui.Controls
{
    public class StringJoinConverter : IValueConverter, IMultiValueConverter
    {
        public static readonly StringJoinConverter Instance = new StringJoinConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is IEnumerable items ? Convert(items.OfType<object>().ToArray(), targetType, parameter, culture) : value;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => string.Join(GetSeparator(parameter), values.Where(value => !string.IsNullOrEmpty(value?.ToString())));

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is string str ? str.Replace(GetSeparator(parameter), ",").Split(',') : value;

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => value is string str ? str.Replace(GetSeparator(parameter), ",").Split(',') : null;

        private string GetSeparator(object parameter) => parameter as string ?? ", ";
    }

    public class StringConcatConverter : IValueConverter, IMultiValueConverter
    {
        public string Separator { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is IEnumerable list))
            {
                return value?.ToString();
            }

            string result = string.Empty;

            foreach (object item in list)
            {
                if (item != null)
                {
                    result += item.ToString() + Separator;
                }
            }

            return result.TrimEnd(Separator.ToCharArray());
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => Convert((object)values, targetType, parameter, culture);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
