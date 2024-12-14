using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls.Extensions;

namespace Microsoft.Maui.Controls
{
    public static class NativeImplementation
    {
        public static BindableProperty CreateAttached(string name) => BindableProperty.CreateAttached(name, typeof(Delegate), typeof(NativeImplementation), null);

        public static void SetNativeImplementation<T>(this BindableObject bindable, BindableProperty<T> property, T value) => bindable.SetValue(property, value);

        public static T GetNativeImplementation<T>(this BindableObject bindable, BindableProperty<T> property)
        {
            object value = bindable.GetValue(property);

            if (value == null)
            {
                System.Diagnostics.Debug.WriteLine("Functionality " + ((BindableProperty)property).PropertyName + " has not been implemented natively for " + bindable.GetType());
                return default;
            }
            else
            {
                return (T)value;
            }
        }
    }

    public class BindableProperty<T>
    {
        private BindableProperty Property;

        public BindableProperty(BindableProperty property) => Property = property;

        public static implicit operator BindableProperty(BindableProperty<T> property) => property.Property;
        public static implicit operator BindableProperty<T>(BindableProperty property) => new BindableProperty<T>(property);
    }
}
