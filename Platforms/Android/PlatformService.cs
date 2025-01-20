using Android.Content;
using Android.Content.Res;
using Android.Hardware.Lights;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat;
using Microsoft.Maui.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiExtensions.Services
{
    public partial class PlatformService
    {
        private static double? ToolBarHeight;

        internal static partial double GetNavBarHeight()
        {
            if (Shell.Current?.CurrentPage == null)
            {
                return -1;
            }
            if (ToolBarHeight.HasValue)
            {
                return ToolBarHeight.Value;
            }

            //var test = Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat.NavigationPage.GetBarHeight(Shell.Current.CurrentPage);
            Resources resources = Android.App.Application.Context.Resources;
            int resourceId = resources.GetIdentifier("navigation_bar_height", "dimen", "android");
            if (resourceId > 0)
            {
                ToolBarHeight = Android.App.Application.Context.FromPixels(resources.GetDimensionPixelSize(resourceId));
            }

            return ToolBarHeight ?? -1;
        }
    }
}
