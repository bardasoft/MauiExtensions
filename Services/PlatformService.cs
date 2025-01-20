using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiExtensions.Services
{
    public partial class PlatformService
    {
        public static double NavBarHeight => GetNavBarHeight();

        internal static partial double GetNavBarHeight();
    }
}
