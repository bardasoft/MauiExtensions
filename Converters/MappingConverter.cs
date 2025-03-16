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

    [ContentProperty(nameof(Mappings))]
    public class MappingConverter<TKey, TValue> : IValueConverter<TKey, TValue>
        where TKey : notnull
    {
        public ICollection<Mapping<object, object>> Mappings => _Mappings;

        private MappingDictionary<TKey, TValue, object, object> _Mappings = new MappingDictionary<TKey, TValue, object, object>();

        public object? Convert(TKey? value, Type targetType, object? parameter, CultureInfo culture) => value != null && _Mappings.TryGetValue(value, out var result) ? result : null;

        public object? ConvertBack(TValue? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MappingDictionary<TKey, TValue> : MappingDictionary<TKey, TValue, TKey, TValue> where TKey : notnull { }

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
