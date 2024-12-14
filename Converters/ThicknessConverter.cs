using Microsoft.Maui.Layouts;
using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
    [Flags]
    public enum ThicknessValues
    {
        Left = 1 << 0,
        Top = 1 << 1,
        Right = 1 << 2,
        Bottom = 1 << 3,
        Horizontal = Left | Right,
        Vertical = Top | Bottom,
        Uniform = Left | Top | Right | Bottom
    }

    public class ThicknessValuesExtension : EnumExtension<ThicknessValues> { }

    [ContentProperty(nameof(Value))]
    public class EnumExtension<T> : IMarkupExtension<T> where T : Enum
    {
        public T? Value { get; set; }

        public T ProvideValue(IServiceProvider serviceProvider) => Value!;

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
    }

    public interface IValueConverter<TIn, TOut> : IValueConverter<TIn, TOut, object> { }
    public interface IValueConverter<TIn, TOut, TParameter> : IValueConverter
    {
        object? Convert(TIn value, Type targetType, TParameter parameter, CultureInfo culture);
        object? ConvertBack(TOut value, Type targetType, TParameter parameter, CultureInfo culture);

        object? IValueConverter.Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is TIn t && parameter is TParameter tp ? Convert(t, targetType, tp, culture) : null;
        object? IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value is TOut t && parameter is TParameter tp ? ConvertBack(t, targetType, tp, culture) : null;
    }

    public class ThicknessConverter : IValueConverter<double, Thickness, ThicknessValues>, IMultiValueConverter
    {
        public static readonly ThicknessConverter Instance = new ThicknessConverter();

        public object? Convert(double value, Type targetType, ThicknessValues parameter, CultureInfo culture)
        {
            var result = new Thickness();

            if (parameter.HasFlag(ThicknessValues.Left))
            {
                result.Left = value;
            }
            if (parameter.HasFlag(ThicknessValues.Right))
            {
                result.Right = value;
            }
            if (parameter.HasFlag(ThicknessValues.Top))
            {
                result.Top = value;
            }
            if (parameter.HasFlag(ThicknessValues.Bottom))
            {
                result.Bottom = value;
            }

            return result;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string[] properties = ((string)parameter).Split(',');

            Thickness thickness = new Thickness();

            for (int i = 0; i < properties.Length; i++)
            {
                string property = properties[i].Trim().ToLower();
                int index = i >= values.Length && values.Length == 1 ? 0 : i;

                if (values[index] == null)
                {
                    return BindableProperty.UnsetValue;
                }

                double value = (double)(dynamic)values[index];

                if (property == "left" || property == "horizontal" || property == "uniform")
                {
                    thickness.Left = value;
                }
                if (property == "right" || property == "horizontal" || property == "uniform")
                {
                    thickness.Right = value;
                }
                if (property == "top" || property == "vertical" || property == "uniform")
                {
                    thickness.Top = value;
                }
                if (property == "bottom" || property == "vertical" || property == "uniform")
                {
                    thickness.Bottom = value;
                }
            }

            return thickness;
        }

        public object? ConvertBack(Thickness value, Type targetType, ThicknessValues parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
