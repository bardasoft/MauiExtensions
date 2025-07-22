using CoreGraphics;
using Foundation;
using Google.MobileAds;
using Microsoft.Maui.Handlers;
using UIKit;
using iOSAdView = Google.MobileAds.BannerView;
using MauiAdView = Microsoft.Maui.Controls.AdView;

namespace Microsoft.Maui.Controls
{
    public static partial class AdExtensions
    {
        public static partial MauiAppBuilder UseAdmobAds(this MauiAppBuilder builder)
        {
            MobileAds.SharedInstance.Start(status => System.Diagnostics.Debug.WriteLine($"Admob initialized with status {status}"));

            return builder;
        }
    }
}

namespace MauiExtensions.Handlers
{
    public partial class AdViewHandler : ViewHandler<MauiAdView, iOSAdView>
    {
        public static void MapAdUnitId(AdViewHandler handler, MauiAdView adView)
        {
            handler.PlatformView.AdUnitId = adView.AdUnitID;
            Reload(handler.PlatformView);
        }

        public static void MapAdSize(AdViewHandler handler, MauiAdView adView)
        {
            handler.PlatformView.AdSize = GetAdSize(adView.AdSize, adView);

            if (adView.HeightRequest == MauiAdView.InlineBannerHeight)
            {
                // Wire AdReceived event to know when the Ad is ready to be displayed
                handler.PlatformView.AdReceived += (sender, e) =>
                {
                    adView.HeightRequest = ((iOSAdView)sender!).AdSize.Size.Height;
                };
            }

            Reload(handler.PlatformView);
        }

        protected override iOSAdView CreatePlatformView() => new iOSAdView();/*GetAdSize())
        {
            AdUnitId = Element.AdUnitID,
            RootViewController = GetVisibleViewController()
        };*/

        protected override void ConnectHandler(iOSAdView platformView)
        {
            base.ConnectHandler(platformView);
        }

        protected override void DisconnectHandler(iOSAdView platformView)
        {
            base.DisconnectHandler(platformView);
        }

        private static void Reload(iOSAdView adView)
        {
            var request = Request.GetDefaultRequest();
            request.RegisterAdNetworkExtras(new Extras
            {
                AdditionalParameters = new NSDictionary(new NSString("npa"), new NSString("1"))
            });
            request.Keywords = Keywords.ToArray();

            adView.LoadRequest(request);
        }

        private static AdSize GetAdSize(AdSizes adSize, MauiAdView adView)
        {
            if (adSize == AdSizes.Banner)
            {
                return AdSizeCons.Banner;
            }
            else if (adSize == AdSizes.MediumRectangle)
            {
                return AdSizeCons.MediumRectangle;
            }
            else if (adView.HeightRequest == MauiAdView.InlineBannerHeight)
            {
                return AdSizeCons.MediumRectangle;
                return AdSizeCons.GetCurrentOrientationInlineAdaptiveBannerAdSizeh((nfloat)adView.WidthRequest);
            }
            else
            {
                return new AdSize { Size = new CGSize((float)adView.WidthRequest, (float)adView.HeightRequest) };
            }
        }
    }
}

#if false
using CoreGraphics;
using Foundation;
using Google.MobileAds;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UIKit;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

// TODO Xamarin.Forms.ExportRendererAttribute is not longer supported. For more details see https://github.com/dotnet/maui/wiki/Using-Custom-Renderers-in-.NET-MAUI
[assembly: ExportRenderer(typeof(AdView), typeof(Movies.iOS.AdRenderer))]
// TODO Xamarin.Forms.ExportRendererAttribute is not longer supported. For more details see https://github.com/dotnet/maui/wiki/Using-Custom-Renderers-in-.NET-MAUI
[assembly: ExportRenderer(typeof(Microsoft.Maui.Controls.NativeAdView), typeof(Movies.iOS.NativeAdRenderer))]

namespace Movies.iOS
{
    public class NativeAdRenderer : ViewRenderer<Microsoft.Maui.Controls.NativeAdView, Google.MobileAds.NativeAdView>
    {
        private void GetAd()
        {
            var controller = GetVisibleViewController();
            var adLoader = new AdLoader("ca-app-pub-3940256099942544/3986624511", controller, new AdLoaderAdType[] { AdLoaderAdType.Native }, new AdLoaderOptions[0] );
            adLoader.Delegate = new Listener
            {
                Element = Element
            };
            Element.Headline = "requested";

            adLoader.LoadRequest(Request.GetDefaultRequest());
        }

