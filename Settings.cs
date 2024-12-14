using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
    public static class Settings
    {
        public delegate T DeserializeDelegate<T>(object value);
        public delegate object SerializeDelegate<T>(T value);

        private static Dictionary<string, Setting> Registered = new Dictionary<string, Setting>();

        public static BindableValue<T> Register<T>(string identifier, T defaultValue = default, BindingMode defaultBindingMode = BindingMode.TwoWay, BindableProperty.ValidateValueDelegate validateValue = null, BindableProperty.BindingPropertyChangedDelegate propertyChanged = null, BindableProperty.BindingPropertyChangingDelegate propertyChanging = null, BindableProperty.CoerceValueDelegate coerceValue = null, BindableProperty.CreateDefaultValueDelegate defaultValueCreator = null, DeserializeDelegate<T> deserializer = null, SerializeDelegate<T> serializer = null) => Register(identifier, new BindableValue<T>(typeof(T), defaultValue, defaultBindingMode, validateValue, propertyChanged, propertyChanging, coerceValue, defaultValueCreator), deserializer, serializer);

        public static BindableValue<T> Register<T>(string identifier, BindableValue<T> bindable, DeserializeDelegate<T> deserializer = null, SerializeDelegate<T> serializer = null)
        {
            if (Registered.ContainsKey(identifier))
            {
                throw new ArgumentException("A Setting with identifier " + identifier + " already exists; Setting identifiers must be unique");
            }
            else
            {
                Setting setting = new Setting(bindable,
                    value => deserializer != null ? deserializer(value) : (T)value,
                    value => serializer?.Invoke((T)value) ?? value);

                Registered.Add(identifier, setting);

                return bindable;
            }
        }

        /// <summary>
        /// Loads values stored in Application.Current.Properties into the associated registered settings
        /// </summary>
        /// <returns>
        /// false if there exists a registered setting and a corresponding dictionary entry that
        /// cannot be converted to the type of the setting. A registered setting with no dictionary entry or a dictionary entry with no registered setting will not cause this method to fail.
        /// </returns>
        //public static bool Load() => LoadFrom(Application.Current.Properties);

        public static bool LoadFrom(IDictionary<string, object> storage)
        {
            bool successfullyLoaded = true;
            
            foreach (KeyValuePair<string, object> kvp in storage)
            {
                Setting setting;
                if (Registered.TryGetValue(kvp.Key, out setting))
                {
                    if (!setting.LoadValue(kvp.Value))
                    {
                        successfullyLoaded = false;
                    }
                }
            }

            return successfullyLoaded;
        }

        /// <summary>
        /// Stores the values of all registered settings in the Application.Current.Properties dictionary
        /// </summary>
        /// <param name="clearValues">
        /// If true, the dictionary will be cleared before values are stored. This can help save space by
        /// removing old values that are no longer being used.
        /// </param>
        //public static void Store(bool clearValues = false)
        //{
        //    if (clearValues)
        //    {
        //        Application.Current.Properties.Clear();
        //    }
        //    SaveTo(Application.Current.Properties);
        //}

        public static void SaveTo(IDictionary<string, object> storage)
        {
            foreach (KeyValuePair<string, Setting> kvp in Registered)
            {
                storage[kvp.Key] = kvp.Value.SerializedValue;
            }
        }

        public static Dictionary<string, object> Save()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            SaveTo(result);
            return result;
        }

        private class Setting
        {
            public object SerializedValue
            {
                get
                {
                    try
                    {
                        return Serializer?.Invoke(Bindable.Value) ?? Bindable.Value;
                    }
                    catch
                    {
                        return Bindable.Value;
                    }
                }
            }

            private readonly BindableValue Bindable;
            private readonly Func<object, object> Deserializer;
            private readonly Func<object, object> Serializer;

            public Setting(BindableValue bindable, Func<object, object> deserializer, Func<object, object> serializer)
            {
                Bindable = bindable;
                Deserializer = deserializer;
                Serializer = serializer;
            }

            public bool LoadValue(object value)
            {
                try
                {
                    value = Deserializer?.Invoke(value) ?? value;
                    Bindable.SetValue(Bindable.ValueProperty, value);

                    return true;
                }
                catch
                {
                    return false;
                }

                //return value.GetType() == Bindable.ValueProperty.ReturnType;
            }
        }
    }
}
