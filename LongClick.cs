using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Extensions
{
    public interface ILongClickable
    {
        event EventHandler<TouchEventArgs> LongClick;

        bool ShouldInvoke { get; }

        void OnLongClick(Point point, TouchState state);
    }
}
