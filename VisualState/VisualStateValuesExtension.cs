using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Controls
{
    public class InitialState : Attribute
    {
        public string State;

        public InitialState(string state) => State = state;
    }

    [ContentProperty(nameof(Binding))]
    public class BindingProviderExtension : IMarkupExtension<BindingBase>
    {
        public VisualStateValuesExtension Binding { get; set; }

        public BindingBase ProvideValue(IServiceProvider serviceProvider) => (BindingBase)Binding.ProvideValue(serviceProvider);

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
    }

    [ContentProperty(nameof(Values))]
    public class VisualStateBehavior : Behavior<VisualElement>
    {
        public Setter Values { get; set; }

        public BindableProperty Property { get; set; }

        protected override void OnAttachedTo(VisualElement bindable)
        {
            base.OnAttachedTo(bindable);
        }
    }

    public class VisualStateValues : Dictionary<string, object>
    {
        public VisualElement Owner { get; set; }
        public BindableProperty Property { get; set; }

        public VisualStateValues() { }

        public VisualStateValues(BindableProperty property, VisualElement owner = null) : this()
        {
            Owner = owner;
            Property = property;
        }
    }

    [ContentProperty(nameof(Owner))]
    public abstract class VisualStateValuesExtension : IMarkupExtension//, IEnumerable<KeyValuePair<string, object>>
    {
        public object Owner { get; set; }
        public object Default { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            var valueProvider = serviceProvider?.GetService<IProvideValueTarget>() ?? throw new ArgumentException();
            object target = valueProvider.TargetObject;

            if (target is BindingProviderExtension)
            {
                return this;
            }

            BindableProperty property = (target as Setter)?.Property ?? valueProvider.TargetProperty as BindableProperty;
            //BindableProperty property = valueProvider.TargetProperty as BindableProperty ?? Property;
            VisualStateValues values = new VisualStateValues { Property = property };
            string name = Owner as string ?? "self";
            
            try
            {
                values.Owner = Owner as VisualElement ?? (VisualElement)new ReferenceExtension { Name = name }.ProvideValue(serviceProvider);
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Could not find the visual state group " + name + ". Make sure the group is defined in the namescope where this extension is used, or manually set the Owner property to a valid visual state group");

                values.Owner = target as VisualElement;
            }
            
            foreach (var field in GetType().GetProperties())
            {
                if (field.Name == nameof(Default) || field.Name == nameof(Owner))
                {
                    continue;
                }

                object value = field.GetValue(this) ?? Default;

                if (value != null)
                {
                    values.Add(field.Name, XamlExtensions.Convert(value, serviceProvider));
                }
            }

            if (target is VisualElement visualElement)
            {
                visualElement.SetVisualStateValues(values);

                object result;
                if (!values.TryGetValue(VisualStateManager.GetVisualStateGroups(values.Owner)[0].States[0].Name, out result))
                {
                    return property.DefaultValue;
                }

                return result;

                //object result = GetType().GetProperty(GetType().GetCustomAttributes<InitialState>().FirstOrDefault()?.State ?? string.Empty)?.GetValue(this) ?? Default;
                /*if (result is BindingBase binding)
                {
                    return valueProvider.TargetObject is BindingProviderExtension ? this : (object)binding;
                }*/
                //return result == null ? property.DefaultValue : XamlExtensions.Convert(result, serviceProvider);
            }
            else if (target is Setter setter)
            {
                setter.Property = MergedVisualStatesProperty;
            }

            return values;
        }

        private static readonly BindableProperty MergedVisualStatesProperty = BindableProperty.CreateAttached("MergedVisualStatesProperty", typeof(VisualStateValues), typeof(VisualElement), null, propertyChanged: MergedVisualStatesChanged);

        private static void MergedVisualStatesChanged(BindableObject bindable, object oldValue, object newValue)
        {
            VisualElement visualElement = (VisualElement)bindable;

            if (newValue is VisualStateValues values)
            {
                visualElement.SetVisualStateValues(values);
            }
        }

        /*private IEnumerable<KeyValuePair<string, object>> AsEnumerable(Func<object, object> converter = null)
        {
            var fields = GetType().GetProperties();//.Where(info => info.Name != nameof(Default));// BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            foreach (var field in fields)
            {
                if (field.Name == nameof(Default) || field.Name == nameof(Owner))
                {
                    continue;
                }

                object value = field.GetValue(this) ?? Default;

                if (value == null)
                {
                    continue;
                }

                yield return new KeyValuePair<string, object>(field.Name, converter?.Invoke(value) ?? value);
            }
        }*/

        //public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => AsEnumerable().GetEnumerator();

        //IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /*[ContentProperty(nameof(VisualState))]
    public class VisualStateExtension : IMarkupExtension<VisualState>
    {
        public string StateName { get; set; }

        public VisualState VisualState { get; set; } = new VisualState();

        public static implicit operator VisualState(VisualStateExtension visualStateExtension) => visualStateExtension.VisualState;

        public VisualState ProvideValue(IServiceProvider serviceProvider)
        {
            IProvideValueTarget info = serviceProvider.GetService<IProvideValueTarget>();

            if (VisualState.Name == null)
            {
                VisualState.Name = StateName;
            }
            
            if (info.TargetObject is INameScope nameScope)
            {
                nameScope.RegisterName(VisualState.Name, VisualState);
            }

            return VisualState;
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
    }*/

    [ContentProperty(nameof(Value))]
    public class VisualStateGroupExtension<T> : IMarkupExtension<VisualStateGroup>
        where T : VisualStateValuesExtension
    {
        public VisualStateGroup Value { get; set; } = new VisualStateGroup();

        public static implicit operator VisualStateGroup(VisualStateGroupExtension<T> group) => group.Value;

        public VisualStateGroup ProvideValue(IServiceProvider serviceProvider)
        {
            IProvideValueTarget info = serviceProvider.GetService<IProvideValueTarget>();
            //var fields = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            if (Value.Name == null)
            {
                Value.Name = typeof(T).Name;
            }

            return Value;
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => (this as IMarkupExtension<VisualStateGroup>).ProvideValue(serviceProvider);
    }
}
 