using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
    public static class PageExtensions
    {
        public static BindableProperty SafeAreaInsetsProperty => Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SafeAreaInsetsProperty;

        public static Thickness MakeSafe(this Thickness padding, Page page)
        {
            Thickness safeInsets = PlatformConfiguration.iOSSpecific.Page.SafeAreaInsets(page.On<PlatformConfiguration.iOS>());

            return new Thickness(
                Math.Max(padding.Left, safeInsets.Left),
                Math.Max(padding.Top, safeInsets.Top),
                Math.Max(padding.Right, safeInsets.Right),
                Math.Max(padding.Bottom, safeInsets.Bottom));
        }
    }
}
