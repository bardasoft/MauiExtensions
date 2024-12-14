using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Compatibility
{
    public static class ElementExtensions
    {
        public static BindableProperty ParentProperty = BindableProperty.CreateAttached("Parent", typeof(Element), typeof(Element), null, propertyChanged: (bindable, oldValue, newValue) =>
        {
            Element element = (Element)bindable;

            if (newValue == null)
            {
                element.Remove();
            }
            else
            {
                if (element is View view)
                {
                    if (newValue is Layout<View> layout)
                    {
                        layout.Children.Add(view);
                    }
                    else if (newValue is ContentView contentView)
                    {
                        contentView.Content = view;
                    }
                }
            }
        });

        public static bool Remove(this Element element)
        {
            if (element.Parent == null)
            {
                System.Diagnostics.Debug.WriteLine("Element has no parent");
                return false;
            }

            if (element.Parent is Layout<View> layout && element is View view)
            {
                layout.Children.Remove(view);
            }
            else if (element.Parent is ContentView contentView)
            {
                contentView.Content = null;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Unable to remove an objet of type " + element.GetType() + " from " + element.Parent.GetType());
                return false;
            }

            return true;
        }

        public static bool IsDescendantOf(this Element element, Element parent)
        {
            while (element != null)
            {
                if (element.Parent == parent)
                {
                    return true;
                }

                element = element.Parent;
            }

            return false;
        }

        public static T Root<T>(this Element e) where T : Element
        {
            T root = null;
            
            while ((e = e.Parent<T>()) != null)
            {
                root = e as T;
            }

            return root;
        }

        public static T Parent<T>(this Element e) where T : Element
        {
            do
            {
                e = e.Parent;

                if (e == null)
                {
                    return null;
                }
            }
            while (!(e is T));

            return (T)e;
        }

        public static bool HasParent(this Element element) => element.Parent != null;

        public static View ParentView(this Element element) => element.Parent<View>();
    }
}
