using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Extensions;

namespace Microsoft.Maui.Controls.Compatibility
{
    public static class LayoutExtensions
    {
        public static Size UsableSpace(this Layout layout) => new Size(layout.Width - layout.Padding.Left - layout.Padding.Right - layout.Margin.Left - layout.Margin.Right, layout.Height - layout.Padding.Top - layout.Padding.Bottom - layout.Margin.Top - layout.Margin.Bottom);

        /// <summary>
        /// Executes <paramref name="action"/> when an element of or inherited from type <typeparamref name="T"/> is added to <paramref name="layout"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="layout"></param>
        /// <param name="action"></param>
        public static void WhenDescendantAdded<T>(this Layout layout, Action<T> action) where T : Element => layout.WhenDescendantAdded(action, (e) => e.Element is T);

        /// <summary>
        /// Executes <paramref name="action"/> when an element of type <typeparamref name="T"/> is added to <paramref name="layout"/>. <paramref name="action"/> will not be applied to types that inherit from <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="layout"></param>
        /// <param name="action"></param>
        public static void WhenExactDescendantAdded<T>(this Layout layout, Action<T> action) where T : Element => layout.WhenDescendantAdded(action, (e) => e.Element.GetType() == typeof(T));

        private static void WhenDescendantAdded<T>(this Layout layout, Action<T> action, Func<ElementEventArgs, bool> comparer) where T : Element
        {
            layout.DescendantAdded += new IfEventHandler<ElementEventArgs>(
                (sender, e) => comparer(e),// canBeDerivedType ? (e is T) : (e.GetType() == typeof(T)),
                (sender, e) => action((T)e.Element)
                );
        }

        public static IEnumerable<T> GetDescendants<T>(this Layout<View> layout)
            where T : View
        {
            foreach(View view in layout.Children)
            {
                if (view is T)
                {
                    yield return (T)view;
                }

                Layout<View> others = view as Layout<View> ?? (view as ContentView)?.Content as Layout<View> ?? (view as ScrollView)?.Content as Layout<View>;

                if (others != null)
                {
                    foreach (T t in others.GetDescendants<T>())
                    {
                        yield return t;
                    }
                }
            }
        }

        public static View GetViewAt(this Layout<View> layout, Point point)
        {
            Point temp;
            return layout.GetViewAt(point, out temp);
        }

        public static View GetViewAt(this Layout<View> layout, Point point, out Point scaled)
        {
            //int i = 0;
            //for (; i < layout.Children.Count; i++)
            foreach(View child in layout.Children)
            {
                //View child = layout.Children[i];

                //Is the point inside the bounds that this child occupies?
                //if (pos.X >= child.X && pos.X <= child.X + child.Width && pos.Y >= child.Y && pos.Y <= child.Y + child.Height)
                if (child.Bounds.Contains(point))
                {
                    point = point.Subtract(child.Bounds.Location);

                    if (child is Layout<View>)
                    {
                        return GetViewAt(child as Layout<View>, point, out scaled);
                    }
                    else
                    {
                        scaled = point;
                        return child;
                    }

                    /*else if (parent.Editable())
                    {
                        ans = child;
                    }*/
                    
                    //break;
                }
            }

            /*Expression e = parent as Expression;
            if (i == parent.Children.Count && e != null && e.Editable && (pos.X <= e.PadLeft || pos.X >= e.Width - e.PadRight))
            {
                ans = parent;
            }*/

            scaled = point;
            return layout;
        }
    }
}
