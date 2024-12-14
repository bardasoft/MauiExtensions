using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
    public static class SizeExtensions
    {
        public static double Area(this Size size) => size.Width * size.Height;
    }
}
