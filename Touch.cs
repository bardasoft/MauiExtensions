using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.Maui.Graphics;

/*namespace Microsoft.Maui.Controls
{
    using DiscreteTouchEventDelegate = DiscreteTouchEventDelegate<DiscreteTouchEventArgs>;
    using ContinuousTouchEventDelegate = ContinuousTouchEventDelegate<TouchEventArgs>;

    public static class GestureExtensions
    {
        public static BindableProperty LongClickEventProperty = BindableProperty.CreateAttached("LongClick", typeof(DiscreteTouchEvent), typeof(Gestures), null, defaultValueCreator: value => new DiscreteTouchEvent((View)value));

        public static void AddLongClickListener(this View view, DiscreteTouchEventDelegate listener) => view.AddDiscreteTouchListener(LongClickEventProperty, listener);
    }
}*/

namespace Microsoft.Maui.Controls
{
    public enum Gesture
    {
        Click,
        LongClick,
        Pinch,
        Rotation,
        Swipe,
        Pan,
    }

    //public delegate void DiscreteTouchEventDelegate<in T>(object sender, T e) where T : DiscreteTouchEventArgs, new();
    //public delegate void ContinuousTouchEventDelegate<in T>(object sender, T e) where T : TouchEventArgs, new();

    public class TouchInterface
    {
        public event EventHandler<TouchEventArgs> Touch;

        public bool TryGetValue(Gesture key, out ObservableCollection<object> value) => Handlers.TryGetValue(key, out value);

        private Dictionary<Gesture, ObservableCollection<object>> Handlers = new Dictionary<Gesture, ObservableCollection<object>>();
        private Point LastTouchLocation;

        public void AddListener<T>(Gesture gesture, EventHandler<T> listener)
        {
            ObservableCollection<object> collection;
            if (!Handlers.TryGetValue(gesture, out collection))
            {
                collection = new ObservableCollection<object>();
                Handlers.Add(gesture, collection);
            }
            
            collection.Add(listener);
        }

        public void OnTouch(object sender, TouchEventArgs e)
        {
            LastTouchLocation = e.Point;
            Touch?.Invoke(sender, e);
        }

        public void GestureStarted(Gesture gesture, object sender) => GestureStarted(gesture, sender, new DiscreteTouchEventArgs(LastTouchLocation));

        public void GestureStarted<T>(Gesture gesture, object sender, T e)
        {
            ObservableCollection<object> collection;
            if (Handlers.TryGetValue(gesture, out collection))
            {
                foreach(object o in collection)
                {
                    if (o is EventHandler<T> handler)
                    {
                        handler(sender, e);
                    }
                }
            }
        }
    }

    public static class Gestures
    {
        public static BindableProperty TouchInterfaceProperty = BindableProperty.CreateAttached("InterceptedTouches", typeof(TouchInterface), typeof(Gestures), null, defaultValueCreator: value => new TouchInterface());

        //public static InterceptedTouches GetInterceptedTouches(this View view) => (InterceptedTouches)view.GetValue(InterceptedTouchesProperty);

        //public static void Recognized(this View view, BindableProperty property, )

        public static TouchInterface GetTouchInterface(this View view) => (TouchInterface)view.GetValue(TouchInterfaceProperty);

        public static void AddDiscreteTouchListener<T>(this View view, Gesture gesture, EventHandler<T> listener) where T : DiscreteTouchEventArgs, new() => GetTouchInterface(view)?.AddListener(gesture, listener);

