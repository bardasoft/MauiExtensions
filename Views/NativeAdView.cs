using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
    public class NativeAdView : AdView
    {
        public static BindableProperty HeadlineProperty = BindableProperty.Create(nameof(Headline), typeof(string), typeof(string));

        public string Headline
        {
            get => (string)GetValue(HeadlineProperty);
            set => SetValue(HeadlineProperty, value);
        }
    }
}
