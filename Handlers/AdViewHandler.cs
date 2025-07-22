using MauiExtensions.Handlers;

namespace Microsoft.Maui.Controls
{
    public static partial class AdExtensions
    {
        public static partial MauiAppBuilder UseAdmobAds(this MauiAppBuilder builder);
        public static MauiAppBuilder UseAdmobAds(this MauiAppBuilder builder, IEnumerable<string> keywords)
        {
            AdViewHandler.Keywords.AddRange(keywords);
            return UseAdmobAds(builder);
        }
    }
}

namespace MauiExtensions.Handlers
{
    public partial class AdViewHandler
    {
        public static IPropertyMapper<AdView, AdViewHandler> PropertyMapper = new PropertyMapper<AdView, AdViewHandler>(ViewMapper)
        {
            [nameof(AdView.AdUnitID)] = MapAdUnitId,
            [nameof(AdView.AdSize)] = MapAdSize,
        };

        public static CommandMapper<AdView, AdViewHandler> CommandMapper = new(ViewCommandMapper)
        {
        };

        public static readonly List<string> Keywords = new List<string>();

        public AdViewHandler() : base(PropertyMapper, CommandMapper)
        {
            
        }
    }
}
