using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
    public interface IImage
    {
        event EventHandler<EventArgs<bool>> Loaded;

        View View { get; }

        ImageSource Source { get; set; }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WebImage : AbsoluteLayout
    {
        public readonly IImage Image;

        public int LoadTimeout = 7000;

        private bool Loaded = false;

        public WebImage()
        {
            InitializeComponent();
        }

        public WebImage(IImage image) : this()
        {
            //Print.Log("image is from", image.Source, image.Source is UriImageSource);
            Image = image;
            
            Image.Loaded += (sender, e) =>
            {
                if (e.Value)
                {
                    Loaded = true;
                    LoadingLabel.IsVisible = ErrorLabel.IsVisible = false;

                    base.InvalidateMeasure();
                }
                else
                {
                    ShowError();
                }
            };

            Image.View.MeasureInvalidated += (sender, e) => base.InvalidateMeasure();

            Children.Add(Image.View);
            AbsoluteLayout.SetLayoutBounds(Image.View, new Rect(0.5, 0.5, 1, 1));
            AbsoluteLayout.SetLayoutFlags(Image.View, AbsoluteLayoutFlags.All);
            Image.View.InputTransparent = true;
            
            SizeChanged += (sender, e) =>
            {
                if (!Loaded && !LoadingLabel.IsVisible)
                {
                    Load();
                }
            };
        }

        public WebImage(Image image) : this(new FormsImage(image)) { }

        public static implicit operator WebImage(Image image) => new WebImage(image);

        private void Reload(object sender, EventArgs e)
        {
            ImageSource source = Image.Source;
            Image.Source = null;
            Image.Source = source;

            Load();
        }

        private async void Load()
        {
            Loaded = false;
            
            LoadingLabel.IsVisible = true;
            ErrorLabel.IsVisible = false;

            base.InvalidateMeasure();

            /*while (Width < 0 || Height < 0)
            {
                await Task.Delay(100);
            }*/
            await Task.Delay(LoadTimeout);

            if (!Loaded)
            {
                ShowError();
            }
        }

        public void ShowError()
        {
            LoadingLabel.IsVisible = false;
            ErrorLabel.IsVisible = true;
        }

        protected override void InvalidateMeasure() { }

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            SizeRequest sr = Loaded ? new SizeRequest(Image.View.Measure(widthConstraint, heightConstraint), new Size(10, 10)) : base.OnMeasure(widthConstraint, heightConstraint);
            //Print.Log("requesting", Loaded, sr.Request, widthConstraint, heightConstraint);
            return sr;
        }

        private class FormsImage : IImage
        {
            public event EventHandler<EventArgs<bool>> Loaded;

            public View View => Image;

            public ImageSource Source
            {
                get => Image.Source;
                set => Image.Source = value;
            }

            private Image Image;
            private bool WasLoaded = false;

            public FormsImage(Image image)
            {
                Image = image;

                image.MeasureInvalidated += (sender, e) =>
                {
                    Size request = ((View)sender).Measure();
                    bool loaded = request.Width >= 1 && request.Height >= 1;
                    
                    if (WasLoaded != loaded)
                    {
                        Loaded?.Invoke(this, new EventArgs<bool>(WasLoaded = loaded));
                    }
                };
            }
        }
    }
}