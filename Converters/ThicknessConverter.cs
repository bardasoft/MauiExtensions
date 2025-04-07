using System.Globalization;

namespace Microsoft.Maui.Controls.Extensions
{
    public class ThicknessConverter : IValueConverter<double, Thickness>
    {
        public static readonly ThicknessConverter Left = new ThicknessConverter { Property = ThicknessProperties.Left };
        public static readonly ThicknessConverter Top = new ThicknessConverter { Property = ThicknessProperties.Top };
        public static readonly ThicknessConverter Right = new ThicknessConverter { Property = ThicknessProperties.Right };
        public static readonly ThicknessConverter Bottom = new ThicknessConverter { Property = ThicknessProperties.Bottom };

        public ThicknessProperties Property { get; set; }

        public object? Convert(double value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = new Thickness();

            if (Property.HasFlag(ThicknessProperties.Left)) result.Left = value;
            if (Property.HasFlag(ThicknessProperties.Top)) result.Top = value;
            if (Property.HasFlag(ThicknessProperties.Right)) result.Right = value;
            if (Property.HasFlag(ThicknessProperties.Bottom)) result.Bottom = value;

            return result;
        }

        public object? ConvertBack(Thickness value, Type targetType, object parameter, CultureInfo culture) => Property switch
        {
            ThicknessProperties.Left => value.Left,
            ThicknessProperties.Top => value.Top,
            ThicknessProperties.Right => value.Right,
            ThicknessProperties.Bottom => value.Bottom,
            ThicknessProperties.Horizontal => value.HorizontalThickness,
            ThicknessProperties.Vertical => value.VerticalThickness,
            _ => null
        };
    }

    public static class ThicknessBindings
    {
        public static BindableProperty CreateProperty(string thicknessFieldName, Type declaringType, BindableProperty parentProperty, BindableProperty.CreateDefaultValueDelegate defaultValueCreator, string propertyName = null!) => BindableProperty.CreateAttached(propertyName ?? (parentProperty.PropertyName + "." + thicknessFieldName), typeof(double), declaringType, null, propertyChanged: (bindable, oldValue, newValue) => UpdateThicknessValue(bindable, parentProperty, thicknessFieldName, (double)newValue), defaultValueCreator: defaultValueCreator);

        private static void UpdateThicknessValue(BindableObject bindable, BindableProperty thicknessProperty, string propertyName, double value)
        {
            var thickness = (Thickness)bindable.GetValue(thicknessProperty);
            if (propertyName == nameof(Thickness.Left)) thickness.Left = value;
            else if (propertyName == nameof(Thickness.Top)) thickness.Top = value;
            else if (propertyName == nameof(Thickness.Right)) thickness.Right = value;
            else if (propertyName == nameof(Thickness.Bottom)) thickness.Bottom = value;

            bindable.SetValue(thicknessProperty, thickness);
        }

        public static void UpdateThicknessProperties(BindableObject bindable, BindableProperty thicknessProperty, BindableProperty leftProperty, BindableProperty topProperty, BindableProperty rightProperty, BindableProperty bottomProperty)
        {
            var thickness = (Thickness)bindable.GetValue(thicknessProperty);
            if (!Equals(bindable.GetValue(leftProperty), thickness.Left)) bindable.SetValue(leftProperty, thickness.Left);
            if (!Equals(bindable.GetValue(topProperty), thickness.Top)) bindable.SetValue(topProperty, thickness.Top);
            if (!Equals(bindable.GetValue(rightProperty), thickness.Right)) bindable.SetValue(rightProperty, thickness.Right);
            if (!Equals(bindable.GetValue(bottomProperty), thickness.Bottom)) bindable.SetValue(bottomProperty, thickness.Bottom);
        }
    }

    public static class Padding
    {
        public static readonly BindableProperty LeftProperty = BindableProperty.CreateAttached(nameof(GetLeft).Substring(3), typeof(double), typeof(Layout), 0);

        public static double GetLeft(this Layout layout) => (double)layout.GetValue(LeftProperty);
        public static void SetLeft(this Layout layout, double value) => layout.SetValue(LeftProperty, value);

        public static readonly BindableProperty TopProperty = BindableProperty.CreateAttached(nameof(GetTop).Substring(3), typeof(double), typeof(Layout), 0);

        public static double GetTop(this Layout layout) => (double)layout.GetValue(TopProperty);
        public static void SetTop(this Layout layout, double value) => layout.SetValue(TopProperty, value);

