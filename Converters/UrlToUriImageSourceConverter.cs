using System;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
    public class UrlToUriImageSourceConverter : IValueConverter
    {
        public static readonly UrlToUriImageSourceConverter Instance = new UrlToUriImageSourceConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var url = value as string;

            if (url == null)// || !Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return value;
            }

            var source = new UriImageSource
            {
                Uri = new Uri(url)
            };

            if (parameter is TimeSpan validity)
            {
                source.CacheValidity = validity;
            }

            return source;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}