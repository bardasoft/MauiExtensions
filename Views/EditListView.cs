using System;
using Microsoft.Maui.Layouts;

using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility
{
    public class EditCell : EditListView.EditCell { }

    public class EditListView : StackLayout
    {
        public static BindableProperty EditingProperty = BindableProperty.Create("Editing", typeof(bool), typeof(EditListView), false);

        public bool Editing
        {
            get => (bool)GetValue(EditingProperty);
            set => SetValue(EditingProperty, value);
        }

        private event SimpleEventHandler DeleteSelected;

        public readonly ListView ListView;
        public readonly Layout<View> EditingToolbar;
        public readonly Button Add;
        public readonly Button Edit;

        private readonly StackLayout Header;
        private readonly ContentView HeaderView;

        public EditListView() : this(new ActionableListView
        {
            ContextActions =
            {
                new MenuItemTemplate(() => new MenuItem
                {
                    Text = "Delete",
                    IsDestructive = true
                })
            }
        })
        { }

        private EditListView(ListView listView)
        {
            Orientation = StackOrientation.Vertical;
            Spacing = 0;
            ListView = listView;
            ListView.VerticalOptions = LayoutOptions.FillAndExpand;
            ListView.HeightRequest = 1;

            Header = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Spacing = 0,
                Children =
                {
                    (HeaderView = new ContentView()),
                    new FlexLayout
                    {
                        Direction = FlexDirection.Row,
                        Wrap = FlexWrap.NoWrap,
                        JustifyContent = FlexJustify.SpaceBetween,
                        AlignItems = FlexAlignItems.Center,
                        Padding = new Thickness(0, 10),
                        Children =
                        {
                            (Edit = new Button
                            {
                                FontSize = NamedSize.Body.On<Button>()
                            }),
                            (Add = new Button
                            {
                                HorizontalOptions = LayoutOptions.EndAndExpand,
                                Text = "+",
                                FontSize = NamedSize.Title.On<Button>(),
                            })
                        }
                    }
                }
            };

            Button delete;

            Children.Add(ListView);
            Children.Add(EditingToolbar = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                BackgroundColor = Colors.LightGray,
                IsVisible = false,
                Children =
                {
                    (delete = new Button
                    {
                        HorizontalOptions = LayoutOptions.EndAndExpand,
                        Text = "Delete",
                        FontSize = NamedSize.Body.On<Button>()
                    })
                }
            });

            EditingToolbar.SetBinding(IsVisibleProperty, this, "Editing");

            Edit.Clicked += (sender, e) => Editing = !Editing;
            Edit.SetBinding<string, bool>(Button.TextProperty, this, "Editing", value => value ? "Cancel" : "Edit");
            Edit.SetBinding<bool, int>(IsEnabledProperty, listView.ItemsSource, "Count", value => value > 0);

            delete.Clicked += (sender, e) => OnDeleteSelected();

            //ListView.SetBinding(Xamarin.Forms.ListView.HeaderProperty, this, "Header", mode: BindingMode.TwoWay);
            //ListView.WhenPropertyChanged(Xamarin.Forms.ListView.HeaderProperty, HeaderChanged);
            HeaderChanged(null, null);
            
            ListView.ItemTemplate.SetBinding(EditCell.EditingProperty, new Binding("Editing", source: this));
            if (ListView is ActionableListView actionableListView)
            {
                actionableListView.ContextActionClicked += (sender, e) =>
                {
                    if (e.Value.IsDestructive)
                    {
                        OnDelete(sender);
                    }
                };
            }
        }

        public static implicit operator EditListView(ListView listView) => new EditListView(listView);

        private void OnDelete(object sender)
        {
            try
            {
                ((dynamic)ListView.ItemsSource).Remove((dynamic)sender);
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Failed to remove item from the ListView. " + ListView.ItemsSource.GetType() + " does not support removal of " + sender.GetType() + " or does not implement INotifyCollectionChanged. Consider using an ObservableCollection");
            }
        }

        private void HeaderChanged(object sender, EventArgs e)
        {
            if (ListView.Header == HeaderView.Content)
            {
                return;
            }

            HeaderView.Content = ListView.Header as View;
            ListView.Header = Header;
        }

        private void OnDeleteSelected()
        {
            DeleteSelected?.Invoke();
            Editing = false;
        }

        public class EditCell : ViewCell
        {
            public static BindableProperty EditingProperty = BindableProperty.Create("Editing", typeof(bool), typeof(EditListView), false, propertyChanged: OnEditingPropertyChanged);

            public bool Editing
            {
                get => (bool)GetValue(EditingProperty);
                set => SetValue(EditingProperty, value);
            }

            public static BindableProperty ViewProperty = BindableProperty.Create("View", typeof(View), typeof(EditCell), propertyChanged: OnViewPropertyChanged);

            new public View View
            {
                get => (View)ViewHolder.GetValue(ContentView.ContentProperty);
                set => ViewHolder.SetValue(ContentView.ContentProperty, value);
            }

            private readonly CheckBox Selected;

            private readonly ContentView ViewHolder = new ContentView();

            public EditCell()
            {
                base.View = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Margin = new Thickness(10, 0, 10, 0),
                    Children =
                    {
                        (Selected = new CheckBox { }),
                        ViewHolder,
                    },
                };

                ViewHolder.HorizontalOptions = LayoutOptions.FillAndExpand;
                Selected.SetBinding<double, bool>(WidthRequestProperty, this, "Editing", value => value ? -1 : 0);

                Selected.WhenPropertyChanged(CheckBox.IsCheckedProperty, (sender, e) =>
                {
                    if (!(this.Parent<EditListView>() is EditListView parent))
                    {
                        return;
                    }

                    if (Selected.IsChecked)
                    {
                        parent.DeleteSelected += DeleteMe;
                    }
                    else
                    {
                        parent.DeleteSelected -= DeleteMe;
                    }
                });
            }

            protected override void OnTapped()
            {
                base.OnTapped();

                if (Editing)
                {
                    Selected.IsChecked = !Selected.IsChecked;
                }
            }

            private void DeleteMe()
            {
                Selected.IsChecked = false;
                this.Parent<EditListView>()?.OnDelete(BindingContext);
            }

            private static void OnEditingPropertyChanged(BindableObject bindable, object oldValue, object newValue)
            {
                EditCell cell = (EditCell)bindable;

                if (!(bool)newValue)
                {
                    cell.Selected.IsChecked = false;
                }
            }

            private static void OnViewPropertyChanged(BindableObject bindable, object oldValue, object newValue) => ((EditCell)bindable).View = (View)newValue;
        }
    }
}
