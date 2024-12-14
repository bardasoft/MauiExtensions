using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Controls
{
    public class MediaTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ImageTemplate { get; set; } = new DataTemplate(() =>
        {
            Image image = new Image();
            image.SetBinding(Image.SourceProperty, new Binding("."));
            return image;
        });

        public DataTemplate VideoTemplate { get; set; } = new DataTemplate(() =>
        {
            WebView video = new WebView();
            video.SetBinding(WebView.SourceProperty, new Binding(".", converter: new EmbeddedVideoConverter()));
            return video;
        });

        public List<string> ImageExts { get; } = new List<string> { "jpg", "jpeg", "png", "svg" };

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            string url = (item as UriImageSource)?.Uri.OriginalString ?? (item as FileImageSource)?.File ?? item as string;

            if (url == null)
            {
                return null;
            }

            foreach (string ext in ImageExts)
            {
                if (url.EndsWith("." + ext))
                {
                    return ImageTemplate;
                }
            }

            return VideoTemplate;
        }
    }
}
