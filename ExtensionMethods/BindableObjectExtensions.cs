using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Extensions;

namespace Microsoft.Maui.Controls
{
    public class BindingPropertyChangedWrapper<TOwner, TValue>
    {


        private static BindableProperty.BindingPropertyChangedDelegate HandlePropertyChanged<TOwner, T>(Func<TOwner, Action<T, T>> action) where TOwner : BindableObject => (bindable, oldValue, newValue) => action((TOwner)bindable)((T)oldValue, (T)newValue);
    }

    public static class BindableObjectExtensions
    {
        public static IList<Setter> GetState(this BindableObject bindable, params BindableProperty[] properties)
        {
            IList<Setter> result = new List<Setter>();

            foreach (BindableProperty property in properties)
            {
                result.Add(new Setter { Property = property, Value = bindable.GetValue(property) });
            }

            return result;
        }

        public static void SetState(this BindableObject bindable, params Setter[] state) => SetState(bindable, (IEnumerable<Setter>)state);

        public static void SetState(this BindableObject bindable, IEnumerable<Setter> state)
        {
            foreach (Setter setter in state)
            {
                bindable.SetValue(setter.Property, setter.Value);
            }
        }

        public static IList<Setter> ClearState(this BindableObject bindable, params BindableProperty[] properties)
        {
            IList<Setter> backup = bindable.GetState(properties);
            foreach (BindableProperty property in properties)
            {
                bindable.ClearValue(property);
            }

            return backup;
        }

        public static Action<object> Setter(this BindableProperty property, BindableObject context) => value => context.SetValue(property, value);

        private static IValueConverter ValueConverterFromFuncs<T1, T2>(Func<T1, T2> convert, Func<T2, T1> convertBack) => convert == null && convertBack == null ? null : new ValueConverter<T1, T2>(convert, convertBack);

        public static void SetBinding<TTarget, TSource>(this BindableObject bindable, BindableProperty targetProperty, string path, Func<TSource, TTarget> converter = null, Func<TTarget, TSource> convertBack = null, BindingMode mode = BindingMode.Default, object converterParameter = null, string stringFormat = null) => bindable.SetBinding(targetProperty, path, mode, ValueConverterFromFuncs(converter, convertBack), stringFormat);

        public static void SetBinding<TTarget, TSource>(this BindableObject bindable, BindableProperty targetProperty, object source, string path, Func<TSource, TTarget> converter = null, Func<TTarget, TSource> convertBack = null, BindingMode mode = BindingMode.Default, object converterParameter = null, string stringFormat = null) => SetBinding(bindable, targetProperty, source, path, ValueConverterFromFuncs(converter, convertBack), mode, converterParameter, stringFormat);

        public static void SetBinding(this BindableObject bindable, BindableProperty targetProperty, object source, string path, Func<object, object> converter = null, Func<object, object> convertBack = null, BindingMode mode = BindingMode.Default, object converterParameter = null, string stringFormat = null) => SetBinding(bindable, targetProperty, source, path, ValueConverterFromFuncs(converter, convertBack), mode, converterParameter, stringFormat);

        public static void SetBinding(this BindableObject bindable, BindableProperty targetProperty, object source, string path, IValueConverter converter, BindingMode mode = BindingMode.Default, object converterParameter = null, string stringFormat = null) => bindable.SetBinding(targetProperty, new Binding(path, mode, converter, converterParameter, stringFormat, source));

        public static T GetValue<T>(this BindableObject bindable, BindableProperty property) => (T)bindable.GetValue(property);

        public static void SetValues(this BindableObject bindable, object value, params BindableProperty[] properties)
        {
            foreach (BindableProperty property in properties)
            {
                bindable.SetValue(property, value);
            }
        }

        public static void Bind<T>(this BindableObject bindable, BindableProperty property, Action<T> action) => Bind(() => (T)bindable.GetValue(property), bindable, property.PropertyName, action);

        public static void Bind<T>(this INotifyPropertyChanged bindable, string propertyName, Action<T> action) => Bind(() => (T)bindable.GetType().GetProperty(propertyName).GetValue(bindable, null), bindable, propertyName, action);

        private static void Bind<T>(Func<T> getter, INotifyPropertyChanged bindable, string propertyName, Action<T> action)
        {
            void Execute() => action(getter());

            Execute();
            bindable.WhenPropertyChanged(propertyName, (sender, e) => Execute());
        }

        public static void WhenPropertyChanged(this INotifyPropertyChanged bindable, BindablePropertyIdentifer identifier, Action<object, PropertyChangedEventArgs> action)
        {
            bindable.PropertyChanged += (sender, e) =>
            {
                if (identifier.Name == e.PropertyName)
                {
                    action(sender, e);
                }
            };
            //bindable.PropertyChanged += new ConditionalSubscriber<PropertyChangedEventArgs>((sender, e) => e.PropertyName == property.PropertyName, action);
        }

        public class BindablePropertyIdentifer
        {
            public string Name => (Identifier as BindableProperty)?.PropertyName ?? (string)Identifier;

            private object Identifier;

            public BindablePropertyIdentifer(string name) => Identifier = name;
            public BindablePropertyIdentifer(BindableProperty property) => Identifier = property;

            public static implicit operator BindablePropertyIdentifer(string name) => new BindablePropertyIdentifer(name);

            public static implicit operator BindablePropertyIdentifer(BindableProperty property) => new BindablePropertyIdentifer(property);
        }
    }
}
