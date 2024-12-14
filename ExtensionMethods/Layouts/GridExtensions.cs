using System;
using System.Collections.Generic;
using System.Globalization;

using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility
{
    public class FillGridBehavior : Behavior<Grid>
    {
        public enum Fill { Rows, Columns }

        public Fill FillPattern { get; set; }

        protected override void OnAttachedTo(Grid bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.ChildAdded += ChildAdded;
        }

        protected override void OnDetachingFrom(Grid bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.ChildAdded -= ChildAdded;
        }

        private void ChildAdded(object sender, ElementEventArgs e)
        {
            if (e.Element.IsSet(Grid.ColumnProperty) || e.Element.IsSet(Grid.RowProperty))
            {
                return;
            }

            Grid grid = (Grid)sender;
            int max = FillPattern == Fill.Columns ? grid.RowDefinitions.Count : grid.ColumnDefinitions.Count;
            int index = grid.Children.Count - 1;

            GridExtensions.SetPos(e.Element, index % max, index / max);
        }
    }

    /*public class GridDefinition
    {
        public ColumnDefinitionCollection ColumnDefinitions { get; set; } = new ColumnDefinitionCollection();

        public RowDefinitionCollection RowDefinitions { get; set; } = new RowDefinitionCollection();

        public GridDefinition(int columns = 0, int rows = 0)
        {
            for (int i = 0; i < columns; i++)
            {
                ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (int i = 0; i < rows; i++)
            {
                RowDefinitions.Add(new RowDefinition());
            }
        }
    }*/

    public class CoerceDefinitionValue : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Math.Max(0, (double)value);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }

    public static class GridExtensions
    {
        public static readonly CoerceDefinitionValue CoerceDefinitionValue = new CoerceDefinitionValue();

        public static void Add(this ColumnDefinitionCollection columns, GridLength value) => columns.Add(new ColumnDefinition { Width = value });

        public static void Add(this RowDefinitionCollection rows, GridLength value) => rows.Add(new RowDefinition { Height = value });

        private static void Add<T>(this DefinitionCollection<T> definitions, double value)
            where T : BindableObject, IDefinition, new()
        {
            T t = new T();
            t.SetValue(value);
            definitions.Add(t);
        }

        public static void SetValue<T>(this T definition, double value)
            where T : BindableObject, IDefinition => definition.SetValue(definition is ColumnDefinition ? ColumnDefinition.WidthProperty : RowDefinition.HeightProperty, value);

        public enum AutoSize { None, Width, Height };

        private static BindableProperty AutoSizeProperty = BindableProperty.Create("AutoSize", typeof(AutoSize), typeof(Grid), AutoSize.None, propertyChanged: OnAutoSizePropertyChanged);

        private static BindableProperty LayoutDataProperty = BindableProperty.Create("LayoutData", typeof((Size, Size)), typeof(Grid));

        public static AutoSize GetAutoSize(Grid grid) => (AutoSize)grid.GetValue(AutoSizeProperty);

        public static double GetAutoSizeRatio(Grid grid) => (((AutoSize, double))grid.GetValue(AutoSizeProperty)).Item2;

        public static void SetAutoSize(Grid grid, AutoSize value, double ratio = 1) => grid.SetValue(AutoSizeProperty, value);

        private static void OnAutoSizePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            Grid grid = (Grid)bindable;

            if ((AutoSize)oldValue == AutoSize.None)
            {
                UpdateLayoutData(grid, null);
                grid.LayoutChanged += UpdateLayoutData;

                grid.PropertyChanged += OnSizeChanged;
            }

            AutoSize value = (AutoSize)newValue;
            if (value == AutoSize.None)
            {
                grid.PropertyChanged -= OnSizeChanged;
                grid.LayoutChanged -= UpdateLayoutData;
            }
            else
            {
                OnSizeChanged(grid, new System.ComponentModel.PropertyChangedEventArgs(LayoutDataProperty.PropertyName));
            }
        }

        private static void UpdateLayoutData(object sender, EventArgs e)
        {
            Grid grid = (Grid)sender;
            Size absolute;
            Size proportional;

            Deconstruct(grid, out absolute, out proportional);
            grid.SetValue(LayoutDataProperty, (absolute, proportional));
        }

        public static SizeRequest Measure(this Grid grid, double widthConstraint, double heightConstraint, AutoSize autoSize, MeasureFlags measureFlags = MeasureFlags.None)
        {
            SizeRequest sizeRequest = new SizeRequest(new Size(widthConstraint, heightConstraint), new Size(40, 40));// grid.Measure(widthConstraint, heightConstraint, measureFlags);
            UpdateLayoutData(grid, null);
            (Size, Size) data = ((Size, Size))grid.GetValue(LayoutDataProperty);
            double ratio = 1;

            Size absolute = data.Item1;
            Size proportional = data.Item2;

            Size request = sizeRequest.Request;
            if (autoSize == AutoSize.Width)
            {
                request.Width = proportional.Height == 0 ? 0 : ((heightConstraint - grid.Padding.VerticalThickness - absolute.Height) / proportional.Height * proportional.Width * ratio);
                request.Width += absolute.Width + grid.Padding.HorizontalThickness;
            }
            if (autoSize == AutoSize.Height)
            {
                request.Height = proportional.Width == 0 ? 0 : ((widthConstraint - grid.Padding.HorizontalThickness - absolute.Width) / proportional.Width * proportional.Height * ratio);
                request.Height += absolute.Height + grid.Padding.VerticalThickness;
            }

            sizeRequest.Request = new Size(Math.Min(widthConstraint, request.Width), Math.Min(heightConstraint, request.Height));
            return sizeRequest;
        }

        private static void OnSizeChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Grid grid = (Grid)sender;
            AutoSize value = GetAutoSize(grid);

            if (value == AutoSize.Width && (e.PropertyName == LayoutDataProperty.PropertyName || e.PropertyName == VisualElement.HeightProperty.PropertyName))
            {
                grid.WidthRequest = Measure(grid, double.PositiveInfinity, grid.Height, value).Request.Width;
            }
            if (value == AutoSize.Height && (e.PropertyName == LayoutDataProperty.PropertyName || e.PropertyName == VisualElement.WidthProperty.PropertyName))
            {
                grid.HeightRequest = Measure(grid, grid.Width, double.PositiveInfinity, value).Request.Height;
            }
        }

        public static double CalculateStar(this ColumnDefinitionCollection definitions, double size, double spacing)
        {
            double a;
            double p;
            DeconstructColumns(definitions, spacing, out a, out p);
            return (size - a) / p;
        }

        public static void Deconstruct(this Grid grid, out Size absolute, out Size proportional)
        {
            absolute = new Size();
            proportional = new Size();
            double abs, pro;

            DeconstructColumns(grid.ColumnDefinitions, grid.ColumnSpacing, out abs, out pro);
            absolute.Width = abs;
            proportional.Width = pro;

            DeconstructRows(grid.RowDefinitions, grid.RowSpacing, out abs, out pro);
            absolute.Height = abs;
            proportional.Height = pro;
        }

        public static void DeconstructColumns(this ColumnDefinitionCollection columns, double columnSpacing, out double absolute, out double proportional) => Deconstruct(columns, column => column.Width, columnSpacing, out absolute, out proportional);

        public static void DeconstructRows(this RowDefinitionCollection rows, double rowSpacing, out double absolute, out double proportional) => Deconstruct(rows, row => row.Height, rowSpacing, out absolute, out proportional);

        private static void Deconstruct<T>(DefinitionCollection<T> definitions, Func<T, GridLength> getGridLength, double spacing, out double absolute, out double proportional)
            where T : IDefinition
        {
            absolute = ((definitions?.Count - 1) * spacing) ?? 0;
            proportional = 0;

            if (definitions == null)
            {
                return;
            }

            foreach (T t in definitions)
            {
                GridLength gridLength = getGridLength(t);

                if (gridLength.GridUnitType == GridUnitType.Absolute)
                {
                    absolute += gridLength.Value;
                }
                else if (gridLength.GridUnitType == GridUnitType.Star)
                {
                    proportional += gridLength.Value;
                }
            }
        }

        public static BindableProperty IsTransposedProperty = BindableProperty.CreateAttached("IsTransposed", typeof(bool), typeof(Grid), false, propertyChanging: IsTransposedPropertyChanging);

        public static bool GetIsTransposed(Grid grid) => (bool)grid.GetValue(IsTransposedProperty);

        public static void SetIsTransposed(Grid grid, bool value) => grid.SetValue(IsTransposedProperty, value);

        public static void Transpose(this Grid grid) => SetIsTransposed(grid, !GetIsTransposed(grid));

        private static void IsTransposedPropertyChanging(BindableObject bindable, object oldValue, object newValue)
        {
            Grid grid = (Grid)bindable;
            
            RowDefinitionCollection rowDefintions = new RowDefinitionCollection();
            foreach (ColumnDefinition cd in grid.ColumnDefinitions)
            {
                rowDefintions.Add(new RowDefinition
                {
                    Height = cd.Width
                });
            }

            ColumnDefinitionCollection columnDefinitions = new ColumnDefinitionCollection();
            foreach (RowDefinition rd in grid.RowDefinitions)
            {
                columnDefinitions.Add(new ColumnDefinition
                {
                    Width = rd.Height
                });
            }

            grid.RowDefinitions = rowDefintions;
            grid.ColumnDefinitions = columnDefinitions;

            /*Tuple<int, int>[] spans = new Tuple<int, int>[grid.Children.Count];
            for (int i = 0; i < Children.Count; i++)
            {
                spans[i] = new Tuple<int, int>(GetRowSpan(Children[i]), GetColumnSpan(Children[i]));
            }
            for (int i = 0; i < Children.Count; i++)
            {
                SetRowSpan(Children[i], spans[i].Item2);
                SetColumnSpan(Children[i], spans[i].Item1);
            }*/

            foreach (View child in grid.Children)
            {
                SetPos(child, Grid.GetColumn(child), Grid.GetRow(child));
                int temp = Grid.GetRowSpan(child);
                Grid.SetRowSpan(child, Grid.GetColumnSpan(child));
                Grid.SetColumnSpan(child, temp);

                /*Tuple<int, int> pos = new Tuple<int, int>(Grid.GetRow(child), Grid.GetColumn(child));
                Grid.SetRow(child, pos.Item2);
                Grid.SetColumn(child, pos.Item1);*/
            }
        }

        public static void SetPos(BindableObject bindable, int left, int top)
        {
            Grid.SetColumn(bindable, left);
            Grid.SetRow(bindable, top);
        }

        public static void SetPos(BindableObject bindable, int left, int right, int top, int bottom)
        {
            SetPos(bindable, left, top);
            Grid.SetColumnSpan(bindable, right - left);
            Grid.SetRowSpan(bindable, bottom - top);
        }

        private static void Animate()
        {
            ColumnDefinitionCollection start = new ColumnDefinitionCollection
            {
                new ColumnDefinition(),
                new ColumnDefinition(),
                new ColumnDefinition(),
                new ColumnDefinition(),
                new ColumnDefinition(),
            };
            ColumnDefinitionCollection ending = new ColumnDefinitionCollection();
            ending.AddRange(start);
            //ending.RemoveAt(4);
            //ending.Insert(1, new ColumnDefinition());
            //ending.Insert(3, new ColumnDefinition());
            ending[2] = null;

            int max = Math.Max(start.Count, ending.Count);
            List<double> startValues = new List<double>(max);
            List<double> endValues = new List<double>();
            foreach (ColumnDefinition cd in start)
            {
                startValues.Add(cd.Width.Value);
            }
            foreach (ColumnDefinition cd in ending)
            {
                endValues.Add(cd.Width.Value);
            }

            Print.Log("starting", startValues.Count, endValues.Count);
            for (int i = 0; startValues.Count != endValues.Count; i++)
            {
                if (start.Count > ending.Count)
                {
                    if (!ending.Contains(start[i]))
                    {
                        endValues.Insert(i, 0);
                    }
                }
                else if (ending.Count > start.Count)
                {
                    if (!start.Contains(ending[i]))
                    {
                        startValues.Insert(i, 0);
                    }
                }
            }
            Print.Log("done");
            for (int i = 0; i < max; i++)
            {
                Print.Log("\t" + startValues[i], endValues[i]);
            }
        }
    }

    /*public abstract class RelativeGridPositionExtension : IMarkupExtension<BindingBase>
    {
        public int Offset { get; set; }

        public object Source { get; set; }

        public BindingBase ProvideValue(IServiceProvider serviceProvider)
        {
            var valueProvider = serviceProvider?.GetService<IProvideValueTarget>() ?? throw new ArgumentException();

            BindableProperty property = (valueProvider.TargetObject as Setter)?.Property ?? valueProvider.TargetProperty as BindableProperty;

            return new Binding
            {
                Source = Source,
                Converter = CrunchKeyboard.IndexFromEnd,
                ConverterParameter = Offset
            };
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
    }*/
}
