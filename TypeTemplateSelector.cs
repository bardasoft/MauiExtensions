namespace Microsoft.Maui.Controls
{
    //[ContentProperty(nameof(Template))]
    public class TypeDataTemplate : Mapping<Type, DataTemplate>
    {
        public Type Type
        {
            get => Key;
            set => Key = value;
        }
        public DataTemplate Template
        {
            get => Value;
            set => Value = value;
        }
    }

    //[ContentProperty(nameof(Templates))]
    public class DictionaryTemplateSelector : DataTemplateSelector
    {
        public IDictionary<Type, DataTemplate> Templates { get; } = new Dictionary<Type, DataTemplate>();

        public DataTemplate DefaultTemplate { get; set; } = ObjectTemplate;
        public static DataTemplate ObjectTemplate { get; set; } = new DataTemplate(() =>
        {
            var label = new Label();
            label.SetBinding(Label.TextProperty, ".");
            return label;
        });

        public bool TryGetTemplate(Type type, out DataTemplate result)
        {
            foreach (var kvp in Templates)
            {
                //if (item?.GetType().IsAssignableFrom(template.Type) == true)

            }

            result = null;
            return false;
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item != null && TryGetTemplate(GetType(item), out var template))
            {
                return template;
            }

            return DefaultTemplate is DataTemplateSelector selector ? selector.SelectTemplate(item, container) : DefaultTemplate;
        }

        protected virtual Type GetType(object item) => item.GetType();
    }

    public class TypeDictionary : Dictionary<Type, DataTemplate>, ICollection<TypeDataTemplate>
    {
        public bool IsReadOnly => false;

        public void Add(TypeDataTemplate item) => Add(item.Type, item.Template);

        public bool Contains(TypeDataTemplate item) => ((ICollection<KeyValuePair<Type, DataTemplate>>)this).Contains(new KeyValuePair<Type, DataTemplate>(item.Type, item.Template));

        public void CopyTo(TypeDataTemplate[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(TypeDataTemplate item) => ((ICollection<KeyValuePair<Type, DataTemplate>>)this).Remove(new KeyValuePair<Type, DataTemplate>(item.Type, item.Template));

        IEnumerator<TypeDataTemplate> IEnumerable<TypeDataTemplate>.GetEnumerator() => System.Linq.Enumerable.Select<KeyValuePair<Type, DataTemplate>, TypeDataTemplate>(this, kvp => new TypeDataTemplate { Type = kvp.Key, Template = kvp.Value }).GetEnumerator();
    }

    [ContentProperty(nameof(Templates))]
    public class TypeTemplateSelector : DataTemplateSelector
    {
        public ICollection<TypeDataTemplate> Templates => _Templates;
        private TypeDictionary _Templates = new TypeDictionary();

        public DataTemplate DefaultTemplate { get; set; } = ObjectTemplate;
        public static DataTemplate ObjectTemplate { get; set; } = new DataTemplate(() =>
        {
            var label = new Label();
            label.SetBinding(Label.TextProperty, ".");
            return label;
        });

        public bool TryGetTemplate(Type type, out DataTemplate result)
        {
            if (_Templates.TryGetValue(type, out result))
            {
                return true;
            }

            foreach (var kvp in _Templates)
            {
                //if (item?.GetType().IsAssignableFrom(template.Type) == true)
                if (kvp.Key.IsAssignableFrom(type))
                //if (type?.IsAssignableFrom(template.Type) == true)
                {
                    result = kvp.Value;
                    return true;
                }
            }

            result = null;
            return false;
        }

        /*public bool TryGetTemplate(Type type, out DataTemplate result)
        {

            foreach (TypeDataTemplate template in Templates)
            {
                //if (item?.GetType().IsAssignableFrom(template.Type) == true)
                if (template.Type.IsAssignableFrom(type))
                //if (type?.IsAssignableFrom(template.Type) == true)
                {
                    result = template.Template;
                    return true;
                }
            }

            result = null;
            return false;
        }*/

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item != null && TryGetTemplate(GetType(item), out var template))
            {
                return template;
            }

            return DefaultTemplate is DataTemplateSelector selector ? selector.SelectTemplate(item, container) : DefaultTemplate;
        }

        protected virtual Type GetType(object item) => item.GetType();
    }

    public class TemplateMapping<TKey> : Mapping<TKey, DataTemplate> { }
    public class TemplateMapping : TemplateMapping<object> { }

    [ContentProperty(nameof(Mappings))]
    public abstract class MappingDataTemplateSelector<TKey> : DataTemplateSelector
        where TKey : notnull
    {
        public ICollection<Mapping<object, DataTemplate>> Mappings => _Mappings;

        protected MappingDictionary<object, DataTemplate> _Mappings = new MappingDictionary<object, DataTemplate>();

        public DataTemplate DefaultTemplate { get; set; } = ObjectTemplate;
        public static DataTemplate ObjectTemplate { get; set; } = new DataTemplate(() =>
        {
            var label = new Label();
            label.SetBinding(Label.TextProperty, ".");
            return label;
        });

        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            if (item == null || _Mappings.TryGetValue(GetMappingProperty(item), out var template))
            {
                template = DefaultTemplate;
            }

            return template is DataTemplateSelector selector ? selector.SelectTemplate(item, container) : template;
        }

        protected abstract TKey GetMappingProperty(object item);
    }

    public class MappingDataTemplateSelector : MappingDataTemplateSelector<object>
    {
        protected override object GetMappingProperty(object item) => item;
    }

    public class AutoDataTemplateSelector : MappingDataTemplateSelector<Type>
    {
        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            var template = base.OnSelectTemplate(item, container);
            if (template == DefaultTemplate)
            {
                foreach (var kvp in _Mappings)
                {
                    if (kvp.Key is Type type && type?.IsAssignableFrom(item.GetType()) == true)
                    {
                        return kvp.Value;
                    }
                }
            }

            return template;
        }

        protected override Type GetMappingProperty(object item) => item.GetType();
    }
}
