using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls.Extensions;

namespace Microsoft.Maui.Controls
{
    public static class Scrollable
    {
        public static BindableProperty<Action<ScrollToPositionRequestEventArgs>> NativeScrollImplementationProperty = NativeImplementation.CreateAttached("NativeScrollImplementation");

        public static void ScrollToPosition(this ListView listView, int x, int y, bool animated = false) => listView.GetNativeImplementation(NativeScrollImplementationProperty)?.Invoke(new ScrollToPositionRequestEventArgs(x, y, animated));
    }

    public class ScrollToPositionRequestEventArgs : EventArgs
    {
        public int X;
        public int Y;
        public bool Animated;

        public ScrollToPositionRequestEventArgs(int x, int y, bool animated = false)
        {
            X = x;
            Y = y;
            Animated = animated;
        }
    }
}
