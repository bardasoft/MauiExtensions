using System;

using Constructor = System.Func<double[], object>;
using Deconstructor = System.Func<object, double[]>;

namespace Microsoft.Maui.Controls
{
    using DoubleExtractor = Tuple<Deconstructor, Constructor>;

    public static class PropertyAnimation
    {
        /*private BindableObject Target;
        private BindableProperty Property;
        private Action<object> CallbackAction;

        private DoubleExtractor Extractor;
        private double[] Start;
        private double[] End;

        private Animation Animation;*/

        //public static Animation Create(BindableObject target, BindableProperty property, object end, Easing easing = null, Action<object> callback = null, Action finished = null) => Create(target, property, end, null, easing, callback, finished);

        public static Animation Create(BindableObject target, BindableProperty property, object end, object start = null, Easing easing = null, Action<object> callback = null, Action finished = null, AnimationFactory factory = null)
        {
            //Target = target;
            //Property = property;
            //CallbackAction = callback;

            //Action<double> doubleCallback;

            //if (StateAnimationExtensions.Transition.TryGetValue(Property.ReturnType, out Extractor))

            double[] startValues = null;
            double[] endValues = null;

            DoubleExtractor extractor;
            if (TryGetDoubleExtractor(property.ReturnType, start ?? end, out extractor))
            {
                if (start != null)
                {
                    startValues = extractor.Item1(start);
                }
                endValues = extractor.Item1(end);
                //doubleCallback = Callback;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Error");
                return new Animation();
                //doubleCallback = value => { };
            }

            Action<double> propertyCallback = value =>
            {
                double[] runtimeStartValues = startValues ?? (startValues = extractor.Item1(target.GetValue(property)));

                double[] values = new double[runtimeStartValues.Length];
                for (int k = 0; k < values.Length; k++)
                {
                    values[k] = runtimeStartValues[k] + value * (endValues[k] - runtimeStartValues[k]);
                }

                object reconstructedValue = extractor.Item2(values);
                target.SetValue(property, reconstructedValue);

                callback?.Invoke(reconstructedValue);
            };

            if (factory == null)
            {
                return new Animation(propertyCallback, 0, 1, easing, finished);
            }
            else
            {
                return factory(propertyCallback, 0, 1, easing, finished);
            }
        }

        //public static implicit operator Animation(PropertyAnimation propertyAnimation) => propertyAnimation.Animation;

        private static bool TryGetDoubleExtractor(Type type, object testValue, out DoubleExtractor extractor)
        {
            if (!StateAnimationExtensions.Transition.TryGetValue(type, out extractor))
            {
                try
                {
                    var temp = (double)(dynamic)testValue;
                    extractor = new DoubleExtractor(new Deconstructor(value => new double[] { (double)(dynamic)value }), new Constructor(value => value[0]));
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }
    }
}
