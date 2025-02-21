using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;
using MauiItemsLayout = Microsoft.Maui.Controls.ItemsLayout;

namespace Microsoft.Maui.Controls
{
    
}

namespace Microsoft.Maui.Controls.Extensions
{
    public static class ItemsLayout
    {
        public static readonly BindableProperty ItemSizeProperty = BindableProperty.CreateAttached(nameof(GetItemSize).Substring(3), typeof(string), typeof(MauiItemsLayout), null);//, propertyChanged: (bindable, oldValue, newValue) => ((RowDefinition)bindable).OnSizeChanged());

        //[System.ComponentModel.TypeConverter(typeof(GridLengthTypeConverter))]
        public static string GetItemSize(this MauiItemsLayout bindable) => (string)bindable.GetValue(ItemSizeProperty);
        public static GridLength GetItemSizeGridLength(this MauiItemsLayout bindable) => GetItemSize(bindable) is string str ? (GridLength)new GridLengthTypeConverter().ConvertFrom(null, null, str) : GridLength.Auto;
        public static void SetItemSize(this MauiItemsLayout bindable, string value) => bindable.SetValue(ItemSizeProperty, value);
    }

    public class CollectionViewLayoutBehavior : Behavior<CollectionView>
    {
        protected override void OnAttachedTo(CollectionView bindable)
        {
            base.OnAttachedTo(bindable);
            
            bindable.PropertyChanged += ItemTemplatePropertyChanged;
            UpdateItemTemplate(bindable);

            bindable.Behaviors.Remove(this);
        }