        public static void AddContinuousTouchListener<T>(this View view, Gesture gesture, EventHandler<T> listener)
            where T : TouchEventArgs, new()
        {
            //DiscreteTouchEvent<T> touchEvent = (DiscreteTouchEvent<T>)view.GetValue(property);
            TouchInterface touchInterface = GetTouchInterface(view);

            void Invoke(object sender, TouchEventArgs e)
            {
                if (e.State == TouchState.Up)
                {
                    touchInterface.Touch -= Invoke;
                }

                if (e.State != TouchState.Down)
                {
                    listener(sender, new T
                    {
                        Point = e.Point,
                        State = e.State
                    });
                }
            }

            touchInterface.AddListener<DiscreteTouchEventArgs>(gesture, (sender, e) =>
            {
                listener(sender, e as T ?? new T
                {
                    Point = e.Point,
                    State = TouchState.Down
                });
                touchInterface.Touch += Invoke;
            });
        }

        public static void AddLongClickListener(this View view, EventHandler<DiscreteTouchEventArgs> listener) => AddDiscreteTouchListener(view, Gesture.LongClick, listener);
    }

    /*public abstract class TouchEvent<T> : ObservableCollection<DiscreteTouchEventDelegate<T>>
        where T : DiscreteTouchEventArgs, new()
    {
        //public event System.Extensions.SimpleEventHandler ListenersChanged;
        //public int Listeners => Touch?.GetInvocationList().Length ?? 0;

        //protected event DiscreteTouchEventDelegate<T> Touch;

        public void Attach(DiscreteTouchEventDelegate<T> listener)
        {
            Touch += listener;
            OnListenersChanged();
        }

        public void Detach(DiscreteTouchEventDelegate<T> listener)
        {
            Touch -= listener;
            OnListenersChanged();
        }

        private void OnListenersChanged()
        {
            ListenersChanged?.Invoke();
        }

        public virtual void OnTouch(object sender, T e)
        {
            if (Count == 0)
            {
                return;
            }
            
            Invoke(sender, e);

            /*Touch?.Invoke(sender, e);
            foreach(Delegate d in Touch.GetInvocationList())
            {
                d(sender, e);
            }
        }

        //public virtual T Aggregate(IEnumerable<T> args) => args.First();

        //public abstract T Clone(T e);

        private void Invoke(object sender, T e)
        {
            foreach(DiscreteTouchEventDelegate<T> listener in this.ToArray())
            {
                //T clone = Clone(e);
                listener.Invoke(sender, e);
                //yield return clone;
            }
        }
    }

    public class InterceptedTouches : TouchEvent<TouchEventArgs>
    {
        public Point LastTouch { get; private set; }

        public override void OnTouch(object sender, TouchEventArgs e)
        {
            base.OnTouch(sender, e);
            LastTouch = e.Point;
        }
    }

    public class DiscreteTouchEvent : DiscreteTouchEvent<DiscreteTouchEventArgs>
    {
        public DiscreteTouchEvent(View view) : base(view) { }
    }

    public class DiscreteTouchEvent<T> : TouchEvent<T>
        where T : DiscreteTouchEventArgs, new()
    {
        public InterceptedTouches Touches { get; private set; }
        protected View View;

        public DiscreteTouchEvent(View view)
        {
            View = view;
            Touches = View.GetInterceptedTouches();
        }

        public void Started(Point? point)
        {
            OnTouch(View, new T
            {
                Point = point ?? Touches.LastTouch
            });
        }
    }

    /*public class LongClickGestureRecognizer : Element, IGestureRecognizer
    {
        public LongClickEventHandler Listener;
        //public event LongClickEventHandler LongClick;

        //public bool Invoke(object sender, DiscreteTouchEventArgs e) => LongClick?.Invoke(sender, e) ?? false;
    }

    public interface INativeGestureRecognizerImplementation<T>
        //where T : IGestureRecognizer
    {
        object AddGestureRecognizer(View sharedView, object nativeView, T gestureRecognizer);
        void RemoveGestureRecognizer(View sharedView, object nativeView, T gestureRecognizer, object nativeHandler);
    }
    
    public static class Gestures
    {
        public static BindableProperty InterceptedTouchesProperty = BindableProperty.CreateAttached("InterceptedTouches", typeof(InterceptedTouches), typeof(Gestures), null, defaultValueCreator: value => new InterceptedTouches());

        public static BindableProperty GestureRecognizersProperty = BindableProperty.CreateAttached("GestureRecognizers", typeof(Collection), typeof(Gestures), null, defaultValueCreator: value => new Collection((View)value));

        //public static BindableProperty<INativeGestureRecognizerImplementation<LongClickGestureRecognizer>> NativeLongClickImplementationProperty = NativeImplementation.CreateAttached("NativeLongClickImplementation");

        private static Dictionary<Type, BindableProperty> Supported = new Dictionary<Type, BindableProperty>();

        static Gestures()
        {
            AddSupportFor<LongClickGestureRecognizer>("LongClick");
        }

        public static void AddSupportFor<T>(string name)
            where T : IGestureRecognizer
        {
            //BindableProperty<INativeGestureRecognizerImplementation<T>> property = NativeImplementation.CreateAttached("Native" + name + "Implementation");

            Supported.Add(typeof(T), BindableProperty.CreateAttached(name, typeof(INativeGestureRecognizerImplementation<T>), typeof(Gestures), null));
        }

        public static void SetNativeGestureRecognizerImplementation<T>(this Element element, INativeGestureRecognizerImplementation<T> implementation)
            where T : IGestureRecognizer
        {
            BindableProperty property;
            if (Supported.TryGetValue(typeof(T), out property))
            {
                element.SetValue(property, implementation);
            }
        }

        private static INativeGestureRecognizerImplementation<LongClickGestureRecognizer> GetImplementation(this View view, IGestureRecognizer gestureRecognizer)
        {
            BindableProperty property;
            if (Supported.TryGetValue(gestureRecognizer.GetType(), out property))
            {
                return (INativeGestureRecognizerImplementation<LongClickGestureRecognizer>)view.GetValue(property);
            }
            else
            {
                return null;
            }
        }

        //public static BindableProperty<Func<View, object, IGestureRecognizer, object>> NativeAddGestureRecognizerImplementationProperty = NativeImplementation.CreateAttached("NativeAddGestureRecognizerImplementation");

        //public static BindableProperty<Action<View, object, IGestureRecognizer, object>> NativeRemoveGestureRecognizerImplementationProperty = NativeImplementation.CreateAttached("NativeRemoveGestureRecognizerImplementation");

        public static IList<IGestureRecognizer> GetGestureRecognizers(this View view) => (Collection)view.GetValue(GestureRecognizersProperty);

        public static void AddGestureRecognizer(this View view, IGestureRecognizer gestureRecognizer) => GetGestureRecognizers(view).Add(gestureRecognizer);

        public class Collection : ObservableCollection<IGestureRecognizer>
        {
            private Dictionary<IGestureRecognizer, object> Lookup = new Dictionary<IGestureRecognizer, object>();
            private View View;
            private object NativeView = null;

            public Collection(View view)
            {
                View = view;
            }

            protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                base.OnCollectionChanged(e);

                Print.Log("collection changed", e.OldItems?.Count, e.NewItems?.Count);
                if (e.OldItems != null)
                {
                    foreach (IGestureRecognizer gestureRecognizer in e.OldItems)
                    {
                        //View.GetImplementation(gestureRecognizer)?.RemoveGestureRecognizer(View, NativeView, gestureRecognizer, Lookup[gestureRecognizer]);
                    }
                }
                if (e.NewItems != null)
                {
                    AddGestureRecognizers(e.NewItems);
                }
            }

            public void NativeControlCreated(object nativeView)
            {
                if (NativeView == nativeView)
                {
                    return;
                }
                NativeView = nativeView;

                //Print.Log("native view changed");
                AddGestureRecognizers(this);
            }

            private class Convert<T> : INativeGestureRecognizerImplementation<IGestureRecognizer>
            {
                private INativeGestureRecognizerImplementation<T> Value;

                public object AddGestureRecognizer(View sharedView, object nativeView, IGestureRecognizer gestureRecognizer) => Value.AddGestureRecognizer(sharedView, nativeView, (T)gestureRecognizer);

                public void RemoveGestureRecognizer(View sharedView, object nativeView, IGestureRecognizer gestureRecognizer, object nativeHandler) => Value.RemoveGestureRecognizer(sharedView, nativeView, (T)gestureRecognizer, nativeHandler);
            }

            private void AddGestureRecognizers(System.Collections.IEnumerable gestureRecognizers)
            {
                if (NativeView == null)
                {
                    return;
                }

                foreach (IGestureRecognizer gestureRecognizer in gestureRecognizers)
                {
                    Lookup[gestureRecognizer] = View.GetImplementation(gestureRecognizer)?.AddGestureRecognizer(View, NativeView, (LongClickGestureRecognizer)gestureRecognizer);

                    /*INativeGestureRecognizerImplementation<dynamic> implementation = View.GetImplementation(gestureRecognizer);// View.GetNativeImplementation(Gestures.NativeLongClickImplementationProperty);

                    if (implementation != null)
                    {
                        object nativeHandler = implementation.AddGestureRecognizer(View, NativeView, gestureRecognizer);

                        if (nativeHandler == null)
                        {
                            System.Diagnostics.Debug.WriteLine("Gesture recognizer " + gestureRecognizer + " not implemented natively on " + View.GetType());
                        }
                        else
                        {
                            
                        }
                    }
                }
            }

            /*private void RemoveGestureRecognizer(IGestureRecognizer gestureRecognizer)
            {
                object nativeHandler;

                if (Lookup.TryGetValue(gestureRecognizer, out nativeHandler))
                {
                    var implementation = View.GetNativeImplementation(Gestures.NativeLongClickImplementationProperty);

                    if (implementation == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Removal has not been implemented for gesture recognizer of type " + gestureRecognizer.GetType());
                    }
                    else
                    {
                        //implementation?.Invoke(View, NativeView, gestureRecognizer, nativeHandler);
                    }

                    Lookup.Remove(gestureRecognizer);
                }
            }
        }
    }*/

