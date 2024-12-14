using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
    public static class PointExtensions
    {
        /// <summary>
        /// Constrains the point to fall inside the rectangle
        /// </summary>
        /// <param name="point"></param>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public static Point Bound(this Point point, Rect bounds) => new Point(
            Math.Max(bounds.Left, Math.Min(bounds.Right, point.X)),
            Math.Max(bounds.Top, Math.Min(bounds.Bottom, point.Y))
            );

        public static Point Add(this Point p1, Point p2) => new Point(p1.X + p2.X, p1.Y + p2.Y);

        public static Point Subtract(this Point p1, Point p2) => new Point(p1.X - p2.X, p1.Y - p2.Y);

        public static Point Multiply(this Point p1, double d) => new Point(p1.X * d, p1.Y * d);
    }
}
