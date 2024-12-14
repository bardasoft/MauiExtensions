using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
    public interface ISoftKeyboard
    {
        event EventHandler OnscreenSizeChanged;

        Size Size { get; }

        void Enable(bool animated = false);
        void Disable(bool animated = false);
    }

    /*public class OnScreenKeyboard : ContentView, ISoftKeyboard
    {
        //public event KeystrokeEventHandler Typed;
        public event EventHandler OnscreenSizeChanged;

        //public View Content { get; private set; }
        public Size Size { get; private set; }

        public OnScreenKeyboard(View content)
        {
            Content = content;
            Content.SizeChanged += (sender, e) =>
            {
                Size = Content.Bounds.Size;
                OnscreenSizeChanged?.Invoke(this, new EventArgs());
            };

            if (content is IKeyboard keyboard)
            {
                keyboard.Typed += (keystrokes) => OnTyped(keystrokes);
            }

            /*this.Bind<View>(ContentProperty, value =>
            {
                if (value == null)
                {
                    return;
                }
            });

            return;
            Application.Current.MainPage.SizeChanged += (sender, e) => ScreenSizeChanged();
            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == ContentProperty.PropertyName || e.PropertyName == PaddingProperty.PropertyName)
                {
                    ScreenSizeChanged();
                }
            };
            ScreenSizeChanged();
        }

        public void Enable(bool animated = false) => IsVisible = true;

        public void Disable(bool animated = false) => IsVisible = false;

        protected void OnTyped(IEnumerable<char> keystrokes) => Typed?.Invoke(keystrokes);

        private void ScreenSizeChanged()
        {
            if (Content == null)
            {
                return;
            }

            Size bounds = Application.Current.MainPage.Bounds.Size;
            Size constraint = new Size(bounds.Width - Padding.HorizontalThickness, bounds.Height - Padding.VerticalThickness);
            Size request = Size.Zero;// ((CrunchKeyboard)Content).MeasureOnscreenSize(constraint.Width, constraint.Height);

            Size = new Size(Math.Min(request.Width, constraint.Width), Math.Min(request.Height, constraint.Height));

            if (!request.Equals(Size))
            {
                //InvalidateMeasure();
                //InvalidateLayout();
            }
            //Print.Log("requested " + Size, constraint);

            //LayoutChildIntoBoundingRegion();
        }

        //protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint) => new SizeRequest(Size);

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            Size bounds = Application.Current.MainPage.Bounds.Size;
            Size constraint = new Size(bounds.Width, bounds.Height);
            SizeRequest sr = Content.Measure(constraint.Width, constraint.Height);

            Size request = new Size(Math.Min(constraint.Width, sr.Request.Width), Math.Min(constraint.Height, sr.Request.Height));
            //Print.Log("requested " + sr.Request, request, constraint);
            //Print.Log("\t" + x, y, width, height, Width, Height);

            LayoutChildIntoBoundingRegion(Content, new Rectangle(new Point((width - request.Width) / 2, (height - request.Height) / 2), request));
        }
    }*/

    public static class SoftKeyboardManager
    {
        public static event EventHandler SizeChanged;
        public static event EventHandler KeyboardChanged;
        public static event EventHandler Dismissed;

        public static ISoftKeyboard Current { get; private set; }
        public static Size Size { get; private set; }

        private static List<ISoftKeyboard> Keyboards = new List<ISoftKeyboard>();

        public static void AddKeyboard(params ISoftKeyboard[] keyboards) => Keyboards.AddRange(keyboards);

        public static void ClearKeyboards()
        {
            SwitchTo(null);
            Keyboards = new List<ISoftKeyboard>();
        }

        public static void OnDismissed()
        {
            //Current?.Disable(true);
            Dismissed?.Invoke(Current, new EventArgs());
        }

        public static void SwitchTo(int index) => SwitchTo(Keyboards[index]);

        public static void SwitchTo(ISoftKeyboard keyboard)
        {
            if (keyboard == Current)
            {
                return;
            }

            if (Current != null)
            {
                Current.Disable();
                //Current.Typed -= KeyboardManager.Type;
                Current.OnscreenSizeChanged -= OnSizeChanged;
            }

            Current = keyboard;

            if (Current != null)
            {
                //Current.Typed += KeyboardManager.Type;
                Current.OnscreenSizeChanged += OnSizeChanged;
                Current.Enable();
            }
            
            KeyboardChanged?.Invoke(Current, new EventArgs());

            if (Current != null)
            {
                OnSizeChanged(Current, new EventArgs());
            }
        }

        public static void NextKeyboard() => SwitchToRelative(1);
        public static void PrevKeyboard() => SwitchToRelative(-1);

        public static IEnumerable<ISoftKeyboard> Connected() => Keyboards;

        private static void SwitchToRelative(int diff) => SwitchTo((Keyboards.IndexOf(Current) + diff) % Keyboards.Count);

        public static void OnSizeChanged(Size size)
        {
            Size = size;
            SizeChanged?.Invoke(Current, new EventArgs());
        }

        private static void OnSizeChanged(object sender, EventArgs e)
        {
            ISoftKeyboard softKeyboard = (ISoftKeyboard)sender;
            if (softKeyboard.Size.Equals(Size))
            {
                return;
            }

            Size = softKeyboard.Size;
            SizeChanged?.Invoke(sender, new EventArgs());
        }
    }
}
