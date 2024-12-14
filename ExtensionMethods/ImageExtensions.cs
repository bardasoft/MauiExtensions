using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
    public static class ImageExtensions
    {
        private static LoadView LoadSpace = new LoadView { IsVisible = false };

        public static void Preload(this Layout<View> context, params ImageSource[] sources) => LoadSpace.ForceDraw(context, sources);

        private class LoadView : Layout<View>
        {
            public void ForceDraw(Layout<View> context, params ImageSource[] sources)
            {
                context?.Children.Add(this);

                foreach (ImageSource source in sources)
                {
                    Image image = new Image { Source = source };
                    Children.Add(image);
                    LayoutChildIntoBoundingRegion(image, new Rect(0, 0, double.PositiveInfinity, double.PositiveInfinity));
                }

                this.Remove();
            }

            protected override void InvalidateLayout() { }

            protected override void InvalidateMeasure() { }

            protected override void OnChildMeasureInvalidated() { }

            protected override bool ShouldInvalidateOnChildAdded(View child) => false;

            protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint) => new SizeRequest(Size.Zero);

            protected override void LayoutChildren(double x, double y, double width, double height) { }
        }

#if DEBUG && false
        private class FakeLayout : Microsoft.Maui.Controls.Compatibility.Layout
        {
            public FakeLayout(View content)
            {
                Content = content;
                Parent = Application.Current.MainPage;
                OnParentSet();

                InvalidateMeasure();
                InvalidateLayout();

                OnMeasure(double.PositiveInfinity, double.PositiveInfinity);
                OnSizeAllocated(1000, 1000);
                ForceLayout();
                Layout(new Rect(0, 0, 1000, 1000));
                LayoutChildren(0, 0, 1000, 1000);
                LayoutChildIntoBoundingRegion(content, new Rect(Point.Zero, content.Measure(double.PositiveInfinity, double.PositiveInfinity).Request));

                ForceLayout();
            }

            private View Content;

            protected override void LayoutChildren(double x, double y, double width, double height)
            {
                LayoutChildIntoBoundingRegion(Content, new Rect(Point.Zero, Content.Measure(double.PositiveInfinity, double.PositiveInfinity).Request));
            }
        }

        private class FakeImage : Image
        {
            public FakeImage(ImageSource source)
            {
                Source = source;
                Parent = Application.Current.MainPage;
                //SetIsLoading(true);
                InvalidateMeasure();
                OnMeasure(double.PositiveInfinity, double.PositiveInfinity);
                OnSizeAllocated(1000, 1000);
                Layout(new Rect(0, 0, 1000, 1000));
            }
        }

        public static Image Preload(this Image image)
        {
            new FakeLayout(new Image { Source = image.Source });
            //(App.Current.Home.Content as AbsoluteLayout).Children.Add(new FakeLayout(new Image { Source = image.Source }), new Rectangle(-1000, -1000, 100, 10));
            return image;
        }

        public static ImageSource Preload(this ImageSource source)
        {
            new FakeImage(source);
            return source;
        }
#endif
    }
}
