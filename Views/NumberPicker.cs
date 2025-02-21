using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using System.Extensions;

namespace Microsoft.Maui.Controls.Extensions
{
    public class NumberPicker : StackLayout
    {
        public event EventHandler<ChangedEventArgs<double>> ValueChanged;

        public static readonly BindableProperty ValueProperty = BindableProperty.Create("Value", typeof(double), typeof(NumberPicker), defaultBindingMode: BindingMode.TwoWay, coerceValue: (bindable, value) =>
        {
            NumberPicker picker = (NumberPicker)bindable;
            return Math.Min(picker.Max, Math.Max(picker.Min, (double)value));
        }, propertyChanged: OnValueChanged);

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public int Min
        {
            get { try { return int.Parse(MinLabel.Text); } catch { return 0; } }
            set
            {
                MinLabel.Text = value.ToString();
                ValueSlider.Minimum = value;
            }
        }
        public int Max
        {
            get { try { return int.Parse(MaxLabel.Text); } catch { return 0; } }
            set
            {
                MaxLabel.Text = value.ToString();
                ValueSlider.Maximum = value;
            }
        }

        private Slider ValueSlider;
        private Entry ValueEntry;
        private Button Decrease;
        private Button Increase;

        private Label MinLabel;
        private Label MaxLabel;

        private double Increment => Math.Pow(10, -Precision);
        private int Precision;

        public NumberPicker() : this(new Slider()) { }

        public NumberPicker(Slider slider)
        {
            Orientation = StackOrientation.Vertical;
            Spacing = 0;
            Precision = 0;

            MinLabel = new Label
            {
                VerticalOptions = LayoutOptions.Center,
                Text = slider.Minimum.ToString()
            };
            ValueSlider = slider;
            ValueSlider.VerticalOptions = LayoutOptions.Center;
            ValueSlider.HorizontalOptions = LayoutOptions.FillAndExpand;
            /*ValueSlider = new Slider
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Maximum = Max = max,
                Minimum = Min = min
            };*/
            ValueSlider.ValueChanged += (sender, e) => Value = e.NewValue;
            MaxLabel = new Label
            {
                VerticalOptions = LayoutOptions.Center,
                Text = slider.Maximum.ToString()
            };

            StackLayout coarseSelectionLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            coarseSelectionLayout.Children.Add(MinLabel);
            coarseSelectionLayout.Children.Add(ValueSlider);
            coarseSelectionLayout.Children.Add(MaxLabel);

            Decrease = new Button
            {
                VerticalOptions = LayoutOptions.Center,
                Text = "-"
            };
            Decrease.Clicked += (sender, e) => Value = Value - Increment;
            ValueEntry = new Entry
            {
                VerticalOptions = LayoutOptions.Center,
                Keyboard = Keyboard.Numeric,
                //MaxLength = 1 + Precision + (Precision > 0 ? 1 : 0),
                HorizontalTextAlignment = TextAlignment.Center,
                WidthRequest = 100
            };
            //entry.WidthRequest = entry.MaxLength * 10;
            ValueEntry.TextChanged += (sender, e) =>
            {
                try
                {
                    Value = double.Parse((sender as Entry).Text);
                }
                catch { }
            };
            Increase = new Button
            {
                VerticalOptions = LayoutOptions.Center,
                Text = "+"
            };
            Increase.Clicked += (sender, e) => Value = Value + Increment;

            StackLayout fineSelectionLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Center
            };
            fineSelectionLayout.Children.Add(Decrease);
            fineSelectionLayout.Children.Add(ValueEntry);
            fineSelectionLayout.Children.Add(Increase);

            Children.Add(coarseSelectionLayout);
            Children.Add(fineSelectionLayout);

            Value = Max;
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == IsEnabledProperty.PropertyName)
            {
                foreach (var ve in new Controls.VisualElement[] { ValueSlider, ValueEntry, Decrease, Increase })
                {
                    ve.IsEnabled = IsEnabled;
                }
            }
        }

        private static void OnValueChanged(object bindable, object oldValue, object newValue)
        {
            NumberPicker picker = (NumberPicker)bindable;
            double value = (double)newValue;

            value = Math.Round(value, picker.Precision);

            if (picker.ValueSlider.Value != value)
            {
                picker.ValueSlider.Value = value;
            }
            if (picker.ValueEntry.Text != value.ToString())
            {
                picker.ValueEntry.Text = value.ToString();
            }

            //Value = value;
            picker.ValueChanged?.Invoke(picker, new ChangedEventArgs<double>((double)oldValue, (double)newValue));
        }
    }
}
