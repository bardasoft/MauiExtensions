using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.Controls
{
    public static partial class AdExtensions
    {
        public static partial MauiAppBuilder UseAdmobAds(this MauiAppBuilder builder)
        {
            System.Diagnostics.Debug.WriteLine("Admob ads have not been implemented on MacCatalyst");
            return builder;
        }
    }
}

namespace MauiExtensions.Handlers
{
    public class AndroidAdView : UIView { }

    public partial class AdViewHandler : ViewHandler<AdView, AndroidAdView>
    {
        public static void MapAdUnitId(AdViewHandler handler, AdView adView)
        {
            throw new NotImplementedException();
        }

        public static void MapAdSize(AdViewHandler handler, AdView adView)
        {
            throw new NotImplementedException();
        }

        protected override AndroidAdView CreatePlatformView() => new AndroidAdView();

        protected override void ConnectHandler(AndroidAdView platformView)
        {
            base.ConnectHandler(platformView);
        }

        protected override void DisconnectHandler(AndroidAdView platformView)
        {
            base.DisconnectHandler(platformView);
        }
    }
}

#if false
using Android.Content;
using Android.Gms.Ads;
using Android.Gms.Ads.NativeAd;
using Google.Ads.Mediation.Admob;
using System;
using System.ComponentModel;
using XFAdView = Microsoft.Maui.Controls.AdView;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;

// TODO Xamarin.Forms.ExportRendererAttribute is not longer supported. For more details see https://github.com/dotnet/maui/wiki/Using-Custom-Renderers-in-.NET-MAUI
//[assembly: Xamarin.Forms.ExportRenderer(typeof(XFAdView), typeof(Movies.Droid.AdRenderer))]
// TODO Xamarin.Forms.ExportRendererAttribute is not longer supported. For more details see https://github.com/dotnet/maui/wiki/Using-Custom-Renderers-in-.NET-MAUI
//[assembly: Xamarin.Forms.ExportRenderer(typeof(Xamarin.Forms.NativeAdView), typeof(Movies.Droid.NativeAdRenderer))]
//[assembly: ExportRenderer(typeof(XFAdView), typeof(Movies.Droid.TestAdViewRenderer))]
namespace Movies.Droid
{
    public class NativeAdRenderer : ViewRenderer<Microsoft.Maui.Controls.NativeAdView, NativeAdView>
    {
        public NativeAdRenderer(Context context) : base(context) { }

        private void GetAd()
        {
            var adLoader = new AdLoader.Builder(Context, "ca-app-pub-3940256099942544/2247696110");
            var listener = new NativeListener(nativeAd =>
            {
                Element.Headline = nativeAd.Headline;

                SetNativeControl(new NativeAdView(Context));
            });
            adLoader.ForNativeAd(listener);
            adLoader.WithAdListener(listener);
            adLoader.WithNativeAdOptions(new NativeAdOptions.Builder().Build());

            adLoader.Build().LoadAd(new AdRequest.Builder().Build());
        }

        private class NativeListener : AdListener, NativeAd.IOnNativeAdLoadedListener
        {
            private Action<NativeAd> Action;

            public NativeListener(Action<NativeAd> action)
            {
                Action = action;
            }

            public override void OnAdOpened()
            {
                base.OnAdOpened();
            }

            public override void OnAdFailedToLoad(LoadAdError p0)
            {
                base.OnAdFailedToLoad(p0);
                System.Diagnostics.Debug.WriteLine(p0);
            }

            public void OnNativeAdLoaded(NativeAd p0) => Action(p0);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Microsoft.Maui.Controls.NativeAdView> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                GetAd();
            }
        }
    }

    public class AdRenderer : ViewRenderer<XFAdView, AdView>
    {
        public AdRenderer(Context context) : base(context) { }

        //Note you may want to adjust this, see further down.
        private AdView AdView
        {
            get
            {
                if (adView != null)
                {
                    return adView;
                }
                else if (Element?.AdUnitID == null || Element?.AdSize == null)
                {
                    return null;
                }

                adView = new AdView(Context)
                {
                    AdUnitId = Element.AdUnitID,
                };
                UpdateAdSize();
                adView.LoadAd(new AdRequest.Builder().Build());

                return adView;
            }
        }
        private AdView adView;

        private class Listener : AdListener
        {
            private Action OnLoaded;

            public Listener(Action onLoaded)
            {
                OnLoaded = onLoaded;
            }

            public override void OnAdLoaded()
            {
                base.OnAdLoaded();
                OnLoaded();
            }
        }

        private void UpdateAdSize()
        {
            var adSize = GetAdSize();
            AdView.AdSize = adSize;

            if (Element.HeightRequest == XFAdView.InlineBannerHeight)
            {
                AdView.AdListener = new Listener(() =>
                {
                    try
                    {
                        Element.HeightRequest = AdView.AdSize.Height;
                    }
                    catch { }
                });
            }
            else
            {
                Element.HeightRequest = AdView.AdSize.Height;
            }

            Element.WidthRequest = AdView.AdSize.Width;
        }

        private AdSize GetAdSize()
        {
            var adSize = Element.AdSize;

            if (adSize == Microsoft.Maui.Controls.AdSizes.Banner)
            {
                return AdSize.Banner;
            }
            else if (adSize == Microsoft.Maui.Controls.AdSizes.MediumRectangle)
            {
                return AdSize.MediumRectangle;
            }
            else if (Element.HeightRequest == XFAdView.InlineBannerHeight)
            {
                return AdSize.GetCurrentOrientationInlineAdaptiveBannerAdSize(Context, (int)Element.WidthRequest);
            }
            else
            {
                return new AdSize((int)Element.WidthRequest, (int)Element.HeightRequest);
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == XFAdView.AdSizeProperty.PropertyName)
            {
                UpdateAdSize();
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<XFAdView> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                //MainPage.LoadAd = CreateNativeAdControl;
                //CreateNativeAdControl();
                SetNativeControl(AdView);
            }
        }
    }
}
#endif