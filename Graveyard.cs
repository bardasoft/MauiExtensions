using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Extensions.DependencyInjection;

namespace Graveyard
{
    [ContentProperty(nameof(SourceBinding))]
    public class IntermediateBinding
    {
        public BindingBase SourceBinding { get; set; }
        public BindableObject IntermediateObject { get; set; }
        public BindableProperty IntermediateProperty { get; set; }
        public object TargetObject { get; set; }
        public string TargetPath { get; set; }

        public void Apply()
        {
            if (IntermediateObject == null)
            {
                IntermediateObject = new Intermediary();
            }
            if (IntermediateProperty == null)
            {
                IntermediateProperty = Intermediary.ValueProperty;
            }

            IntermediateObject.SetBinding(IntermediateProperty, SourceBinding);
            IntermediateObject.SetBinding(IntermediateProperty, new Binding(TargetPath, BindingMode.OneWayToSource, source: TargetObject));
        }

        public void Unapply()
        {
            IntermediateObject.RemoveBinding(IntermediateProperty);
        }

        private class Intermediary : BindableObject
        {
            public static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(object), typeof(Intermediary));

            public object Value
            {
                get => GetValue(ValueProperty);
                set => SetValue(ValueProperty, value);
            }
        }
    }

    [ContentProperty(nameof(Binding))]
    public class IntermediateBindingTestExtension : IMarkupExtension
    {
        public BindingBase Binding { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            var valueProvider = serviceProvider?.GetService<IProvideValueTarget>() ?? throw new ArgumentException();
            var propertyInfo = valueProvider.TargetProperty as System.Reflection.PropertyInfo ?? throw new InvalidOperationException("Cannot determine property to provide the value for.");

            var binding = new IntermediateBinding
            {
                SourceBinding = Binding,
                TargetObject = valueProvider.TargetObject,
                TargetPath = propertyInfo.Name
            };

            return binding.IntermediateObject.GetValue(binding.IntermediateProperty);
        }
    }

    [ContentProperty(nameof(Property))]
    public class AttachedBindingExtension : IMarkupExtension<BindingBase>
    {
        public BindingExtension Binding { get; set; }
        public BindableProperty Property { get; set; }

        private AttachedBindingProxy Proxy = new AttachedBindingProxy();
        private static readonly IMarkupExtension<BindingBase> Provider = new BindingExtension();

        public BindingBase ProvideValue(IServiceProvider serviceProvider)
        {
            var valueProvider = serviceProvider?.GetService<IProvideValueTarget>() ?? throw new ArgumentException();
            //var propertyInfo = valueProvider.TargetProperty as System.Reflection.PropertyInfo ?? throw new InvalidOperationException("Cannot determine property to provide the value for.");

            var binding = (Binding as IMarkupExtension<BindingBase>).ProvideValue(serviceProvider);
            Print.Log(valueProvider?.TargetObject);

            object source = null;
            if (binding is Binding standard)
            {
                source = standard.Source;
                standard.Path = BindableObject.BindingContextProperty.PropertyName;
            }

            if (source is BindableObject bindable)
            {
                //bindable.SetBinding(Property, new Binding(BindableObject.BindingContextProperty.PropertyName, source: proxy, mode: BindingMode.OneWayToSource));

                TransferAttachedProperty(bindable, new PropertyChangedEventArgs(Property.PropertyName));
                bindable.PropertyChanged += TransferAttachedProperty;
            }

            return binding;
        }

        private void TransferAttachedProperty(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Property.PropertyName && sender is BindableObject bindable)
            {
                Proxy.BindingContext = bindable.GetValue(Property);
            }
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);

        private class AttachedBindingProxy : BindableObject { }
    }

    public abstract class SelfDetachingBehavior<T> : Behavior<T> where T : VisualElement
    {
        private BindableProperty ParentPageProperty;
        private T Bindable;

        protected override void OnAttachedTo(T bindable)
        {
            base.OnAttachedTo(bindable);

            if (ParentPageProperty == null)
            {
                ParentPageProperty = BindableProperty.CreateAttached("Page", typeof(Page), typeof(T), null, propertyChanged: ParentPageChanged);
            }

            bindable.SetBinding(ParentPageProperty, new Binding(".", source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(Page))));
            Bindable = bindable;
        }

        protected override void OnDetachingFrom(T bindable)
        {
            base.OnDetachingFrom(bindable);

            bindable.RemoveBinding(ParentPageProperty);
            //bindable.ClearValue(ParentPageProperty);
        }

        private void ParentPageChanged(BindableObject bindable, object oldValue, object newValue)
        {
            VisualElement visualElement = (VisualElement)bindable;

            if (oldValue is Page oldPage)
            {
                oldPage.Appearing -= ParentPageAppearing;
                oldPage.Disappearing -= ParentPageDisappearing;
            }

            if (newValue is Page page)
            {
                page.Appearing += ParentPageAppearing;
                page.Disappearing += ParentPageDisappearing;
            }
        }

        private void ParentPageAppearing(object sender, EventArgs e)
        {
            if (Bindable != null && !Bindable.Behaviors.Contains(this))
            {
                Bindable.Behaviors.Add(this);
            }
        }
        private void ParentPageDisappearing(object sender, EventArgs e) => Bindable?.Behaviors.Remove(this);
    }

    public abstract class LambdaBindingSource<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public T Value { get; private set; }

        protected abstract void Attached();

        protected void UpdateValue(T value)
        {
            Value = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        }
    }

    [ContentProperty(nameof(GestureRecognizer))]
    public class AddGestureRecognizerBehavior : Behavior<View>
    {
        //public DataTemplate GestureRecognizerTemplate { get; set; }
        public GestureRecognizer GestureRecognizer { get; set; }

        protected override void OnAttachedTo(View bindable)
        {
            base.OnAttachedTo(bindable);
            GestureRecognizer.BindingContext = bindable.BindingContext;
            bindable.GestureRecognizers.Add(GestureRecognizer);
        }

        protected override void OnDetachingFrom(View bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.GestureRecognizers.Remove(GestureRecognizer);
        }
    }

    public class ParameterBehavior : Behavior<VisualElement>
    {
        public string TargetPath { get; set; }
        public BindingBase SourceBinding { get; set; }

        protected override void OnAttachedTo(VisualElement bindable)
        {
            base.OnAttachedTo(bindable);

            new IntermediateBinding
            {
                SourceBinding = SourceBinding,
                IntermediateObject = bindable,
                IntermediateProperty = BindableProperty.CreateAttached(TargetPath + "Paramter", typeof(object), bindable.GetType(), null),
                TargetObject = bindable.BindingContext,
                TargetPath = TargetPath,
            }.Apply();
        }
    }

    [ContentProperty(nameof(Value))]
    public class ReadOnlyBinding
    {
        public object Value
        {
            get => Proxy.BindingContext;
            set => Proxy.BindingContext = value;
        }

        public BindingBase Binding
        {
            set
            {
                value.Mode = BindingMode.OneWayToSource;
                Proxy.SetBinding(BindableObject.BindingContextProperty, value);
            }
        }

        private BindableProxy Proxy = new BindableProxy();

        public void UnapplyAll()
        {
            Proxy.RemoveBinding(BindableObject.BindingContextProperty);
        }

        private class BindableProxy : BindableObject { }
    }

    public class AttachedBindingBehavior : Behavior
    {
        public BindableProperty TargetProperty { get; set; }
        public BindableProperty SourceProperty { get; set; }
        public BindableObject Source { get; set; }

        protected override void OnAttachedTo(BindableObject bindable)
        {
            base.OnAttachedTo(bindable);

            UpdateValue(bindable, new PropertyChangedEventArgs(SourceProperty.PropertyName));
            bindable.PropertyChanged += UpdateValue;
        }

        protected override void OnDetachingFrom(BindableObject bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.PropertyChanged -= UpdateValue;
        }

        private void UpdateValue(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == SourceProperty.PropertyName && sender is BindableObject bindable)
            {
                bindable.SetValue(TargetProperty, (Source ?? bindable).GetValue(SourceProperty));
            }
        }
    }
}
