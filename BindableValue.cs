using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
    public abstract class BindableValue : BindableObject
    {
        public readonly BindableProperty ValueProperty;

        public object Value => GetValue(ValueProperty);

        public BindableValue(Type returnType, object defaultValue = null, BindingMode defaultBindingMode = BindingMode.OneWay, BindableProperty.ValidateValueDelegate validateValue = null, BindableProperty.BindingPropertyChangedDelegate propertyChanged = null, BindableProperty.BindingPropertyChangingDelegate propertyChanging = null, BindableProperty.CoerceValueDelegate coerceValue = null, BindableProperty.CreateDefaultValueDelegate defaultValueCreator = null)
        {
            ValueProperty = BindableProperty.Create("Value", returnType, GetType(), defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue, defaultValueCreator);
        }
    }

    public class BindableValue<T> : BindableValue
    {
        new public T Value
        {
            get => (T)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public BindableValue(Type returnType, object defaultValue = null, BindingMode defaultBindingMode = BindingMode.OneWay, BindableProperty.ValidateValueDelegate validateValue = null, BindableProperty.BindingPropertyChangedDelegate propertyChanged = null, BindableProperty.BindingPropertyChangingDelegate propertyChanging = null, BindableProperty.CoerceValueDelegate coerceValue = null, BindableProperty.CreateDefaultValueDelegate defaultValueCreator = null) : base(returnType, defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue, defaultValueCreator) { }
    }
}
