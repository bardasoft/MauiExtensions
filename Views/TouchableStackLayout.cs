using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
    public class TouchableStackLayout : StackLayout, ITouchable
    {
        public event EventHandler<TouchEventArgs> Touch;
        public event ClickEventHandler Tapped;

        public bool ShouldIntercept => Touch != null || Tapped != null;

        private bool Moved = false;

        public virtual void OnTouch(Point point, TouchState state)
        {
            if (state == TouchState.Moving)
            {
                Moved = true;
            }
            else if (state != TouchState.Down)
            {
                if (!Moved)
                {
                    Tapped?.Invoke(point);
                }

                Moved = false;
            }

            Touch?.Invoke(this, new TouchEventArgs(point, state));
        }

        public Action<object, TouchEventArgs> LongClicked;
    }
}
