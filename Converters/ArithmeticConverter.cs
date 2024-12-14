using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
    public class ArithmeticConverter : IValueConverter
    {
        public static readonly ArithmeticConverter Instance = new ArithmeticConverter();

        public static readonly ArithmeticConverter ADD = new ArithmeticConverter { Operator = ArithmeticMultiConverter.Operations.Add };
        public static readonly ArithmeticConverter SUBTRACT = new ArithmeticConverter { Operator = ArithmeticMultiConverter.Operations.Subtract };
        public static readonly ArithmeticConverter MULTIPLY = new ArithmeticConverter { Operator = ArithmeticMultiConverter.Operations.Multiply };
        public static readonly ArithmeticConverter DIVIDE = new ArithmeticConverter { Operator = ArithmeticMultiConverter.Operations.Divide };
        public static readonly ArithmeticConverter EXPONENT = new ArithmeticConverter { Operator = ArithmeticMultiConverter.Operations.Exponent };

        public ArithmeticMultiConverter.Operations Operator { get; set; } = ArithmeticMultiConverter.Operations.Add;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                if (parameter is Func<double, double> math)
                {
                    return math(d);
                }
                else if (parameter is double other || double.TryParse(parameter.ToString(), out other))
                {
                    if (ArithmeticMultiConverter.TryOperate(d, Operator, other, out var result))
                    {
                        return result;
                    }
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ArithmeticMultiConverter : IMultiValueConverter
    {
        public enum Operations { Add, Subtract, Multiply, Divide, Exponent }
        public Operations Operation { get; set; }

        public static bool TryOperate(double x, Operations operation, double y, out double result)
        {
            if (operation == Operations.Add)
            {
                result = x + y;
            }
            else if (operation == Operations.Subtract)
            {
                result = x - y;
            }
            else if (operation == Operations.Multiply)
            {
                result = x * y;
            }
            else if (operation == Operations.Divide)
            {
                result = x / y;
            }
            else if (operation == Operations.Exponent)
            {
                result = Math.Pow(x, y);
            }
            else
            {
                result = default;
                return false;
            }

            return true;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            IEnumerable<double> Filter(IEnumerable enumerable)
            {
                foreach (object o in enumerable)
                {
                    if (o is double d)
                    {
                        yield return d;
                    }
                }
            }

            IEnumerator<double> itr = Filter(values).GetEnumerator();

            if (!itr.MoveNext())
            {
                return default(double);
            }

            double result = itr.Current;

            while (itr.MoveNext())
            {
                TryOperate(result, Operation, itr.Current, out result);
            }

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
