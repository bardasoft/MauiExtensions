using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility
{
    public static class VisualElementAdditions
    {
        public static BindableProperty VisibilityProperty = BindableProperty.CreateAttached("Visibility", typeof(double), typeof(VisualElement), 1.0, propertyChanged: (bindable, oldValue, newValue) =>
        {
            VisualElement visualElement = (VisualElement)bindable;
            visualElement.IsVisible = (visualElement.Opacity = (double)newValue) > 0;
        });

        public static double GetVisibility(this VisualElement visualElement) => (double)visualElement.GetValue(VisibilityProperty);

        public static void SetVisibility(this VisualElement visualElement, double value) => visualElement.SetValue(VisibilityProperty, value);
    }

    public static class VisualElementExtensions
    {
        public static void SizeRequest(this VisualElement element, Size size) => SizeRequest(element, size.Width, size.Height);

        public static void SizeRequest(this VisualElement element, double size) => SizeRequest(element, size, size);

        public static void SizeRequest(this VisualElement element, double width, double height)
        {
            element.WidthRequest = width;
            element.HeightRequest = height;
        }

        public static Size Measure(this VisualElement ve) => ve.Measure(double.PositiveInfinity, double.PositiveInfinity);

        public static Point PositionOn(this VisualElement child, VisualElement parent = null)
        {
            //return child.ositionOn(parent);

            Point point = Point.Zero;

            if (child?.Parent is ScrollView scroll)
            {
                point = point.Subtract(scroll.ScrollPos());
            }
            
            if (child == parent)
            {
                return point;
            }
            else if (child is null)
            {
                throw new Exception("child is not a descendant of parent");
            }

            return PositionOn(child.Parent<VisualElement>(), parent).Add(point.Add(new Point(child.X + child.TranslationX, child.Y + child.TranslationY)));
        }

        /*public static Point ositionOn(this View child, View parent)
        {
            if (child == parent || child is null)
            {
                return Point.Zero;
            }

            return child.ParentView().PositionOn(parent).Add(new Point(child.X, child.Y + child.TranslationY));
        }*/
    }
}
