using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
    public class FuncConverter : IValueConverter
    {
        public static readonly FuncConverter Instance = new FuncConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => parameter is Func<object, object> func ? func(value) : value;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
