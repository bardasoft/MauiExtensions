using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Controls
{
    public class EmbeddedVideoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => new HtmlWebViewSource
        {
            Html = "<html><body style=\"margin:0; padding:0\"><iframe width=100% height=100% src=" + (value?.ToString() ?? string.Empty) + " frameborder=\"0\" allowfullscreen></iframe></body></html>"
        };

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
