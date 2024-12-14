using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace System.ComponentModel
{
    //
    // Summary:
    //     Represents the method that will handle the System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    //     event raised when a property is changed on a component.
    //
    // Parameters:
    //   sender:
    //     The source of the event.
    //
    //   e:
    //     A System.ComponentModel.PropertyChangedEventArgs that contains the event data.
    public delegate void PropertyChangeEventHandler(object sender, PropertyChangeEventArgs e);

    //
    // Summary:
    //     Provides data for the System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    //     event.
    public class PropertyChangeEventArgs : EventArgs
    {
        //
        // Summary:
        //     Gets the name of the property that changed.
        //
        // Returns:
        //     The name of the property that changed.
        public string PropertyName { get; }

        public object OldValue { get; }
        public object NewValue { get; }

        //
        // Summary:
        //     Initializes a new instance of the System.ComponentModel.PropertyChangedEventArgs
        //     class.
        //
        // Parameters:
        //   propertyName:
        //     The name of the property that changed.
        public PropertyChangeEventArgs(string propertyName, object oldValue, object newValue)
        {
            PropertyName = propertyName;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    //
    // Summary:
    //     Notifies clients that a property value has changed.
    public interface INotifyPropertyChange
    {
        //
        // Summary:
        //     Occurs when a property value changes.
        event PropertyChangeEventHandler PropertyChange;
    }

    public class MultiValueConverter : IMultiValueConverter
    {
        public delegate object ConvertDelegate(object[] values, Type targetType, object parameter, CultureInfo culture);
        public delegate object[] ConvertBackDelegate(object value, Type[] targetTypes, object parameter, CultureInfo culture);

        public ConvertDelegate ConvertFunc { get; }
        public ConvertBackDelegate ConvertBackFunc { get; }

        public MultiValueConverter(ConvertDelegate convertFunc, ConvertBackDelegate convertBackFunc)
        {
            ConvertFunc = convertFunc;
            ConvertBackFunc = convertBackFunc;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => ConvertFunc?.Invoke(values, targetType, parameter, culture);

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => ConvertBackFunc?.Invoke(value, targetTypes, parameter, culture);
    }

    public abstract class BindableViewModel : INotifyPropertyChanged, INotifyPropertyChanging, INotifyPropertyChange
    {
        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangeEventHandler PropertyChange;

        public void UseEffect(string propertyName, params BindableProperty[] dependents)
        {
            var propertyNames = dependents.Select(dependent => dependent.PropertyName).ToList<string>();

            PropertyChanged += (sender, e) =>
            {
                if (propertyNames.Contains(e.PropertyName))
                {
                    OnPropertyChanged(propertyName);
                }
            };
        }

        public static void UseEffect(BindableObject bindable, BindableProperty property, MultiValueConverter.ConvertDelegate convert, params BindableProperty[] dependents)
        {
            bindable.SetBinding(property, new MultiBinding
            {
                Converter = new MultiValueConverter(convert, null),
                Bindings = dependents.Select<BindableProperty, BindingBase>(dependent => new Binding(dependent.PropertyName)).ToList<BindingBase>()
            });
        }

        protected virtual void SetValue<T>(ref T property, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(property, value))
            {
                OnPropertyChanging(propertyName);
                var oldValue = property;
                property = value;
                OnPropertyChanged(propertyName);
                OnPropertyChange(oldValue, value, propertyName);
            }
        }

        protected virtual bool UpdateValue<T>(ref T property, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(property, value))
            {
                OnPropertyChanging(propertyName);
                var e = new PropertyChangeEventArgs(propertyName, property, property = value);
                OnPropertyChanged(propertyName);
                PropertyChange?.Invoke(this, e);

                return true;
            }

            return false;
        }

        protected virtual void OnPropertyChanging([CallerMemberName] string property = null)
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(property));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        protected virtual void OnPropertyChange(object oldValue, object newValue, [CallerMemberName] string property = null)
        {
            PropertyChange?.Invoke(this, new PropertyChangeEventArgs(property, oldValue, newValue));
        }
    }
}
