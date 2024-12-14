using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Original = Microsoft.Maui.Controls.ContentPage;

namespace Microsoft.Maui.Controls.Extensions
{
    public static class ContentPage
    {
        public static readonly BindableProperty ContentTemplateProperty = BindableProperty.CreateAttached("ContentTemplate", typeof(DataTemplate), typeof(ContentPage), null, propertyChanged: (b, o, n) =>
        {
            Original contentView = (Original)b;
            contentView.Content = ((DataTemplate)n).CreateContent() as View;
        });

        public static DataTemplate GetContentTemplate(Original content) => (DataTemplate)content.GetValue(ContentTemplateProperty);
        public static void SetContentTemplate(Original content, DataTemplate template) => content.SetValue(ContentTemplateProperty, template);
    }
}
