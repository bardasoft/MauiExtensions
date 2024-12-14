using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Extensions;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.Extensions
{
    public delegate void DragEventHandler(DragState state);
    public enum DragState { Started, Moving, Ended };

    public static class TouchScreen
    {
        public static Point FirstTouch { get; private set; }
        public static Point LastTouch { get; private set; }

        public static bool Active { get; private set; } = false;
        public static Page Instance;

        public static double FatFinger = 50;

        private static View Draggable;
        private static Layout<View> DropArea;
        private static double Speed;
        private static Point LastPosition;

        public static event StaticEventHandler<TouchEventArgs> Touch;
        public static event StaticEventHandler<TouchEventArgs> InterceptedTouch;
        public static event EventHandler<EventArgs<DragState>> Dragging;
        public static event ClickEventHandler Tapped;

        private static bool Moved = false;

        public static void MakeDraggable<T>(this T touchable, Layout<View> dropArea, EventHandler<EventArgs<DragState>> onDrag = null, Point? startPoint = null, View startView = null, double speed = 1)
            where T : View, ITouchable
        {
            touchable.WhenTouched(TouchState.Moving, (sender, e) =>
            {
                if (startPoint.HasValue)
                {
                    BeginDrag(touchable, dropArea, startPoint.Value, speed);
                }
                else
                {
                    BeginDrag(touchable, dropArea, startView ?? touchable, speed);
                }

                if (onDrag != null)
                {
                    Dragging += onDrag;
                }
            });
        }

        public static void BeginDrag(View draggable, Layout<View> dropArea, double speed = 1) => BeginDrag(draggable, dropArea, draggable, speed);

        public static void BeginDrag(View draggable, Layout<View> dropArea, View startAt, double speed = 1) => BeginDrag(draggable, dropArea, startAt.PositionOn(Instance).Subtract(dropArea.PositionOn(Instance)), speed);

        public static void BeginDrag(View draggable, Layout<View> dropArea, Point start, double speed = 1)
        {
            Draggable = draggable;
            DropArea = dropArea;
            Speed = speed;

            Point touch = FirstTouch.Subtract(dropArea.PositionOn(Instance));
            Size size = Draggable.Measure();
            if (new Rect(touch.X - FatFinger, touch.Y - FatFinger, FatFinger * 2, FatFinger * 2).Contains(new Rect(start, size)))
            {
                start = new Point(start.X, touch.Y - FatFinger / 2 - size.Height);
            }

            DropArea.Children.Add(Draggable);
            MoveDraggable(start);

            Active = true;
            Dragging?.Invoke(Draggable, new EventArgs<DragState>(DragState.Started));
        }

        public static void OnTouch(Point point, TouchState state)
        {
            if (state == TouchState.Moving)
            {
                if (Active)
                {
                    UpdatePosition(point, state);
                    Dragging?.Invoke(Draggable, new EventArgs<DragState>(DragState.Moving));
                }

                Moved = true;
            }
            else if (state == TouchState.Up)
            {
                Active = false;
                Dragging?.Invoke(Draggable, new EventArgs<DragState>(DragState.Ended));
                Dragging = null;

                if (!Moved)
                {
                    Tapped?.Invoke(point);
                }

                Moved = false;
            }

            Touch?.Invoke(new TouchEventArgs(point, state));
            InterceptedTouch?.Invoke(new TouchEventArgs(point, state));
        }

        public static void OnInterceptedTouch(Point point, TouchState state)
        {
            if (state == TouchState.Down)
            {
                FirstTouch = point;
            }
            LastTouch = point;

            InterceptedTouch?.Invoke(new TouchEventArgs(point, state));
        }

        private static void UpdatePosition(Point point, TouchState state)
        {
            MoveDraggable(LastPosition.Add(point.Subtract(FirstTouch).Multiply(Speed)));
            FirstTouch = point;
        }

        private static void MoveDraggable(Point point)
        {
            Draggable.MoveToBounded(point);
            LastPosition = point;
        }
    }

    /*public class TouchScreen : AbsoluteLayout, ITouchable
    {
        public static Point LastTouch { get; private set; }
        public static bool Active { get; private set; } = false;
        public static double FatFinger = 50;

        private static TouchScreen Instance;
        private static View Draggable;
        private static Layout<View> DropArea;
        private static double Speed;
        private static Point LastPosition;

        public static event DragEventHandler Dragging;
        public event EventHandler<TouchEventArgs> Touch;
        public event ClickEventHandler Tapped;
        public event EventHandler<TouchEventArgs> InterceptedTouch;

        public bool ShouldIntercept => true;

        public TouchScreen()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
#if DEBUG
                throw new Exception("There can only be one instance of TouchScreen");
#endif
            }
        }

        private bool Moved = false;

        public void OnTouch(Point point, TouchState state)
        {
            if (state == TouchState.Moving)
            {
                if (Active)
                {
                    UpdatePosition(point, state);
                    Dragging?.Invoke(DragState.Moving);
                }

                Moved = true;
            }
            else if (state == TouchState.Up)
            {
                Active = false;
                Dragging?.Invoke(DragState.Ended);
                Dragging = null;

                if (!Moved)
                {
                    Tapped?.Invoke(point);
                }

                Moved = false;
            }

            Touch?.Invoke(this, new TouchEventArgs(point, state));
            InterceptedTouch?.Invoke(this, new TouchEventArgs(point, state));
        }

        public void OnInterceptedTouch(Point point, TouchState state)
        {
            if (state == TouchState.Down)
            {
                LastTouch = point;
            }

            InterceptedTouch?.Invoke(this, new TouchEventArgs(point, state));
        }

        public static void BeginDrag(View draggable, Layout<View> dropArea, double speed = 1) => BeginDrag(draggable, dropArea, draggable, speed);

        public static void BeginDrag(View draggable, Layout<View> dropArea, View startAt, double speed = 1) => BeginDrag(draggable, dropArea, startAt.PositionOn(Instance).Subtract(dropArea.PositionOn(Instance)), speed);

        public static void BeginDrag(View draggable, Layout<View> dropArea, Point start, double speed = 1)
        {
            Draggable = draggable;
            DropArea = dropArea;
            Speed = speed;

            Point touch = LastTouch.Subtract(dropArea.PositionOn(Instance));
            Size size = Draggable.Measure();
            if (new Rectangle(touch.X - FatFinger, touch.Y - FatFinger, FatFinger * 2, FatFinger * 2).Contains(new Rectangle(start, size)))
            {
                start = new Point(start.X, touch.Y - FatFinger / 2 - size.Height);
            }

            DropArea.Children.Add(Draggable);
            MoveDraggable(start);

            Active = true;
        }

        private static void UpdatePosition(Point point, TouchState state)
        {
            MoveDraggable(LastPosition.Add(point.Subtract(LastTouch).Multiply(Speed)));
            LastTouch = point;
        }

        private static void MoveDraggable(Point point)
        {
            Draggable.MoveToBounded(point);
            LastPosition = point;
        }
    }*/
}
