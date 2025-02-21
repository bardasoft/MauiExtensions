using Microsoft.Maui.Controls.Compatibility;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
    public class RemoveBindingTriggerAction :  TriggerAction<BindableObject>
    {
        public BindableProperty? Property { get; set; }

        protected override void Invoke(BindableObject sender)
        {
            sender.RemoveBinding(Property);
        }
    }
}

namespace Microsoft.Maui.Controls.Extensions
{
    public static class VisualElement
    {
        public static readonly BindableProperty LockHeightProperty = BindableProperty.CreateAttached(nameof(GetLockHeight).Substring(3), typeof(bool), typeof(VisualElement), false, propertyChanged: (bindable, oldValue, newValue) =>
        {
            var visualElement = (Controls.VisualElement)bindable;

            if ((bool)newValue)
            {
                LockSizeRequest(visualElement, Controls.VisualElement.HeightProperty, Controls.VisualElement.HeightRequestProperty);
            }
            else
            {
                visualElement.ClearValue(Controls.VisualElement.HeightRequestProperty);
            }
        });

        public static bool GetLockHeight(this Controls.VisualElement bindable) => (bool)bindable.GetValue(LockHeightProperty);
        public static void SetLockHeight(this Controls.VisualElement bindable, bool value) => bindable.SetValue(LockHeightProperty, value);

        public static readonly BindableProperty OneTimeHeightRequestProperty = BindableProperty.CreateAttached(nameof(GetOneTimeHeightRequest).Substring(3), typeof(double), typeof(VisualElement), -1d, propertyChanged: (bindable, oldValue, newValue) =>
        {
            if ((double)oldValue >= 0)
            {
                return;
            }
            
            MakeOneTimeSizeRequest((Controls.VisualElement)bindable, (double)newValue, Controls.VisualElement.HeightProperty, Controls.VisualElement.HeightRequestProperty, OneTimeHeightRequestProperty!);
        });

        public static double GetOneTimeHeightRequest(this Controls.VisualElement bindable) => (double)bindable.GetValue(OneTimeHeightRequestProperty);
        public static void SetOneTimeHeightRequest(this Controls.VisualElement bindable, double value) => bindable.SetValue(OneTimeHeightRequestProperty, value);

        private static void MakeOneTimeSizeRequest(Controls.VisualElement visualElement, double value, BindableProperty sizeProperty, BindableProperty sizeRequestProperty, BindableProperty oneTimeSizeRequestProperty)
        {
            PropertyChangedEventHandler handler = null!;
            handler = (sender, e) =>
            {
                if (e.PropertyName != sizeProperty.PropertyName)
                {
                    return;
                }

                var visualElement = (Controls.VisualElement)sender!;

                if (Equals(visualElement.GetValue(oneTimeSizeRequestProperty), visualElement.GetValue(sizeProperty)))
                {
                    visualElement.PropertyChanged -= handler;
                    visualElement.ClearValue(sizeRequestProperty);
                }
            };

            visualElement.PropertyChanged += handler;
            visualElement.SetValue(sizeRequestProperty, value);
        }

        private static void LockSizeRequest(Controls.VisualElement visualElement, BindableProperty sizeProperty, BindableProperty sizeRequestProperty)
        {
            PropertyChangedEventHandler handler = null!;
            handler = (sender, e) =>
            {
                if (e.PropertyName != sizeProperty.PropertyName)
                {
                    return;
                }

                var visualElement = (Controls.VisualElement)sender!;
                var size = (double)visualElement.GetValue(sizeProperty);
                
                if (size >= 0)
                {
                    visualElement.PropertyChanged -= handler;
                    visualElement.SetValue(sizeRequestProperty, size);
                }
            };

            visualElement.PropertyChanged += handler;
        }

        public static Point PositionOn(this Controls.VisualElement child, Controls.VisualElement parent = null)
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

            return PositionOn(child.Parent<Controls.VisualElement>(), parent).Add(point.Add(new Point(child.X + child.TranslationX, child.Y + child.TranslationY)));
        }
    }
}

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
