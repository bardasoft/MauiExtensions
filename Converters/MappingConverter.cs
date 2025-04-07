using System.Collections;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
    public class Mapping : Mapping<object, object> { }

    [ContentProperty(nameof(Value))]
    public class Mapping<TKey, TValue>
    {
        public TKey? Key { get; set; }
        public TValue? Value { get; set; }
    }

    public class MappingConverter : MappingConverter<object, object> { }

    [ContentProperty(nameof(Mappings))]
    public class MappingConverter<TKey, TValue> : IValueConverter, IMultiValueConverter
        where TKey : notnull
    {
        public ICollection<Mapping<TKey, TValue>> Mappings => _Mappings;
        public object? Default { get; set; }

        private MappingDictionary<TKey, TValue> _Mappings = new MappingDictionary<TKey, TValue>();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => Convert((TKey)value, targetType, parameter, culture, _Mappings, Default);

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 0)
            {
                return Default!;
            }

            object? result = this;

            foreach (var value in values)
            {
                if (result is MappingConverter<TKey, TValue> converter)
                {
                    result = converter.Convert(value, targetType, parameter, culture);
                }
                else if (result is IDictionary<TKey, TValue> dictionary)
                {
                    result = Convert((TKey)value, targetType, parameter, culture, dictionary, Default);
                }
            }

            return result!;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static object? Convert(TKey? value, Type targetType, object? parameter, CultureInfo culture, IDictionary<TKey, TValue> mappings, object? keyNotFoundValue)
        {
            if (value == null)
            {
                return null;
            }

            return mappings.TryGetValue(value, out var result) ? result : keyNotFoundValue;
        }
    }

    public interface IHashable<TKey, TValue>
    {
        TValue? this[TKey key] { get; }
    }

    public static class Hashable
    {
        public static bool TryCreate<TKey, TValue>(this object dictionary, out IHashable<TKey, TValue> hashable)
        {
            if (dictionary is IDictionary<TKey, TValue> d1)
            {
                hashable = new DictionaryImpl<TKey, TValue>(d1);
            }
            else if (dictionary is IReadOnlyDictionary<TKey, TValue> d2)
            {
                hashable = new ReadOnlyDictionaryImpl<TKey, TValue>(d2);
            }
            else if (dictionary is IDictionary d3 && typeof(TKey) == typeof(object) && typeof(TValue) == typeof(object))
            {
                hashable = (IHashable<TKey, TValue>)new DictionaryImpl(d3);
            }
            else
            {
                hashable = null!;
                return false;
            }

            return true;
        }

        private class DictionaryImpl<TKey, TValue> : IHashable<TKey, TValue>
        {
            public TValue this[TKey key] => Dictionary[key];

            public IDictionary<TKey, TValue> Dictionary { get; }

            public DictionaryImpl(IDictionary<TKey, TValue> dictionary)
            {
                Dictionary = dictionary;
            }
        }

        private class DictionaryImpl : IHashable<object, object>
        {
            public object? this[object key] => Dictionary[key];

            public IDictionary Dictionary { get; }

            public DictionaryImpl(IDictionary dictionary)
            {
                Dictionary = dictionary;
            }
        }

        private class ReadOnlyDictionaryImpl<TKey, TValue> : IHashable<TKey, TValue>
        {
            public TValue? this[TKey key] => Dictionary[key];

            public IReadOnlyDictionary<TKey, TValue> Dictionary { get; }

            public ReadOnlyDictionaryImpl(IReadOnlyDictionary<TKey, TValue> dictionary)
            {
                Dictionary = dictionary;
            }
        }
    }

    [ContentProperty(nameof(Mappings))]
    public class MappingDictionary : MappingDictionary<object, object>
    {
        public ICollection<Mapping<object, object>> Mappings => this;
    }

    public class MappingDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ICollection<Mapping<TKey, TValue>>
        where TKey : notnull
    {
        public bool IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)this).IsReadOnly;

        public void Add(Mapping<TKey, TValue> item) => Add(item.Key, item.Value);

        public bool Contains(Mapping<TKey, TValue> item) => TryGetValue(item.Key, out var value) && Equals(item.Value, value);

        public void CopyTo(Mapping<TKey, TValue>[] array, int arrayIndex) => ((IDictionary<TKey, TValue>)this).CopyTo(array.Select(mapping => new KeyValuePair<TKey, TValue>(mapping.Key, mapping.Value)).ToArray(), arrayIndex);

        public bool Remove(Mapping<TKey, TValue> item) => Remove(item.Key);

        IEnumerator<Mapping<TKey, TValue>> IEnumerable<Mapping<TKey, TValue>>.GetEnumerator()
        {
            foreach (var kvp in this)
            {
                yield return new Mapping<TKey, TValue>
                {
                    Key = kvp.Key,
                    Value = kvp.Value
                };
            }
        }
    }

    public class MappingDictionary<TKey, TValue, TKey1, TValue1> : Dictionary<TKey, TValue>, ICollection<Mapping<TKey1, TValue1>>
        where TKey : notnull
    {
        public bool IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)this).IsReadOnly;

        public void Add(Mapping<TKey1, TValue1> item) => ((IDictionary<TKey, TValue>)this).Add(Convert(item));

        public bool Contains(Mapping<TKey1, TValue1> item) => ((IDictionary<TKey, TValue>)this).Contains(Convert(item));

        public void CopyTo(Mapping<TKey1, TValue1>[] array, int arrayIndex) => ((IDictionary<TKey, TValue>)this).CopyTo(array.Select(Convert).ToArray(), arrayIndex);

        public bool Remove(Mapping<TKey1, TValue1> item) => ((IDictionary<TKey, TValue>)this).Remove(Convert(item));

        IEnumerator<Mapping<TKey1, TValue1>> IEnumerable<Mapping<TKey1, TValue1>>.GetEnumerator()
        {
            foreach (var kvp in this)
            {
                if (kvp.Key is TKey1 key && kvp.Value is TValue1 value)
                {
                    yield return new Mapping<TKey1, TValue1>
                    {
                        Key = key,
                        Value = value
                    };
                }
            }
        }

        private static KeyValuePair<TKey, TValue> Convert(Mapping<TKey1, TValue1> mapping)
        {
            if (mapping.Key is not TKey key) throw new ArgumentException($"Key {mapping.Key} must be of type {typeof(TKey)}");
            if (mapping.Value is not TValue value) throw new ArgumentException($"Key {mapping.Value} must be of type {typeof(TValue)}");

            return new KeyValuePair<TKey, TValue>(key, value);
        }
    }
}
