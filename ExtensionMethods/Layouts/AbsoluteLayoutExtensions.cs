using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Controls
{
    public static class AbsoluteLayoutExtensions
    {
        public static void SetLayout(BindableObject bindable, AbsoluteLayoutData layoutData, AbsoluteLayoutFlags? flags = null)
        {
            Rect? currentBounds = null;
            if (layoutData.Location == null || layoutData.Size == null)
            {
                currentBounds = AbsoluteLayout.GetLayoutBounds(bindable);
            }

            AbsoluteLayout.SetLayoutBounds(bindable, new Rect(layoutData.Location ?? currentBounds.Value.Location, layoutData.Size ?? currentBounds.Value.Size));
            AbsoluteLayout.SetLayoutFlags(bindable, flags ?? layoutData.Flags ?? AbsoluteLayout.GetLayoutFlags(bindable));
        }

        public static void SetLayout(BindableObject bindable, double? x = null, double? y = null, double? width = null, double? height = null, AbsoluteLayoutFlags? flags = null)
        {
            Rect bounds = AbsoluteLayout.GetLayoutBounds(bindable);
            SetLayout(bindable, new Rect(x ?? bounds.X, y ?? bounds.Y, width ?? bounds.Width, height ?? bounds.Height), flags);
        }

        public static void Add(this Compatibility.AbsoluteLayout.IAbsoluteList<View> children, View view, AbsoluteLayoutData bounds, AbsoluteLayoutFlags? flags = null, int? index = null)
        {
            children.Insert(index ?? children.Count, view);
            SetLayout(view, bounds, flags);
        }

        public class AbsoluteLayoutData
        {
            public Point? Location { get; set; }

            public Size? Size { get; set; }

            public AbsoluteLayoutFlags? Flags { get; set; }

            public static implicit operator AbsoluteLayoutData(Rect bounds) => new AbsoluteLayoutData { Location = bounds.Location, Size = bounds.Size };

            public static implicit operator AbsoluteLayoutData(Point location) => new AbsoluteLayoutData { Location = location };

            public static implicit operator AbsoluteLayoutData(Size size) => new AbsoluteLayoutData { Size = size };

            public static implicit operator AbsoluteLayoutData(AbsoluteLayoutFlags flags) => new AbsoluteLayoutData { Flags = flags };
        }

        private static double Convert(double value, double limit, bool absoluteToProportional)
        {
            // go from absolute to proportional
            //if (!valueFlags.HasFlag(flag) && targetFlags.HasFlag(flag))
            if (absoluteToProportional)
            {
                return limit == 0 ? 0 : (value / limit).Bound(0, 1);
            }
            // go from proportional to absolute
            //else if (valueFlags.HasFlag(flag) && !targetFlags.HasFlag(flag))
            else //if (!isAbsolute && shouldBeAbsolute)
            {
                return value * limit;
            }
            /*else
            {
                return value;
            }*/
        }

        public static void ConvertLayoutBounds(VisualElement visualElement, AbsoluteLayoutFlags flags)
        {
            if (!(visualElement.Parent is AbsoluteLayout absoluteLayout))
            {
                System.Diagnostics.Debug.WriteLine(nameof(visualElement) + " is not a child of an absolute layout");
                return;
            }

            Rect bounds = AbsoluteLayout.GetLayoutBounds(visualElement);
            AbsoluteLayoutFlags currentFlags = AbsoluteLayout.GetLayoutFlags(visualElement);

            Size parentSize = absoluteLayout.Bounds.Size - new Size(absoluteLayout.Padding.HorizontalThickness, absoluteLayout.Padding.VerticalThickness);

            Size absoluteSize = bounds.Size; 
            if (currentFlags.HasFlag(AbsoluteLayoutFlags.WidthProportional))
            {
                absoluteSize.Width = Convert(absoluteSize.Width, parentSize.Width, false);
            }
            else if (absoluteSize.Width == AbsoluteLayout.AutoSize)
            {
                absoluteSize.Width = visualElement.Width;
            }

            if (currentFlags.HasFlag(AbsoluteLayoutFlags.HeightProportional))
            {
                absoluteSize.Height = Convert(absoluteSize.Height, parentSize.Height, false);
            }
            else if (absoluteSize.Height == AbsoluteLayout.AutoSize)
            {
                absoluteSize.Height = visualElement.Height;
            }

            double Adjust(double value, double limit, AbsoluteLayoutFlags flag)
            {
                bool isProportional = currentFlags.HasFlag(flag);
                bool shouldBeProportional = flags.HasFlag(flag);

                return isProportional == shouldBeProportional ? value : Convert(value, limit, !isProportional && shouldBeProportional);
            }

            bounds.Width = Adjust(bounds.Width, parentSize.Width, AbsoluteLayoutFlags.WidthProportional);
            bounds.Height = Adjust(bounds.Height, parentSize.Height, AbsoluteLayoutFlags.HeightProportional);

            parentSize -= absoluteSize;
            bounds.X = Adjust(bounds.X, parentSize.Width, AbsoluteLayoutFlags.XProportional);
            bounds.Y = Adjust(bounds.Y, parentSize.Height, AbsoluteLayoutFlags.YProportional);

            SetLayout(visualElement, bounds, flags);
        }

        public static Rect GetLayoutBounds(this Rect bounds, Rect context, AbsoluteLayoutFlags currentFlags, AbsoluteLayoutFlags desiredFlags)
        {
            //AbsoluteLayoutFlags currentFlags = AbsoluteLayout.GetLayoutFlags(bindable);
            //Rectangle bounds = AbsoluteLayout.GetLayoutBounds(bindable);
            Rect feasibleBounds = context.Shrink(bounds.Size);

            foreach (var stuff in new Tuple<AbsoluteLayoutFlags, Func<Rect, double>, Action<double>, Rect>[]
            {
                new Tuple<AbsoluteLayoutFlags, Func<Rect, double>, Action<double>, Rect>(AbsoluteLayoutFlags.XProportional, (rect) => rect.X, (value) => bounds.X = value, feasibleBounds),
                new Tuple<AbsoluteLayoutFlags, Func<Rect, double>, Action<double>, Rect>(AbsoluteLayoutFlags.YProportional, (rect) => rect.Y, (value) => bounds.Y = value, feasibleBounds),
                new Tuple<AbsoluteLayoutFlags, Func<Rect, double>, Action<double>, Rect>(AbsoluteLayoutFlags.WidthProportional, (rect) => rect.Width, (value) => bounds.Width = value, context),
                new Tuple<AbsoluteLayoutFlags, Func<Rect, double>, Action<double>, Rect>(AbsoluteLayoutFlags.HeightProportional, (rect) => rect.Height, (value) => bounds.Height = value, context)
            })
            {
                // go from absolute to proportional
                if (!currentFlags.HasFlag(stuff.Item1) && desiredFlags.HasFlag(stuff.Item1))
                {
                    stuff.Item3(stuff.Item2(bounds) / stuff.Item2(stuff.Item4));
                }
                // go from proportional to absolute
                else if (currentFlags.HasFlag(stuff.Item1) && !desiredFlags.HasFlag(stuff.Item1))
                {
                    stuff.Item3(stuff.Item2(bounds) * stuff.Item2(stuff.Item4));
                }
            }

            return bounds;
        }
    }
}
