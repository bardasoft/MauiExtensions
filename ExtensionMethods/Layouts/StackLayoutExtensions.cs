using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
    public static class StackLayoutExtensions
    {
        public static T Orient<T>(this StackLayout layout, T ifHorizontal, T ifVertical) => layout.Orientation == StackOrientation.Horizontal ? ifHorizontal : ifVertical;

        public static StackOrientation Invert(this StackOrientation orientation) => orientation == StackOrientation.Horizontal ? StackOrientation.Vertical : StackOrientation.Horizontal;
    }
}
