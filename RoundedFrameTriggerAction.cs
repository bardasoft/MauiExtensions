using System;

namespace Microsoft.Maui.Controls
{
    public class RoundedFrameTriggerAction : TriggerAction<Frame>
    {
        protected override void Invoke(Frame sender)
        {
            sender.CornerRadius = (float)(Math.Min(sender.Width, sender.Height) / 2);
        }
    }
}
