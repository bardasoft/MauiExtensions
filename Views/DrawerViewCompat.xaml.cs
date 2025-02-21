using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Microsoft.Maui.Controls.Compatibility
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DrawerView : ContentView
    {
        public static class VisualStates
        {
            public static readonly string Open = "Open";
            public static readonly string Closed = "Closed";
        }

        private static readonly BindablePropertyKey SnapPointsPropertyKey = BindableProperty.CreateReadOnly(nameof(SnapPoints), typeof(bool), typeof(DrawerView), null, defaultValueCreator: bindable =>
        {
            var drawer = (DrawerView)bindable;

            var snapPoints = new ObservableCollection<ISnapPoint>();
            snapPoints.CollectionChanged += (sender, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems.OfType<Element>())
                    {
                        //item.Parent = drawer.DrawerContentView;
                        item.Parent = drawer;
                    }
                }

                //drawer.ChangeSnapPoint((();
            };

            return snapPoints;
        });

        public static readonly BindableProperty SnapPointsProperty = SnapPointsPropertyKey.BindableProperty;

        public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(nameof(IsOpen), typeof(bool), typeof(DrawerView), true, propertyChanged: (bindable, oldValue, newValue) =>
        {
            var drawer = (DrawerView)bindable;
            //drawer.ChangeState();
        });

        /*public static readonly BindableProperty DrawerContentProperty = BindableProperty.Create(nameof(DrawerContent), typeof(object), typeof(DrawerView), propertyChanged: (bindable, oldValue, newValue) =>
        {
            DrawerView drawer = (DrawerView)bindable;
            drawer.LazyView = new Lazy<View>(() => newValue as View ?? (newValue as ElementTemplate)?.CreateContent() as View);
            drawer.OnPropertyChanged(nameof(DrawerContentView));
        });*/

        public IList<ISnapPoint> SnapPoints => (IList<ISnapPoint>)GetValue(SnapPointsProperty);

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        private static readonly Maui.Controls.Extensions.ViewFromTemplate<DrawerView> Items = new Maui.Controls.Extensions.ViewFromTemplate<DrawerView>((drawerView, view) =>
        {
            if (view == null)
            {
                drawerView.AbortAnimation(SNAP_ANIMATION_NAME);
            }

            drawerView.DrawerContentView = view;
            drawerView.OnPropertyChanged(nameof(DrawerContentView));
        }, nameof(Drawer), nameof(DrawerTemplate));

        public static readonly BindableProperty DrawerProperty = Items.ItemSourceProperty;
        public static readonly BindableProperty DrawerTemplateProperty = Items.ItemTemplateProperty;

        public object Drawer
        {
            get => GetValue(DrawerProperty);
            set => SetValue(DrawerProperty, value);
        }

        public ElementTemplate DrawerTemplate
        {
            get => (ElementTemplate)GetValue(DrawerTemplateProperty);
            set => SetValue(DrawerTemplateProperty, value);
        }

        /*public object DrawerContent
        {
            get => GetValue(DrawerContentProperty);
            set => SetValue(DrawerContentProperty, value);
        }*/

        public View DrawerContentView { get; private set; }

        public ICommand Toggle { get; private set; }
        public ICommand NextSnapPointCommand { get; }

        private SwipeGestureRecognizer Swipe { get; }

        public DrawerView()
        {
            Toggle = new Command(() => IsOpen = !IsOpen);
            NextSnapPointCommand = new Command(() => ToggleSnapPoint(1, true));

            Swipe = new SwipeGestureRecognizer
            {
                Direction = SwipeDirection.Down | SwipeDirection.Up,
            };
            Swipe.Swiped += Swiped;

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == IsOpenProperty.PropertyName || e.PropertyName == nameof(DrawerContentView))
                {
                    ChangeState();
                    foreach (var snapPoint in SnapPoints.OfType<Element>())
                    {
                        snapPoint.Parent = this;
                    }

                    if (SnapPoints.Count > 0)
                    {
                        SnapTo(SnapPoints.First(), false);
                    }
                }
            };

            //InitializeComponent();
            ChangeState();
            //IsOpenPropertyChanged(this, true, false);
        }

        private void Swiped(object sender, SwipedEventArgs e)
        {
            int direction = e.Direction == SwipeDirection.Down ? -1 : 1;
            ToggleSnapPoint(direction, true);
        }

        private void ToggleSnapPoint(int step, bool animated) => SnapTo(SnapPoints[(NearestSnapPointIndex(DrawerContentView.Height) + step) % SnapPoints.Count], animated);

        private const string SNAP_ANIMATION_NAME = "Snap";

        public void SnapTo(ISnapPoint snapPoint, bool animated)
        {
            if (DrawerContentView == null)
            {
                return;
            }

            if (snapPoint != SnapPoints.LastOrDefault())
            {
                ChangeState(snapPoint);
            }

            var animation = new Animation(value => DrawerContentView.HeightRequest = value, DrawerContentView.Height, snapPoint.Value, Easing.SpringOut);
            animation.Commit(this, SNAP_ANIMATION_NAME, length: animated ? 250u : 0, finished: (value, cancelled) => Snapped(snapPoint));
            //HeightRequest = SnapPoints[index % SnapPoints.Count].Value;
        }

        private void Snapped(ISnapPoint snapPoint)
        {
            DrawerContentView.SetBinding(HeightRequestProperty, new Binding(nameof(ISnapPoint.Value), source: snapPoint));
            ChangeState(snapPoint);
        }

        private void ChangeState(ISnapPoint snapPoint)
        {
            var state = snapPoint == SnapPoints.LastOrDefault() ? VisualStates.Open : VisualStates.Closed;

            if (VisualStateManager.GoToState(DrawerContentView, state))
            {
                return;
                if (state == VisualStates.Open)
                {
                    DrawerContentView.GestureRecognizers.Remove(Swipe);
                }
                else if (!DrawerContentView.GestureRecognizers.Contains(Swipe))
                {
                    DrawerContentView.GestureRecognizers.Add(Swipe);
                }
            }
        }

        public ISnapPoint NearestSnapPoint(double value) => SnapPoints[NearestSnapPointIndex(value) % SnapPoints.Count];
        public int NearestSnapPointIndex(double value)
        {
            var snapPoints = new List<double>(SnapPoints.Select(snapPoint => snapPoint.Value));

            //snapPoints.Add(Height);
            snapPoints.Sort();

            var index = snapPoints.BinarySearch(value);

            if (index < 0)
            {
                index = ~index;

                if (index != 0 && (index == snapPoints.Count || value - snapPoints[index - 1] < snapPoints[index] - value))
                {
                    index--;
                }
            }

            return index;
        }

        private void ChangeState()
        {
            if (DrawerContentView != null)
            {
                VisualStateManager.GoToState(DrawerContentView, IsOpen ? VisualStates.Open : VisualStates.Closed);
            }
        }

        //private static void IsOpenPropertyChanged(BindableObject bindable, object oldValue, object newValue) => VisualStateManager.GoToState((VisualElement)bindable, (bool)newValue ? VisualStates.Open : VisualStates.Closed);
    }
}