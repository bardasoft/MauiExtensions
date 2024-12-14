using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls.Extensions;

using Deconstructor = System.Func<object, double[]>;
using Constructor = System.Func<double[], object>;

namespace Microsoft.Maui.Controls
{
    public static class AnimationExtensions
    {
        public static void AnimateTo<T>(this T self, string name, VisualState end, uint rate = 16, uint length = 250, Easing easing = null, Action<double, bool> finished = null, Func<bool> repeat = null)
            where T : BindableObject, IAnimatable
        {
            Animation animation = new Animation();

            foreach (Setter setter in end.Setters)
            {
                if (setter.Property.ReturnType != typeof(double))
                {
                    continue;
                }

                animation.Add(0, 1, self.AnimationForProperty(setter));
            }

            self.Animate(name, animation, rate, length, easing, finished, repeat);
        }

        private static void ValidateValue(Type type)
        {
            if (type != typeof(double))
            {
                throw new Exception("Property return type is " + type + "; must be double to be animated");
            }
        }

        private static Action<double> DoubleSetter(this BindableProperty property, BindableObject context) => value => property.Setter(context)(value);

        private static double GetValue(this BindableObject bindable, BindableProperty property, double? value) => value ?? (double)bindable.GetValue(property);

        public static Animation AnimationForProperty(this BindableObject bindable, Setter start = null, Setter end = null, Easing easing = null, Action finished = null)
        {
            if (start != null && end != null && start.Property != end.Property)
            {
                throw new Exception("Setters must be for the same property: start is " + start.Property + " and end is " + end.Property);
            }

            return AnimationForProperty(bindable, start.Property, start == null ? null : (double?)start.Value, end == null ? 1 : (double)end.Value, easing, finished);
        }

        public static Animation AnimationForProperty(this BindableObject bindable, BindableProperty property, double? start = null, double end = 1, Easing easing = null, Action finished = null) => new Animation(property.DoubleSetter(bindable), bindable.GetValue(property, start), end, easing, finished);

        public static void AnimateAtSpeed(this IAnimatable self, string name, Action<double> callback, double start, double end, uint rate = 16, double speed = 1, Easing easing = null, Action<double, bool> finished = null, Func<bool> repeat = null) => self.Animate(name, callback, start, end, rate, (uint)(Math.Abs(start - end) / speed), easing, finished, repeat);

        // Use bindable property setter as callback
        public static void Animate<T>(this T self, string name, BindableProperty property, double? start = null, double end = 1, uint rate = 16, uint length = 250, Easing easing = null, Action<double, bool> finished = null, Func<bool> repeat = null) where T : BindableObject, IAnimatable =>
            self.Animate(name, self.AnimationForProperty(property, start, end), rate, length, easing, finished, repeat);

        public static void AnimateAtSpeed<T>(this T self, string name, BindableProperty property, double? start = null, double end = 1, uint rate = 16, double speed = 1, Easing easing = null, Action<double, bool> finished = null, Func<bool> repeat = null) where T : BindableObject, IAnimatable =>
            AnimateAtSpeed(self, name, property.DoubleSetter(self), self.GetValue(property, start), end, rate, speed, easing, finished, repeat);
    }
}
