using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
    public enum AdSizes { Undefined, Banner, MediumRectangle }

    public class AdView : View
    {
        public static readonly Size BannerAdSize = new Size(-1, 0);
        public static readonly double InlineBannerHeight = -2;

        public static readonly BindableProperty AdUnitIDProperty = BindableProperty.Create(nameof(AdUnitID), typeof(string), typeof(AdView));

        public static readonly BindableProperty AdSizeProperty = BindableProperty.Create(nameof(AdSize), typeof(AdSizes), typeof(AdView));

        public string AdUnitID
        {
            get => (string)GetValue(AdUnitIDProperty);
            set => SetValue(AdUnitIDProperty, value);
        }

        public AdSizes AdSize
        {
            get => (AdSizes)GetValue(AdSizeProperty);
            set => SetValue(AdSizeProperty, value);
        }
    }

    public class AdInfo
    {
        public int Frequency { get; set; }
        public DataTemplate Template { get; set; }
    }

    public static class Ads
    {
        private static readonly object AdObject = new object();

        private static void Clean(ItemsView itemsView, int? frequency)
        {
            var list = itemsView.ItemsSource as IList ?? itemsView.ItemsSource?.OfType<object>().ToList();

            bool changed = false;

            for (int i = 0; i < (list?.Count ?? 0); i++)
            {
                if (list[i] == AdObject)
                {
                    list.RemoveAt(i--);
                }
                else if (frequency.HasValue && i > 0 && i % frequency.Value == 0)
                {
                    list.Insert(i, AdObject);
                }
                else
                {
                    continue;
                }

                changed = true;
            }

            if (changed && !(itemsView.ItemsSource is INotifyCollectionChanged))
            {
                itemsView.ItemsSource = list;
            }
        }

        public static readonly BindableProperty CollectionAdsProperty = BindableProperty.CreateAttached("CollectionAds", typeof(AdInfo), typeof(ItemsView), null, propertyChanged: (bindable, oldValue, newValue) =>
        {
            return;
            var itemsView = (ItemsView)bindable;
            var oldInfo = (AdInfo)oldValue;
            var info = (AdInfo)newValue;

            if (itemsView?.ItemTemplate is AdTemplateSelector ads)
            {
                ads.AdTemplate = info.Template;
            }

            if (oldInfo == null || info == null && oldInfo.Frequency != info.Frequency)
            {
                Clean(itemsView, info?.Frequency);
            }
        }, defaultValueCreator: bindable =>
        {
            return null;
            ObservableCollection<object> items = null;
            ICommand command = null;
            int lastAdIndex = 0;

            bindable.PropertyChanged += PropertyChanged;

            PropertyChanged(bindable, new PropertyChangedEventArgs(ItemsView.RemainingItemsThresholdReachedCommandProperty.PropertyName));
            PropertyChanged(bindable, new PropertyChangedEventArgs(ItemsView.ItemTemplateProperty.PropertyName));

            void PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                var itemsView = (ItemsView)sender;

                if (e.PropertyName == ItemsView.ItemsSourceProperty.PropertyName)
                {
                    if (itemsView.IsSet(CollectionAdsProperty))
                    {
                        Clean(itemsView, itemsView.GetCollectionAds().Frequency);
                    }
                    if (itemsView.ItemsSource is INotifyCollectionChanged observable)
                    {
                        observable.CollectionChanged += (sender1, e1) =>
                        {
                            if (e1.Action == NotifyCollectionChangedAction.Reset)
                            {
                                lastAdIndex = 0;
                            }
                        };
                    }
                }
                else if (e.PropertyName == ItemsView.RemainingItemsThresholdReachedCommandProperty.PropertyName)
                {
                    if (itemsView.RemainingItemsThresholdReachedCommand != command)
                    {
                        var original = itemsView.RemainingItemsThresholdReachedCommand;
                        itemsView.RemainingItemsThresholdReachedCommand = command = new Command<object>(parameter =>
                        {
                            original?.Execute(parameter);

                            //if (items.Count % itemsView.GetAdFrequency() == 0)
                            var ads = itemsView.GetCollectionAds();
                            var list = itemsView.ItemsSource as IList;

                            if (ads != null && list != null && list.Count - lastAdIndex > ads.Frequency)
                            {
                                try
                                {
                                    list.Insert(lastAdIndex + ads.Frequency, AdObject);
                                }
                                catch { }

                                lastAdIndex += ads.Frequency;
                            }
                        });
                    }
                }
                else if (e.PropertyName == ItemsView.ItemTemplateProperty.PropertyName)
                {
                    if (!(itemsView.ItemTemplate is AdTemplateSelector))
                    {
                        var template = new AdTemplateSelector(itemsView.ItemTemplate);
                        itemsView.ItemTemplate = template;

                        if (itemsView.IsSet(CollectionAdsProperty))
                        {
                            template.AdTemplate = itemsView.GetCollectionAds()?.Template;
                        }
                    }
                }
                //else if (e.PropertyName == ItemsView.ItemsSourceProperty.PropertyName)
                else if (false)
                {
                    if (itemsView.ItemsSource != items)
                    {
                        items = new ObservableCollection<object>(itemsView.ItemsSource.OfType<object>());

                        if (itemsView.ItemsSource is INotifyCollectionChanged observable)
                        {
                            observable.CollectionChanged += (sender1, e1) =>
                            {
                                if (e1.NewItems != null)
                                {
                                    for (int i = 0; i < e1.NewItems.Count; i++)
                                    {
                                        items.Insert(e1.NewStartingIndex + i, e1.NewItems[i]);
                                    }
                                }

                                if (items.Count % itemsView.GetCollectionAds()?.Frequency == 0)
                                {
                                    items.Add(AdObject);
                                }
                            };
                        }
                    }
                }
            };

            return null;
        });

        private class AdTemplateSelector : DataTemplateSelector
        {
            public DataTemplate AdTemplate { get; set; }
            /*{
                get => _AdTemplate;
                set
                {
                    if (value != _AdTemplate)
                    {
                        _AdTemplate = value;
                        AdView = _AdTemplate?.CreateContent() as View;
                        CachedAdTemplate = new DataTemplate(() =>
                        {
                            if (AdView.Parent is ContentView contentView)
                            {
                                contentView.Content = null;
                            }

                            return new ContentView
                            {
                                Content = AdView
                            };
                        });
                    }
                }
            }*/
            private DataTemplate Original;

            private View AdView;
            private DataTemplate CachedAdTemplate;
            private DataTemplate _AdTemplate;

            public AdTemplateSelector(DataTemplate original)
            {
                Original = original;
            }

            protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
            {
                if (item == AdObject && AdTemplate != null)
                {
                    return AdTemplate;// CachedAdTemplate;
                }

                return (Original as DataTemplateSelector)?.SelectTemplate(item, container) ?? Original ?? TypeTemplateSelector.ObjectTemplate;
            }
        }

        public static AdInfo GetCollectionAds(this ItemsView bindable) => (AdInfo)bindable.GetValue(CollectionAdsProperty);
        public static void SetCollectionAds(this ItemsView bindable, int value) => bindable.SetValue(CollectionAdsProperty, value);
    }
}