    public delegate bool TouchEventHandler(Point point, TouchState state);
    public delegate bool LongClickEventHandler(object sender, DiscreteTouchEventArgs e);

    public class AnyGestureRecognizer : Element, IGestureRecognizer
    {
        
    }

    public class DiscreteTouchEventArgs : EventArgs
    {
        public Point Point { get; set; }

        public DiscreteTouchEventArgs() { }

        public DiscreteTouchEventArgs(Point point) : this()
        {
            Point = point;
        }
    }

    public class TouchEventArgs : DiscreteTouchEventArgs
    {
        public TouchState State { get; set; }
        public bool ShouldEat { get; set; }

        public TouchEventArgs() : base() { }

        public TouchEventArgs(Point point, TouchState state) : base(point)
        {
            State = state;
        }
    }

    //public delegate void TouchEventHandler(Point point, TouchState state);
    public delegate void ClickEventHandler(Point point);
    //public delegate void LongClickEventHandler();

    public enum TouchState { Down, Up, Moving };

    public interface ITouchable
    {
        event EventHandler<TouchEventArgs> Touch;

        bool ShouldIntercept { get; }
        void OnTouch(Point point, TouchState state);
    }

    public static class TouchExtensions
    {
        public static void WhenTouched(this ITouchable touchable, TouchState state, Action<object, TouchEventArgs> action) => touchable.Touch += new System.Extensions.IfEventHandler<TouchEventArgs>((sender, e) => e.State == state, action);

        public static bool TryToTouch(this VisualElement v, Point point, TouchState state)
        {
            (v as ITouchable)?.OnTouch(point, state);
            return v is ITouchable;
        }

        /*public static void Add<T>(this ICollection<IGestureRecognizer> gestureRecognizers, Action<object, EventArgs> action)
            where T : IGestureRecognizer, new()
        {
            gestureRecognizers.Add(new T().);
        }*/
    }
}
