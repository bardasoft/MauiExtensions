using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
    public static class Misc
    {
        public static double On<T>(this NamedSize size) => Device.GetNamedSize(size, typeof(T));

        public static string UsefulToString(this Thickness t) => t.Left + ", " + t.Top + ", " + t.Right + ", " + t.Bottom;
    }
}
