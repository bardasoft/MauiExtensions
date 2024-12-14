using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility
{
    public delegate void SelectedEventHandler(int selected);

    public class Selector : Frame
    {
        /*public static readonly BindableProperty SelectedIndicesProperty = BindableProperty.Create(nameof(SelectedIndices), typeof(int), typeof(Picker), -1, BindingMode.TwoWay, propertyChanged: OnSelectedIndexChanged, coerceValue: (bindable, value) => ((int)value).Bound(-1, (((Selector)bindable).Items?.Count - 1) ?? 1));

        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IList<string>), typeof(Picker), default(IList<string>), propertyChanged: OnItemsSourceChanged);

        public static readonly BindableProperty SelectedItemsProperty = BindableProperty.Create(nameof(SelectedItems), typeof(object), typeof(Picker), null, BindingMode.TwoWay, propertyChanged: OnSelectedItemChanged);

        public IList<string> ItemsSource
        {
            get => (IList<string>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        //public IList<string> Items { get; } = new ObservableCollection<string>();

        public int SelectedIndices
        {
            get => (int)GetValue(SelectedIndicesProperty);
            set => SetValue(SelectedIndicesProperty, value);
        }

        public object SelectedItems
        {
            get => GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }*/

        public event SelectedEventHandler Selected;

        public static BindableProperty SelectedColorProperty = BindableProperty.Create("SelectedColor", typeof(Color), typeof(Selector));

        public Color SelectedColor
        {
            get => (Color)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        public int SelectedIndex = -1;

        private IList<View> Options => (Content as Layout<View>).Children;
        private Button selected;

        public Selector()
        {
            Content = new StackLayout
            {
                HorizontalOptions = LayoutOptions.EndAndExpand,
                Orientation = StackOrientation.Horizontal,
                Spacing = 0,
            };
            Padding = new Thickness(0);
            CornerRadius = 5;
            IsClippedToBounds = true;
            HasShadow = false;
            this.SetBinding(BorderColorProperty, this, "SelectedColor");
        }

        public Selector(params string[] list) : this()
        {
            foreach (string str in list)
            {
                Options.Add(CreateControl(str));
            }
        }

        public void Select(int index) => Select(Options[index] as Button);

        private void Deselect(Button b)
        {
            b.SetBinding(Button.TextColorProperty, this, "SelectedColor");
            b.BackgroundColor = Colors.Transparent;
        }

        private void Select(Button b)
        {
            if (selected != null)
            {
                Deselect(selected);
            }

            b.SetBinding(Button.TextColorProperty, this, "BackgroundColor");
            b.SetBinding(BackgroundColorProperty, this, "SelectedColor");

            SelectedIndex = Options.IndexOf(selected = b);
            Selected?.Invoke(SelectedIndex);
        }

        private View CreateControl(string value)
        {
            Button button = new Button
            {
                Text = value,
                BorderWidth = 0,
                CornerRadius = 0,
                Padding = new Thickness(10),
            };
            button.Clicked += (sender, e) => Select((Button)sender);

            Deselect(button);

            return button;
        }

        /*private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            Selector selector = (Selector)bindable;
            IList<string> value = (IList<string>)newValue;

            selector.Children.Clear();
            foreach (string str in value)
            {
                selector.Children.Add(selector.CreateControl(str));
            }

            if (newValue is INotifyCollectionChanged collection)
            {
                collection.CollectionChanged += (sender, e) =>
                {
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        selector.Children.RemoveAt(e.OldStartingIndex);
                    }
                    
                    for (int i = e.NewItems.Count - 1; i >= 0; i--)
                    {
                        selector.Children.Insert(e.NewStartingIndex, selector.CreateControl((string)e.NewItems[i]));
                    }
                };
            }
        }*/
    }
}
