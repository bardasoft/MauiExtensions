using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Controls
{
    public static class RectangleExtensions
    {
        public static readonly Rect Unbounded = new Rect(double.NegativeInfinity, double.NegativeInfinity, double.PositiveInfinity, double.PositiveInfinity);

        public static Rect Shrink(this Rect bounds, Size size) => new Rect(bounds.Location, new Size(bounds.Width - size.Width, bounds.Height - size.Height));
    }
}
