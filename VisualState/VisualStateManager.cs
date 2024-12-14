using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using XFView = Microsoft.Maui.Controls.VisualStateManager;

namespace Microsoft.Maui.Controls.Extensions
{
    public class AttachedStateTrigger : StateTriggerBase
    {
        public BindableProperty Property { get; set; }
        public object Value { get; set; }

        public AttachedStateTrigger()
        {
            ;
        }

        protected override void OnAttached()
        {
            Print.Log(BindingContext);
            base.OnAttached();

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == BindingContextProperty.PropertyName)
                {
                    ;
                }
            };
        }

        protected override void OnBindingContextChanged()
        {
            Print.Log(BindingContext);
            base.OnBindingContextChanged();
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            Print.Log(propertyName + " property changed");
            if (propertyName == Property.PropertyName)
            {
                ValueChanged();
            }
        }

        protected virtual void ValueChanged()
        {
            if (BindingContext is BindableObject bindable)
            {
                SetActive(bindable.GetValue(Property) == Value);
            }
        }
    }

    public static class VisualStateManager
    {
        public static readonly BindableProperty VisualStateProperty = BindableProperty.CreateAttached("VisualState", typeof(string), typeof(VisualElement), XFView.CommonStates.Normal, propertyChanged: (bindable, oldValue, newValue) => XFView.GoToState((VisualElement)bindable, (string)newValue));

        public static readonly BindableProperty GoToStateCommandProperty = BindableProperty.CreateAttached("GoToState", typeof(ICommand), typeof(VisualElement), null, BindingMode.OneWayToSource, defaultValueCreator: bindable => new Command<string>(stateName =>
        {
            VisualElement visualElement = (VisualElement)bindable;

            XFView.GoToState(visualElement, stateName);
        }));

        public static readonly BindableProperty AutoWireStateChangeProperty = BindableProperty.CreateAttached("AutoWireStateChange", typeof(bool), typeof(VisualElement), false, propertyChanged: (b, o, n) =>
        {
            VisualElement visualElement = (VisualElement)b;

            if (visualElement is Button button && !button.IsSet(Button.CommandProperty))
            {
                button.SetBinding(GoToStateCommandProperty, new Binding("Command", source: button));
            }
        });


        public static string GetVisualState(this VisualElement visualElement) => (string)visualElement.GetValue(VisualStateProperty);
        public static string GetCanSetState(this VisualElement visualElement) => (string)visualElement.GetValue(AutoWireStateChangeProperty);
        public static ICommand GetGoToStateCommand(this VisualElement visualElement) => (ICommand)visualElement.GetValue(GoToStateCommandProperty);
        public static void SetVisualState(this VisualElement visualElement, string value) => visualElement.SetValue(VisualStateProperty, value);
        public static void SetCanSetState(this VisualElement visualElement, string value) => visualElement.SetValue(AutoWireStateChangeProperty, value);
        public static void SetGoToStateCommand(this VisualElement visualElement, ICommand value) => visualElement.SetValue(GoToStateCommandProperty, value);
    }
}