        private void ItemTemplatePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ItemsView.ItemTemplateProperty.PropertyName || e.PropertyName == StructuredItemsView.ItemsLayoutProperty.PropertyName)
            {
                UpdateItemTemplate((CollectionView)sender!);
            }
        }

        private static BindableProperty ItemLayoutTemplateProperty = BindableProperty.Create("Changeling" + ItemsView.ItemTemplateProperty.PropertyName, typeof(ItemLayoutDataTemplateSelector), typeof(CollectionViewLayoutBehavior));

        private static ItemLayoutDataTemplateSelector GetItemLayoutTemplate(CollectionView collectionView) => (ItemLayoutDataTemplateSelector)collectionView.GetValue(ItemLayoutTemplateProperty);

        private async void UpdateItemTemplate(CollectionView collectionView)
        {
            var layout = collectionView.ItemsLayout;
            if (false == collectionView.ItemsLayout is ICollectionViewLayoutManager layoutManager)
            {
                return;
            }

            try
            {
                await ChangelingTemplate.Replace<DataTemplate>(collectionView, ItemsView.ItemTemplateProperty, ItemLayoutTemplateProperty, template => new CollectionViewLayoutDataTemplateSelector(template, layoutManager));
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
        }
    }

    public interface ICollectionViewLayoutManager
    {
        LayoutInfo Layout(object item);
    }

    public class LinearItemsLayout : Maui.Controls.LinearItemsLayout, ICollectionViewLayoutManager
    {
        public static readonly BindableProperty ItemSizeProperty = BindableProperty.Create(nameof(ItemSize), typeof(GridLength), typeof(LinearItemsLayout), GridLength.Auto);//, propertyChanged: (bindable, oldValue, newValue) => ((RowDefinition)bindable).OnSizeChanged());

        [System.ComponentModel.TypeConverter(typeof(GridLengthTypeConverter))]
        public GridLength ItemSize
        {
            get => (GridLength)GetValue(ItemSizeProperty);
            set => SetValue(ItemSizeProperty, value);
        }

        public LinearItemsLayout(ItemsLayoutOrientation orientation) : base(orientation) { }

        public LayoutInfo Layout(object item)
        {
            var result = new LayoutInfo();

            if (Orientation == ItemsLayoutOrientation.Horizontal)
            {
                result.Width = ItemSize;
            }
            else if (Orientation == ItemsLayoutOrientation.Vertical)
            {
                result.Height = ItemSize;
            }

            return result;
        }
    }

    public class VerticalItemsLayout() : LinearItemsLayout(ItemsLayoutOrientation.Vertical) { }
    public class HorizontalItemsLayout() : LinearItemsLayout(ItemsLayoutOrientation.Horizontal) { }

    public class GridItemsDefinitionLayout : GridItemsLayout, ICollectionViewLayoutManager
    {
        public static readonly BindableProperty SpanDefinitionsProperty = BindableProperty.Create(nameof(SpanDefinitions), typeof(RowDefinitionCollection), typeof(GridItemsDefinitionLayout), null, validateValue: (bindable, value) => value != null, defaultValueCreator: bindable => new RowDefinitionCollection());

        [System.ComponentModel.TypeConverter(typeof(RowDefinitionCollectionTypeConverter))]
        public RowDefinitionCollection SpanDefinitions
        {
            get => (RowDefinitionCollection)GetValue(SpanDefinitionsProperty);
            set => SetValue(SpanDefinitionsProperty, value);
        }

        new public int Span => SpanDefinitions.Count;

        public GridItemsDefinitionLayout(ItemsLayoutOrientation orientation) : base(orientation) { }

        public GridItemsDefinitionLayout(int span, ItemsLayoutOrientation orientation) : base(span, orientation) { }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == SpanDefinitionsProperty.PropertyName)
            {
                base.Span = SpanDefinitions.Count;
            }
        }

        public LayoutInfo Layout(object item)
        {
            if (SpanDefinitions.Count > 1)
            {
                throw new NotImplementedException();
            }

            return new LayoutInfo
            {
                //Height = SpanDefinitions[0].Height,
                //Width = SpanDefinitions[0].Height,
            };
        }
    }

    public class LayoutInfo
    {
        public GridLength? Width { get; set; } = null;
        public GridLength? Height { get; set; } = null;

        public override bool Equals(object? obj)
        {
            return obj is LayoutInfo info &&
                   EqualityComparer<GridLength?>.Default.Equals(Width, info.Width) &&
                   EqualityComparer<GridLength?>.Default.Equals(Height, info.Height);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Width, Height);
        }
    }

    public static class ChangelingTemplate
    {
        public static async Task Replace<T>(BindableObject bindable, BindableProperty property, BindableProperty changelingProperty, Func<T, T> replacementFunc) where T : ElementTemplate
        {
            Print.Log(bindable.GetHashCode());
            var template = (T)bindable.GetValue(property);
            if (template == null)
            {
                return;
            }

            var changelingTemplate = (ElementTemplate)bindable.GetValue(changelingProperty);
            if (changelingTemplate != null && await TestPresence(changelingTemplate, bindable))
            {
                return;
            }

            changelingTemplate = replacementFunc(template);

            bindable.SetValue(changelingProperty, changelingTemplate);
            bindable.SetValue(property, changelingTemplate);
        }

        private static IDictionary<ElementTemplate, ObserverSemaphore> ActivelyTestingInstances = new Dictionary<ElementTemplate, ObserverSemaphore>();

        public static Task<bool> TestPresence(ElementTemplate template, BindableObject container) => Task.Factory.StartNew(() =>
        {
            ObserverSemaphore semaphore;
            lock (ActivelyTestingInstances)
            {
                if (!ActivelyTestingInstances.TryGetValue(template, out semaphore))
                {
                    ActivelyTestingInstances.Add(template, semaphore = new ObserverSemaphore());
                }

                semaphore.EnterOne();
            }

            try
            {
                lock (semaphore)
                {
                    try
                    {
                        ElementTemplate testTemplate = template is DataTemplateSelector selector ? selector.SelectTemplate("test", container) : template;
                        testTemplate?.CreateContent();
                    }
                    catch (DummyTestException)
                    {
                        return true;
                    }
                    catch { }
                }
            }
            finally
            {
                lock (ActivelyTestingInstances)
                {
                    if (semaphore.Release() == 0)
                    {
                        ActivelyTestingInstances.Remove(template);
                    }
                }
            }

            return false;
        });

        public static void EnableDetection(ElementTemplate instance)
        {
            // Should not be doing this test on the main thread
            if (MainThread.IsMainThread)
            {
                return;
            }

            ObserverSemaphore semaphore;
            lock (ActivelyTestingInstances)
            {
                if (!ActivelyTestingInstances.TryGetValue(instance, out semaphore))
                {
                    return;
                }
            }

            lock (semaphore)
            {
                throw new DummyTestException();
            }
        }

        private class DummyTestException : Exception { }

        private class ObserverSemaphore
        {
            private int Count;
            private object CountLock = new object();

            public void EnterOne()
            {
                lock (CountLock)
                {
                    Count++;
                }
            }

            public int Release()
            {
                lock (CountLock)
                {
                    return --Count;
                }
            }
        }
    }

    public abstract class ItemLayoutDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Template { get; set; }
        private Dictionary<(LayoutInfo, DataTemplate), DataTemplate> TemplateCache { get; } = new Dictionary<(LayoutInfo, DataTemplate), DataTemplate>();

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            ChangelingTemplate.EnableDetection(this);

            var layout = Layout(item);//, new Lazy<int>(() => ComputeIndex(container, item)));
            var baseTemplate = Template is DataTemplateSelector selector ? selector.SelectTemplate(item, container) : Template;
            var key = (layout, baseTemplate);

            if (!TemplateCache.TryGetValue(key, out var template))
            {
                TemplateCache.Add(key, template = new DataTemplate(() =>
                {
                    var content = baseTemplate?.CreateContent();
                    Controls.VisualElement? visualElement;

                    if (content is View view)
                    {
                        var contentView = new Maui.Controls.ContentView { Content = view };
                        CompressedLayout.SetIsHeadless(contentView, true);

                        visualElement = contentView;
                    }
                    else
                    {
                        visualElement = content as Controls.VisualElement;
                    }

                    if (visualElement != null)
                    {
                        if (layout.Width.HasValue)
                        {
                            SetSizeProperty(visualElement, true, layout.Width.Value);
                        }
                        if (layout.Height.HasValue)
                        {
                            SetSizeProperty(visualElement, false, layout.Height.Value);
                        }

                        content = visualElement;
                    }

                    return content;
                }));
            }

            return template;
        }

        private void SetSizeProperty(BindableObject bindable, bool width, GridLength length)
        {
            var property = width ? Controls.VisualElement.WidthRequestProperty : Controls.VisualElement.HeightRequestProperty;

            if (length.IsAuto)
            {
                bindable.ClearValue(property);
            }
            else if (length.IsStar)
            {
                var path = width ? Controls.VisualElement.HeightProperty.PropertyName : Controls.VisualElement.WidthProperty.PropertyName;
                var converter = length.Value == 1 ? null : new ScaleConverter(length.Value);

                bindable.SetBinding(property, new RelativeBindingSource(RelativeBindingSourceMode.Self), path, value =>
                {
                    return (double)value * length.Value;
                });
            }
            else if (length.IsAbsolute)
            {
                bindable.SetValue(property, length.Value);
            }
        }

        private class ScaleConverter : IValueConverter<double, double>
        {
            public double Scale { get; }

            public ScaleConverter(double scale)
            {
                Scale = scale;
            }

            public object? Convert(double value, Type targetType, object parameter, CultureInfo culture) => value * Scale;

            public object? ConvertBack(double value, Type targetType, object parameter, CultureInfo culture) => value / Scale;
        }

        protected abstract LayoutInfo Layout(object item);

        private int ComputeIndex(BindableObject container, object item)
        {
            IEnumerable source;
            if (container is CollectionView collectionView)
            {
                source = collectionView.ItemsSource;
            }
            else
            {
                return -1;
            }

            var itr = source.GetEnumerator();
            for (int i = 0; itr.MoveNext(); i++)
            {
                if (Equals(i, item))
                {
                    return i;
                }
            }

            return -1;
        }
    }

    public class CollectionViewLayoutDataTemplateSelector : ItemLayoutDataTemplateSelector
    {
        public ICollectionViewLayoutManager LayoutManager { get; }

        public CollectionViewLayoutDataTemplateSelector(DataTemplate template, ICollectionViewLayoutManager layoutManager)
        {
            Template = template;
            LayoutManager = layoutManager;
        }

        protected override LayoutInfo Layout(object item) => LayoutManager.Layout(item);
    }

    public class UniformLayoutExtension : IMarkupExtension<UniformLayoutDataTemplateSelector>
    {
        public DataTemplate Template { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }

        public UniformLayoutDataTemplateSelector ProvideValue(IServiceProvider serviceProvider) => new UniformLayoutDataTemplateSelector
        {
            Template = Template,
            LayoutInfo = new LayoutInfo
            {
                Width = Width,
                Height = Height
            }
        };

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
    }

    public class UniformLayoutDataTemplateSelector : ItemLayoutDataTemplateSelector
    {
        public LayoutInfo LayoutInfo { get; set; }

        protected override LayoutInfo Layout(object item) => LayoutInfo;
    }
}
