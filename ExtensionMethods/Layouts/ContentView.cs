using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Target = Microsoft.Maui.Controls.ContentView;

namespace Microsoft.Maui.Controls.Extensions
{
    public class ViewFromTemplate<TDeclaring> where TDeclaring : BindableObject
    {
        public readonly BindableProperty ItemTemplateProperty;
        public readonly BindableProperty ItemSourceProperty;
        public readonly BindableProperty EmptyViewProperty;

        private Action<TDeclaring, View> SetContent;

        public ViewFromTemplate(Action<TDeclaring, View> setContent, string templatePropertyName = "ContentTemplate", string itemPropertyName = "ItemSource", string emptyPropertyName = "EmptyView")
        {
            ItemTemplateProperty = BindableProperty.CreateAttached(templatePropertyName, typeof(ElementTemplate), typeof(TDeclaring), null, propertyChanged: (b, o, n) => UpdateContent((TDeclaring)b, (ElementTemplate)n));
            ItemSourceProperty = BindableProperty.CreateAttached(itemPropertyName, typeof(object), typeof(TDeclaring), null, propertyChanged: (b, o, n) => UpdateContent((TDeclaring)b, item: n));
            EmptyViewProperty = BindableProperty.CreateAttached(emptyPropertyName, typeof(object), typeof(TDeclaring), null, propertyChanged: (b, o, n) => UpdateContent((TDeclaring)b, emptyView: n));

            SetContent = setContent;
        }

        private void UpdateContent(TDeclaring bindable, ElementTemplate template = null, object item = null, object emptyView = null)
        {
            //contentView.Content = (template is DataTemplateSelector selector && item != null ? selector.SelectTemplate(item, contentView) : template)?.CreateContent() as View;

            if (template == null)
            {
                template = GetItemTemplate(bindable);
            }
            if (item == null)
            {
                item = GetItemSource(bindable);
            }
            if (emptyView == null)
            {
                emptyView = GetEmptyView(bindable);
            }

            if (item != null)
            {
                if (template is DataTemplateSelector selector)
                {
                    template = selector.SelectTemplate(item, bindable);
                }

                View view = item as View;

                if (view == null && template?.CreateContent() is View content)
                {
                    content.BindingContext = item;
                    view = content;
                }
                else
                {
                    return;
                    throw new Exception("Could not convert " + item + " to a view");
                }

                SetContent(bindable, view);
            }
            else if (emptyView != null)
            {
                SetContent(bindable, emptyView as View ?? (emptyView as ElementTemplate)?.CreateContent() as View ?? new Label
                {
                    Text = emptyView?.ToString(),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                });
            }
            else
            {
                SetContent(bindable, null);
            }
        }

        public ElementTemplate GetItemTemplate(TDeclaring bindable) => (ElementTemplate)bindable.GetValue(ItemTemplateProperty);
        public object GetItemSource(TDeclaring bindable) => bindable.GetValue(ItemSourceProperty);
        public object GetEmptyView(TDeclaring bindable) => bindable.GetValue(EmptyViewProperty);

        public void SetItemTemplate(TDeclaring bindable, ElementTemplate value) => bindable.SetValue(ItemTemplateProperty, value);
        public void SetItemSource(TDeclaring bindable, object value) => bindable.SetValue(ItemSourceProperty, value);
        public void SetEmptyView(TDeclaring bindable, object value) => bindable.SetValue(EmptyViewProperty, value);
    }

    public static class ContentView
    {
        private static readonly ViewFromTemplate<Target> Items = new ViewFromTemplate<Target>((contentView, view) => contentView.Content = view);

        public static readonly BindableProperty ContentTemplateProperty = Items.ItemTemplateProperty;
        public static readonly BindableProperty ItemSourceProperty = Items.ItemSourceProperty;
        public static readonly BindableProperty EmptyViewProperty = Items.EmptyViewProperty;

        public static ElementTemplate GetContentTemplate(this Target contentView) => Items.GetItemTemplate(contentView);
        public static object GetItemSource(this Target contentView) => Items.GetItemSource(contentView);
        public static object GetEmptyView(this Target contentView) => Items.GetEmptyView(contentView);

        public static void SetContentTemplate(this Target contentView, ElementTemplate value) => Items.SetItemTemplate(contentView, value);
        public static void SetItemSource(this Target contentView, object value) => Items.SetItemSource(contentView, value);
        public static void SetEmptyView(this Target contentView, object value) => Items.SetEmptyView(contentView, value);
    }
}
