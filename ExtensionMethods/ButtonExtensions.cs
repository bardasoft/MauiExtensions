using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
    public static class ButtonExtensions
    {
        public static BindableProperty IsGlyphProperty = BindableProperty.CreateAttached("IsGlyph", typeof(bool), typeof(Button), false, propertyChanged: IsGlyphPropertyChanged);

        public static bool GetIsGlyph(this Button button) => (bool)button.GetValue(IsGlyphProperty);

        public static void SetIsGlyph(this Button button, bool value) => button.SetValue(IsGlyphProperty, value);

        private static void IsGlyphPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            Button button = (Button)bindable;
            bool value = (bool)newValue;

            if (value)
            {
                FontImageSource source = new FontImageSource();
                
                source.SetBinding(FontImageSource.GlyphProperty, button, "Text");
                source.SetBinding(FontImageSource.FontFamilyProperty, button, "FontFamily");
                source.SetBinding(FontImageSource.SizeProperty, button, "FontSize");
                source.SetBinding(FontImageSource.ColorProperty, button, "TextColor");

                button.ImageSource = source;
            }
            else
            {
                button.ImageSource = null;
            }
        }
    }
}
