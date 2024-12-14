using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
    public class LongClickableButton : Button, ITouchable
    {
        public event EventHandler<EventArgs> LongClick;
        public event EventHandler<TouchEventArgs> Touch;

        public bool ShouldIntercept => Touch != null;
        new public bool IsPressed => base.IsPressed && IsLongPressed;
        public bool IsLongPressed = true;

        public virtual void OnLongClick() => LongClick?.Invoke(this, new EventArgs());
        public virtual void OnTouch(Point point, TouchState state) => Touch?.Invoke(this, new TouchEventArgs(point, state));
    }
}
