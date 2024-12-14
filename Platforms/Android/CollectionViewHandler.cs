using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Controls.Handlers.Items;

namespace MauiExtensions
{
    public partial class CollectionViewHandler : Microsoft.Maui.Controls.Handlers.Items.CollectionViewHandler
    {
        protected override RecyclerView CreatePlatformView() =>
            new Platforms.Android.MauiRecyclerView<ReorderableItemsView, GroupableItemsViewAdapter<ReorderableItemsView, IGroupableItemsViewSource>, IGroupableItemsViewSource>(Context, GetItemsLayout, CreateAdapter);
    }
}
