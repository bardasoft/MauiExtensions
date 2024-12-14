using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
    public class EnumerableConcatenator : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            IList list = new List<object>();

            foreach (object value in values)
            {
                if (value is IEnumerable enumerable)
                {
                    foreach (object part in enumerable)
                    {
                        list.Add(part);
                    }
                }
                else
                {
                    list.Add(value);
                }
            }

            return list;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => null;
    }
}
