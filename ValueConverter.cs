using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
    public class ValueConverter<T> : ValueConverter<T, T>
    {
        public ValueConverter(Func<T, T> convertFunc) : base(convertFunc, convertFunc) { }
    }

    public class ValueConverter<TSource, TTarget> : IValueConverter
    {
        private Func<TSource, TTarget> ConvertFunc;
        private Func<TTarget, TSource> ConvertBackFunc;

        public ValueConverter(Func<TSource, TTarget> convertFunc, Func<TTarget, TSource> convertBackFunc)
        {
            ConvertFunc = convertFunc;
            ConvertBackFunc = convertBackFunc;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            ConvertFunc((TSource)value);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => ConvertBackFunc((TTarget)value);
    }
}
