using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
    public static class LabelExtensions
    {
        public static BindableProperty AutoSizeFontProperty = BindableProperty.CreateAttached("AutoSizeFont", typeof(bool), typeof(Label), false, propertyChanged: HandleAutoSizeFontPropertyChanged);

        public static bool GetAutoSizeFont(this Label label) => (bool)label.GetValue(AutoSizeFontProperty);

        public static void SetAutoSizeFont(this Label label, bool value) => label.SetValue(AutoSizeFontProperty, value);

        private static void HandleAutoSizeFontPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            Label label = (Label)bindable;

            if ((bool)newValue)
            {
                label.PropertyChanged += AutoSizeFont;
                //AutoSizeFont(label, new System.ComponentModel.PropertyChangedEventArgs(Label.TextProperty.PropertyName));
            }
            else
            {
                label.PropertyChanged -= AutoSizeFont;
            }
        }

        public static double EstimateFontSize(double widthConstraint, double heightConstraint, string Text, double charWidth = 0.5, double lineHeight = 1.2)
        {
            // Because:
            //   lineCount = view.Height / (lineHeight * fontSize)
            //   charsPerLine = view.Width / (charWidth * fontSize)
            //   charCount = lineCount * charsPerLine
            // Hence, solving for fontSize:
            //int fontSize = (int)Math.Sqrt(view.Width * view.Height / (charCount * lineHeight * charWidth));
            //return Math.Min(widthConstraint / charWidth, heightConstraint / lineHeight);

            int maxCharsPerLine = Text.Length;
            int lineCount = 1;
            //Print.Log("here", (int)(widthConstraint / (maxCharsPerLine * charWidth)), (int)(heightConstraint / (lineCount * lineHeight)));
            return Math.Min((int)(widthConstraint / (maxCharsPerLine * charWidth)), (int)(heightConstraint / (lineCount * lineHeight)));
        }

        private static SizeRequest MeasureNative(this Label label) => throw new NotImplementedException();// Device.PlatformServices.GetNativeSize(label, double.PositiveInfinity, double.PositiveInfinity);

        public static SizeRequest Measure(this Label label, double fontSize)
        {
            var backup = label.ClearState();
            label.FontSize = fontSize;
            SizeRequest sr = label.MeasureNative();
            label.SetState(backup);

            return sr;
        }

        private static void AutoSizeFont(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Label label = (Label)sender;

            if (e.PropertyName != VisualElement.WidthProperty.PropertyName && e.PropertyName != VisualElement.HeightProperty.PropertyName && e.PropertyName != Label.TextProperty.PropertyName && e.PropertyName != Label.PaddingProperty.PropertyName)
            {
                if (e.PropertyName == Label.FontSizeProperty.PropertyName)
                {
                    //label.SetAutoSizeFont(false);
                }

                return;
            }

            if (label.Width <= 0 || label.Height <= 0)
            {
                return;
            }

            Size size = label.Bounds.Size;
            if (Device.RuntimePlatform == Device.iOS)
            {
                //size -= new Size(label.Padding.HorizontalThickness + label.Margin.HorizontalThickness, label.Padding.VerticalThickness + label.Margin.VerticalThickness);
            }
            label.FontSize = AutoSizeFont(label, size.Width, size.Height);
        }

        private static IList<Setter> ClearState(this Label label) => label.ClearState(Label.FontSizeProperty);//, VisualElement.WidthRequestProperty, VisualElement.HeightRequestProperty);

        public static double AutoSizeFont(this Label label, double widthConstraint, double heightConstraint)
        {
            if (widthConstraint < 0)
            {
                widthConstraint = double.PositiveInfinity;
            }
            if (heightConstraint < 0)
            {
                heightConstraint = double.PositiveInfinity;
            }

            if (double.IsInfinity(widthConstraint) && double.IsInfinity(heightConstraint))
            {
                System.Diagnostics.Debug.WriteLine("widthConstraint or heightConstraint must be a valid value");
                return label.FontSize;
            }

            var backup = label.ClearState();

            label.FontSize = EstimateFontSize(widthConstraint, heightConstraint, label.Text);
            double increment = 1;
            double lastSign = 0;

            do
            {
                Size size = label.MeasureNative().Request;

                if (size.Width == widthConstraint && size.Height == heightConstraint)
                {
                    break;
                }

                double sign = size.Width < widthConstraint && size.Height < heightConstraint ? 1 : -1;
                if (sign == lastSign * -1)
                {
                    if (lastSign == 1)
                    {
                        label.FontSize--;
                    }

                    break;
                }

                label.FontSize += (lastSign = sign) * increment;
            }
            while (label.FontSize > 0 && label.FontSize < 1000);
            //while (lastSign == 1 || label.FontSize > 0);

            double fontSize = label.FontSize;
            label.SetState(backup);
            return fontSize;
        }
    }
}
