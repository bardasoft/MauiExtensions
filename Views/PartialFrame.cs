using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls.Extensions;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility
{
    public enum Corner
    {
        None = 0,
        TopLeft = 1,
        TopRight = 2,
        BottomLeft = 4,
        BottomRight = 8,
        Top = TopLeft | TopRight,
        Bottom = BottomRight | BottomLeft,
        Left = TopLeft | BottomLeft,
        Right = TopRight | BottomRight
    }

    public class PartialFrame : AbsoluteLayout
    {
        public readonly Frame Frame;

        public Corner Covered
        {
            get => _Covered;
            set
            {
                int i = 0;
                foreach (Corner corner in new Corner[] { Corner.TopLeft, Corner.TopRight, Corner.BottomLeft, Corner.BottomRight })
                {
                    BoxView cover = CornerCovers[i];

                    if (value.HasFlag(corner) && cover.Parent == null)
                    {
                        Children.Add(cover, new Rect(i % 2, Math.Floor(i / 2.0), 0.5, 0.5), AbsoluteLayoutFlags.All, 0);
                    }
                    else
                    {
                        Children.Remove(cover);
                    }

                    i++;
                }

                _Covered = value;
            }
        }
        private Corner _Covered = Corner.None;

        private readonly BoxView[] CornerCovers = new BoxView[4];

        public PartialFrame() : this(new Frame()) { }

        public PartialFrame(Frame frame)
        {
            Frame = frame;

            this.SetBinding(IsVisibleProperty, Frame, "IsVisible");
            for (int i = 0; i < 4; i++)
            {
                BoxView cover = new BoxView
                {
                    IsVisible = false,
                };
                cover.SetBinding(BackgroundColorProperty, Frame, "BackgroundColor");
                cover.SetBinding(IsVisibleProperty, Frame, "IsVisible");

                CornerCovers[i] = cover;
            }

            Children.Add(Frame, new Rect(0, 0, 1, 1), AbsoluteLayoutFlags.All);
        }

        public static implicit operator PartialFrame(Frame frame) => new PartialFrame(frame);
    }
}