        UIViewController GetVisibleViewController()
        {
            var rootController = UIApplication.SharedApplication.KeyWindow.RootViewController;

            if (rootController.PresentedViewController == null)
                return rootController;

            if (rootController.PresentedViewController is UINavigationController)
            {
                return ((UINavigationController)rootController.PresentedViewController).VisibleViewController;
            }

            if (rootController.PresentedViewController is UITabBarController)
            {
                return ((UITabBarController)rootController.PresentedViewController).SelectedViewController;
            }

            return rootController.PresentedViewController;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Microsoft.Maui.Controls.NativeAdView> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                GetAd();
            }
        }

        private class Listener : UnifiedNativeAdLoaderDelegate
        {
            public Microsoft.Maui.Controls.NativeAdView Element { get; set; }

            public override void DidFailToReceiveAd(AdLoader adLoader, NSError error)
            {
                Element.Headline = "failed " + error;
                ;
            }

            public override void DidReceiveUnifiedNativeAd(AdLoader adLoader, NativeAd nativeAd)
            {
                Element.Headline = "loaded " + nativeAd;
                ;
            }
        }
    }

    public class AdRenderer : ViewRenderer<AdView, BannerView>
    {
        private BannerView NativeAdView
        {
            get
            {
                if (_NativeAdView != null)
                {
                    return _NativeAdView;
                }
                else if (Element?.AdUnitID == null || Element?.AdSize == null)
                {
                    return null;
                }

                // Setup your BannerView, review AdSizeCons class for more Ad sizes. 
                //adView = new BannerView(size: AdSizeCons.SmartBannerPortrait, origin: new CGPoint(0, UIScreen.MainScreen.Bounds.Size.Height - AdSizeCons.Banner.Size.Height))
                _NativeAdView = new BannerView(GetAdSize())
                {
                    AdUnitId = Element.AdUnitID,
                    RootViewController = GetVisibleViewController()
                };
                AdSizeUpdated();

                var request = Request.GetDefaultRequest();
                request.RegisterAdNetworkExtras(new Extras
                {
                    AdditionalParameters = new NSDictionary(new NSString("npa"), new NSString("1"))
                });
                request.Keywords = App.AdKeywords;

                _NativeAdView.LoadRequest(request);
                return _NativeAdView;
            }
        }
        private BannerView _NativeAdView;

        protected override void OnElementChanged(ElementChangedEventArgs<AdView> e)
        {
            base.OnElementChanged(e);

            if (Control == null && _NativeAdView == null)
            {
                try
                {
                    SetNativeControl(NativeAdView);
                }
                catch { }
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == AdView.AdSizeProperty.PropertyName)
            {
                return;
                var adSize = GetAdSize();
                NativeAdView.AdSize = adSize;

                AdSizeUpdated();
            }
        }

        private void AdSizeUpdated()
        {
            var size = NativeAdView.AdSize.Size;
            Element.HeightRequest = size.Height;

            if (Element.HeightRequest == AdView.InlineBannerHeight)
            {
                // Wire AdReceived event to know when the Ad is ready to be displayed
                _NativeAdView.AdReceived += (sender, e) =>
                {
                    Element.HeightRequest = ((BannerView)sender).AdSize.Size.Height;
                };
            }
            else
            {

            }

            Element.WidthRequest = size.Width;
        }

        private AdSize GetAdSize()
        {
            var adSize = Element.AdSize;

            if (adSize == AdSizes.Banner)
            {
                return AdSizeCons.Banner;
            }
            else if (adSize == AdSizes.MediumRectangle)
            {
                return AdSizeCons.MediumRectangle;
            }
            else if (Element.HeightRequest == AdView.InlineBannerHeight)
            {
                return AdSizeCons.MediumRectangle;
                return AdSizeCons.GetCurrentOrientationInlineAdaptiveBannerAdSizeh((nfloat)Element.WidthRequest);
            }
            else
            {
                return new AdSize { Size = new CGSize((float)Element.WidthRequest, (float)Element.HeightRequest) };
            }
        }

        /// 
        /// Gets the visible view controller.
        /// 
        /// The visible view controller.
        UIViewController GetVisibleViewController()
        {
            var rootController = UIApplication.SharedApplication.KeyWindow.RootViewController;

            if (rootController.PresentedViewController == null)
                return rootController;

            if (rootController.PresentedViewController is UINavigationController)
            {
                return ((UINavigationController)rootController.PresentedViewController).VisibleViewController;
            }

            if (rootController.PresentedViewController is UITabBarController)
            {
                return ((UITabBarController)rootController.PresentedViewController).SelectedViewController;
            }

            return rootController.PresentedViewController;
        }
    }

    public class CachedAdRenderer : ViewRenderer<AdView, BannerView>
    {
        private static readonly Dictionary<string, BannerView> Cache = new Dictionary<string, BannerView>();

        private BannerView NativeAdView
        {
            get
            {
                if (Element?.AdUnitID == null || Element?.AdSize == null)
                {
                    return null;
                }
                else if (Cache.TryGetValue(Element.AdUnitID, out var ad))
                {
                    return ad;
                }

                // Setup your BannerView, review AdSizeCons class for more Ad sizes. 
                //adView = new BannerView(size: AdSizeCons.SmartBannerPortrait, origin: new CGPoint(0, UIScreen.MainScreen.Bounds.Size.Height - AdSizeCons.Banner.Size.Height))
                var _NativeAdView = Cache[Element.AdUnitID] = new BannerView(GetAdSize())
                {
                    AdUnitId = Element.AdUnitID,
                    RootViewController = GetVisibleViewController()
                };
                AdSizeUpdated(_NativeAdView);

                var request = Request.GetDefaultRequest();
                request.RegisterAdNetworkExtras(new Extras
                {
                    AdditionalParameters = new NSDictionary(new NSString("npa"), new NSString("1"))
                });
                request.Keywords = App.AdKeywords;

                _NativeAdView.LoadRequest(request);
                return _NativeAdView;
            }
        }
        //private BannerView _NativeAdView;

        protected override void OnElementChanged(ElementChangedEventArgs<AdView> e)
        {
            base.OnElementChanged(e);

            if (Control == null)// && _NativeAdView == null)
            {
                try
                {
                    foreach (var subview in NativeView.Subviews)
                    {
                        subview.RemoveFromSuperview();
                    }

                    SetNativeControl(NativeAdView);
                }
                catch { }
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == AdView.AdSizeProperty.PropertyName)
            {
                return;
                /*var adSize = GetAdSize();
                NativeAdView.AdSize = adSize;

                AdSizeUpdated();*/
            }
        }

        private void AdSizeUpdated(BannerView ad)
        {
            var size = ad.AdSize.Size;
            Element.HeightRequest = size.Height;

            if (Element.HeightRequest == AdView.InlineBannerHeight)
            {
                // Wire AdReceived event to know when the Ad is ready to be displayed
                ad.AdReceived += (sender, e) =>
                {
                    Element.HeightRequest = ((BannerView)sender).AdSize.Size.Height;
                };
            }
            else
            {

            }

            Element.WidthRequest = size.Width;
        }

        private AdSize GetAdSize()
        {
            var adSize = Element.AdSize;

            if (adSize == AdSizes.Banner)
            {
                return AdSizeCons.Banner;
            }
            else if (adSize == AdSizes.MediumRectangle)
            {
                return AdSizeCons.MediumRectangle;
            }
            else if (Element.HeightRequest == AdView.InlineBannerHeight)
            {
                return AdSizeCons.MediumRectangle;
                return AdSizeCons.GetCurrentOrientationInlineAdaptiveBannerAdSizeh((nfloat)Element.WidthRequest);
            }
            else
            {
                return new AdSize { Size = new CGSize((float)Element.WidthRequest, (float)Element.HeightRequest) };
            }
        }

        /// 
        /// Gets the visible view controller.
        /// 
        /// The visible view controller.
        UIViewController GetVisibleViewController()
        {
            var rootController = UIApplication.SharedApplication.KeyWindow.RootViewController;

            if (rootController.PresentedViewController == null)
                return rootController;

            if (rootController.PresentedViewController is UINavigationController)
            {
                return ((UINavigationController)rootController.PresentedViewController).VisibleViewController;
            }

            if (rootController.PresentedViewController is UITabBarController)
            {
                return ((UITabBarController)rootController.PresentedViewController).SelectedViewController;
            }

            return rootController.PresentedViewController;
        }
    }
}
#endif