        public static readonly BindableProperty RightProperty = BindableProperty.CreateAttached(nameof(GetRight).Substring(3), typeof(double), typeof(Layout), 0);

        public static double GetRight(this Layout layout) => (double)layout.GetValue(RightProperty);
        public static void SetRight(this Layout layout, double value) => layout.SetValue(RightProperty, value);

        public static readonly BindableProperty BottomProperty = BindableProperty.CreateAttached(nameof(GetBottom).Substring(3), typeof(double), typeof(Layout), 0);

        public static double GetBottom(this Layout layout) => (double)layout.GetValue(BottomProperty);
        public static void SetBottom(this Layout layout, double value) => layout.SetValue(BottomProperty, value);
    }

    public static class Margin
    {
        public static readonly BindableProperty LeftProperty = CreateProperty(nameof(Thickness.Left));

        public static double GetLeft(this View view) => (double)view.GetValue(LeftProperty);
        public static void SetLeft(this View view, double value) => view.SetValue(LeftProperty, value);

        public static readonly BindableProperty TopProperty = CreateProperty(nameof(Thickness.Top));

        public static double GetTop(this View view) => (double)view.GetValue(TopProperty);
        public static void SetTop(this View view, double value) => view.SetValue(TopProperty, value);

        public static readonly BindableProperty RightProperty = CreateProperty(nameof(Thickness.Right));

        public static double GetRight(this View view) => (double)view.GetValue(RightProperty);
        public static void SetRight(this View view, double value) => view.SetValue(RightProperty, value);

        public static readonly BindableProperty BottomProperty = CreateProperty(nameof(Thickness.Bottom));

        public static double GetBottom(this View view) => (double)view.GetValue(BottomProperty);
        public static void SetBottom(this View view, double value) => view.SetValue(BottomProperty, value);

        private static BindableProperty CreateProperty(string property) => ThicknessBindings.CreateProperty(property, typeof(View), View.MarginProperty, MarginDefaultValueCreator);

        private static object MarginDefaultValueCreator(BindableObject bindable)
        {
            bindable.PropertyChanged += ThicknessPropertyChanged;
            return 0d;
        }

        private static void ThicknessPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == View.MarginProperty.PropertyName)
            {
                ThicknessBindings.UpdateThicknessProperties((BindableObject)sender!, View.MarginProperty, LeftProperty, TopProperty, RightProperty, BottomProperty);
            }
        }
    }
}

namespace Microsoft.Maui.Controls
{
    [Flags]
    public enum ThicknessProperties
    {
        Left = 1 << 0,
        Top = 1 << 1,
        Right = 1 << 2,
        Bottom = 1 << 3,
        Horizontal = Left | Right,
        Vertical = Top | Bottom,
        Uniform = Left | Top | Right | Bottom
    }

    public class ThicknessValuesExtension : EnumExtension<ThicknessProperties> { }

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
        object? Convert(TIn? value, Type targetType, TParameter? parameter, CultureInfo culture);
        object? ConvertBack(TOut? value, Type targetType, TParameter? parameter, CultureInfo culture);

        object? IValueConverter.Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => TryCast(value, out TIn? t) && TryCast(parameter, out TParameter? tp) ? Convert(t, targetType, tp, culture) : null;
        object? IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => TryCast(value, out TOut? t) && TryCast(parameter, out TParameter? tp) ? ConvertBack(t, targetType, tp, culture) : null;

        private static bool TryCast<T>(object? value, out T? result)
        {
            if (value is T t)
            {
                result = t;
                return true;
            }
            else
            {
                result = default;
                return value == null && !typeof(T).IsValueType;
            }
        }
    }

    public class ThicknessConverter : IValueConverter<double, Thickness, ThicknessProperties>, IMultiValueConverter
    {
        public static readonly ThicknessConverter Instance = new ThicknessConverter();

        public object? Convert(double value, Type targetType, ThicknessProperties parameter, CultureInfo culture)
        {
            var result = new Thickness();

            if (parameter.HasFlag(ThicknessProperties.Left))
            {
                result.Left = value;
            }
            if (parameter.HasFlag(ThicknessProperties.Right))
            {
                result.Right = value;
            }
            if (parameter.HasFlag(ThicknessProperties.Top))
            {
                result.Top = value;
            }
            if (parameter.HasFlag(ThicknessProperties.Bottom))
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

        public object? ConvertBack(Thickness value, Type targetType, ThicknessProperties parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
