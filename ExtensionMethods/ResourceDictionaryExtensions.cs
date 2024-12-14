using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Extensions;
using System.Collections;

namespace Microsoft.Maui.Controls
{
    public class Style<T> : IEnumerable
    {
        private Style Value;

        public Style() : this(new Style(typeof(T))) { }

        public Style(Style style)
        {
            Value = style;
        }

        public void Add(Behavior behavior) => Value.Behaviors.Add(behavior);
        public void Add(Setter setter) => Value.Setters.Add(setter);
        public void Add(TriggerBase triggerBase) => Value.Triggers.Add(triggerBase);


        public static implicit operator Style(Style<T> style) => style.Value;

        public IEnumerator GetEnumerator()
        {
            foreach(Behavior behavior in Value.Behaviors)
            {
                yield return behavior;
            }
            foreach (Setter setter in Value.Setters)
            {
                yield return setter;
            }
            foreach (TriggerBase triggerBase in Value.Triggers)
            {
                yield return triggerBase;
            }
        }
    }
}

namespace Microsoft.Maui.Controls.Extensions
{
    public class StyleSetter<T> : IEnumerable<Setter>
    {
        public Style Style;

        public StyleSetter()
        {
            Style = new Style(typeof(T));
        }

        public static implicit operator Style(StyleSetter<T> style) => style.Style;

        public void Add(Setter setter) => Style.Setters.Add(setter);

        public IEnumerator<Setter> GetEnumerator()
        {
            foreach(Setter setter in Style.Setters)
            {
                yield return setter;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public static class ResourceDictionaryExtensions
    {
        /*public static ResourceDictionary Populate<T>(this ResourceDictionary dictionary, Action<T> behavior, params Setter[] setters)
            where T : BindableObject
        {
            dictionary.AddStyle<T>(behavior, setters);
            return dictionary;
        }

        public static ResourceDictionary Populate<T>(this ResourceDictionary dictionary, params Setter[] setters)
            where T : BindableObject
        {
            dictionary.AddStyle<T>(setters);
            return dictionary;
        }*/

        //public static Style AddStyle<T>(this ResourceDictionary dictionary, params Setter[] setters)
        //  where T : BindableObject //=> dictionary.AddStyle<T>(null, setters);

        public static Style AddBehavior<T>(this Style style, Action<T> behavior)
            where T : BindableObject
        {
            style.Behaviors.Add(new BehaviorFunc<T>(behavior));
            return style;
        }

        public static void AddBehavior<T>(this ResourceDictionary dictionary, Action<T> behavior) where T : BindableObject => dictionary.Add(new Style(typeof(T)).AddBehavior(behavior));

        //public static void AddStyle<T>(this ResourceDictionary dictionary, Action<T> behavior, bool applyToDerivedTypes = true) where T : BindableObject => dictionary.AddStyle<T>(new Style(typeof(T)) { ApplyToDerivedTypes = applyToDerivedTypes }, behavior);
    }
}
