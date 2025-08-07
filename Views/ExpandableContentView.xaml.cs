using System.Windows.Input;

namespace Microsoft.Maui.Controls;

public partial class ExpandableContentView : ContentView
{
    public static readonly string COLLAPSED_STATE = "Collapsed";
    public static readonly string EXPANDED_STATE = "Expanded";

    public static readonly BindableProperty CollapsedHeightProperty = BindableProperty.Create(nameof(CollapsedHeight), typeof(double), typeof(ExpandableContentView), -1d);

    public static readonly BindableProperty CollapseHintProperty = BindableProperty.Create(nameof(CollapseHint), typeof(string), typeof(ExpandableContentView), null);

    public static readonly BindableProperty IsExpandedProperty = BindableProperty.Create(nameof(IsExpanded), typeof(bool), typeof(ExpandableContentView), false, propertyChanged: IsExpandedPropertyChanged);

    public static readonly BindableProperty ExpandHintProperty = BindableProperty.Create(nameof(ExpandHint), typeof(string), typeof(ExpandableContentView), null);

    public bool CanExpand
    {
        get => _CanExpand;
        set
        {
            if (_CanExpand != value)
            {
                OnPropertyChanging(nameof(CanExpand));
                _CanExpand = value;
                OnPropertyChanged(nameof(CanExpand));
            }
        }
    }

    public double CollapsedHeight
    {
        get => (double)GetValue(CollapsedHeightProperty);
        set => SetValue(CollapsedHeightProperty, value);
    }

    public string CollapseHint
    {
        get => (string)GetValue(CollapseHintProperty);
        set => SetValue(CollapseHintProperty, value);
    }

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public string ExpandHint
    {
        get => (string)GetValue(ExpandHintProperty);
        set => SetValue(ExpandHintProperty, value);
    }

    public ICommand ToggleExpandedCommand { get; }

    private bool _CanExpand;

    public ExpandableContentView()
    {
        InitializeComponent();

        ToggleExpandedCommand = new Command(() =>
        {
            IsExpanded = !IsExpanded;
        });

        IsExpandedPropertyChanged(this, true, false);
    }

    private static void IsExpandedPropertyChanged(object bindable, object oldValue, object newValue)
    {
        var ecv = (ExpandableContentView)bindable;
        VisualStateManager.GoToState(ecv, (bool)newValue ? EXPANDED_STATE : COLLAPSED_STATE);

        if ((bool)newValue)
        {
            ecv.HeightRequest = -1;
        }
        else if (ecv.IsSet(CollapsedHeightProperty))
        {
            ecv.SetBinding(HeightRequestProperty, new Binding
            {
                Path = nameof(CollapsedHeight),
                Source = ecv
            });
        }
    }
}