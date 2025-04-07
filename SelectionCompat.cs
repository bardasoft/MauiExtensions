using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using Selectable = Microsoft.Maui.Controls.Compatibility.Layout<Microsoft.Maui.Controls.View>;
using SelectionList = System.Collections.Generic.IList<object>;

namespace Microsoft.Maui.Controls.Compatibility
{
    public static class Selection
    {
        [System.Flags]
        public enum SelectionMode
        {
            None = 0,
            Single = 1,
            Multiple = 2,
            Mandatory = 4,
            MandatorySingle = Single | Mandatory
        }

        public static readonly BindableProperty IsSelectedProperty = BindableProperty.CreateAttached("IsSelected", typeof(bool), typeof(VisualElement), false, propertyChanged: (b, o, n) =>
        {
            VisualStateManager.GoToState((VisualElement)b, (bool)n ? VisualStateManager.CommonStates.Selected : VisualStateManager.CommonStates.Normal);
        });

        public static readonly BindableProperty SelectionModeProperty = BindableProperty.CreateAttached(nameof(SelectionMode), typeof(SelectionMode), typeof(Selectable), SelectionMode.None, propertyChanged: (bindable, oldValue, newValue) =>
        {
            Selectable selectable = (Selectable)bindable;
            SelectionMode oldMode = (SelectionMode)oldValue;
            SelectionMode mode = (SelectionMode)newValue;

            if (oldMode == SelectionMode.Multiple)
            {
                selectable.GetSelectedItems().Clear();
            }

            if (mode == SelectionMode.None)
            {
                selectable.ChildAdded -= HandleSelection;
            }
            else
            {
                foreach (View view in selectable.Children)
                {
                    HandleSelection(selectable, new ElementEventArgs(view));
                }
                selectable.ChildAdded += HandleSelection;
            }
        });

        public static readonly BindableProperty SelectedItemsProperty = BindableProperty.CreateAttached("SelectedItems", typeof(SelectionList), typeof(Selectable), null, BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) =>
        {
            // TODO Needs to be updated to remove references to Compatibility namespace
            return;
            Selectable selectable = (Selectable)bindable;
            SelectionList oldList = (SelectionList)oldValue;
            SelectionList list = (SelectionList)newValue;

            if (oldValue != null && newValue == null)
            {
                selectable.ChildAdded -= ChildrenChanged;
                //selectable.ChildRemoved -= ChildrenChanged;
            }
            else if (oldValue == null && newValue != null)
            {
                selectable.ChildAdded += ChildrenChanged;
                //selectable.ChildRemoved += ChildrenChanged;
            }

            NotifyCollectionChangedEventHandler OnCollectionChanged = null;
            OnCollectionChanged = (object sender, NotifyCollectionChangedEventArgs e) =>
            {
                if (selectable.GetSelectedItems() != sender)
                {
                    if (sender is INotifyCollectionChanged collection)
                    {
                        collection.CollectionChanged -= OnCollectionChanged;
                    }

                    return;
                }

                SelectedItemsChanged(selectable, (SelectionList)sender);
            };

            if (oldList != null && oldList.Count > 0)
            {
                OnCollectionChanged(oldList, null);
            }
            if (list != null && list.Count > 0)
            {
                OnCollectionChanged(list, null);
            }

            /*if ((oldList?.Count ?? 0) > 0 || (list?.Count ?? 0) > 0)
            {
                OnCollectionChanged(list, null);
                //OnCollectionChanged(list, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, list?.ToArray() ?? new object[0], oldList?.ToArray() ?? new object[0]));
            }*/

            if (list is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += OnCollectionChanged;
            }
        });

        private static void ChildrenChanged(object sender, ElementEventArgs e)
        {
            var selectable = (Selectable)sender;
            SelectedItemsChanged(selectable, selectable.GetSelectedItems());
        }

        public static readonly BindableProperty SelectedIndicesProperty = BindableProperty.CreateAttached("SelectedIndices", typeof(IList<int>), typeof(Selectable), null, BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) =>
        {
            var selectable = (Selectable)bindable;
            //var selectedItems = selectable.GetSelectedItems();
            var indices = new HashSet<int>(Enumerable.Range(0, selectable.Children.Count).Where(index => selectable.Children[index].GetIsSelected()));
            indices.SymmetricExceptWith((IList<int>)newValue ?? Enumerable.Empty<int>());
            
            foreach (var index in indices)
            {
                if (index > 0 && index < selectable.Children.Count)
                {
                    ToggleSelected(selectable, selectable.Children[index]);
                }
            }
        });

        private static void Test(object state)
        {
            var stateTuple = ((NotifyCollectionChangedEventArgs, Selectable, int))state;
            NotifyCollectionChangedEventArgs e = stateTuple.Item1;
            Selectable selectable = stateTuple.Item2;
            int collectionCount = stateTuple.Item3;

            return;

            bool clear = e.Action == NotifyCollectionChangedAction.Reset || (e.Action == NotifyCollectionChangedAction.Replace && collectionCount == e.NewItems.Count);
            IEnumerable<View> deselect = clear ? selectable.Children : (e.OldItems == null ? null : selectable.Children.Where(view => e.OldItems.Contains(view.BindingContext)));

            if (deselect != null)
            {
                foreach (View view in deselect)
                {
                    Device.BeginInvokeOnMainThread(() => view.SetIsSelected(false));
                }
            }

            if (e.NewItems != null)
            {
                foreach (View view in selectable.Children.Where(view => e.NewItems.Contains(view.BindingContext)))
                {
                    Device.BeginInvokeOnMainThread(() => view.SetIsSelected(true));
                }
            }
        }

        public static readonly BindableProperty SelectionChangedCommandProperty = BindableProperty.CreateAttached("SelectionChangedCommand", typeof(ICommand), typeof(Selectable), null);

        public static readonly BindableProperty SelectionChangedCommandParameterProperty = BindableProperty.CreateAttached("SelectionChangedCommandParameter", typeof(object), typeof(Selectable), null);

        private static void HandleSelection(object sender, ElementEventArgs e)
        {
            var parameter = ((Selectable)sender, e.Element as VisualElement);

            if (parameter.Item2 != null && parameter.Item1.GetSelectionMode().HasFlag(SelectionMode.Mandatory) && parameter.Item1.GetSelectedItems()?.Count == 0)
            {
                ToggleSelected(parameter.Item1, parameter.Item2);
            }

            if (e.Element is Button button)
            {
                button.Command = ItemTappedCommand;
                button.CommandParameter = parameter;
            }
            else if (e.Element is View view)
            {
                view.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = ItemTappedCommand,
                    CommandParameter = parameter
                });
            }
        }

        private static Command ItemTappedCommand = new Command<(Selectable, VisualElement)>(parameter => ToggleSelected(parameter.Item1, parameter.Item2));

        private static void SelectedItemsChanged(Selectable selectable, SelectionList selectedItems)
        {
            if (selectedItems.Count == 0)
            {
                var selectionMode = selectable.GetSelectionMode();

                if (selectionMode.HasFlag(SelectionMode.Mandatory))
                {
                    selectable.SetSelectionMode(selectionMode & ~SelectionMode.Mandatory);
                }
            }
            else if (selectedItems.Count > 1)
            {
                selectable.SetSelectionMode(SelectionMode.Multiple);
            }

            var indices = new List<int>();

            for (int i = 0; i < selectable.Children.Count; i++)
            {
                var view = selectable.Children[i];
                var selected = selectedItems.Contains(view.BindingContext);

                view.SetIsSelected(selected);
                if (selected)
                {
                    indices.Add(i);
                }
            }

            selectable.SetSelectedIndices(indices);

            //var task = new System.Threading.Tasks.Task(Test, (e, selectable, collection.Count));
            //await System.Threading.Tasks.Task.Run(() => );
            //Test((e, selectable, collection.Count));
            OnSelectedItemsChanged(selectable);
        }

        private static void ToggleSelected(Selectable selectable, VisualElement view)
        {
            //Selectable selectable = parameter.Item1;
            //View view = parameter.Item2;

            var selectionMode = selectable.GetSelectionMode();
            if (selectionMode == SelectionMode.None)
            {
                return;
            }

            var selectedItems = selectable.GetSelectedItems();
            var command = selectable.GetSelectionChangedCommand();
            bool isSelected = !view.GetIsSelected();

            selectable.SetSelectionChangedCommand(null);

            if (isSelected)
            {
                //view.SetIsSelected(true);

                if (selectedItems == null)
                {
                    selectable.SetSelectedItems(selectedItems = new List<object>());
                }

                if (selectionMode.HasFlag(SelectionMode.Single) && selectedItems.Count == 1)
                {
                    selectedItems[0] = view.BindingContext;
                }
                else
                {
                    selectedItems.Add(view.BindingContext);
                }
            }
            else if (selectedItems != null && !(selectionMode.HasFlag(SelectionMode.Mandatory) && selectedItems.Count == 1))
            {
                //view.SetIsSelected(false);
                selectedItems.Remove(view.BindingContext);
            }
            else
            {
                return;
            }

            if (selectionMode.HasFlag(SelectionMode.Single) || !(selectedItems is INotifyCollectionChanged))
            {
                selectable.SetSelectedItems(new List<object>(selectedItems));
            }

            selectable.SetSelectionChangedCommand(command);
            OnSelectedItemsChanged(selectable);
        }

        private static void OnSelectedItemsChanged(Selectable selectable)
        {
            var command = selectable.GetSelectionChangedCommand();
            if (command == null)
            {
                return;
            }

            var commandParameter = selectable.GetSelectionChangedCommandParameter();

            if (command.CanExecute(commandParameter))
            {
                command.Execute(commandParameter);
            }
        }

        public static bool GetIsSelected(this VisualElement bindable) => (bool)bindable.GetValue(IsSelectedProperty);
        public static SelectionMode GetSelectionMode(this Selectable bindable) => (SelectionMode)bindable.GetValue(SelectionModeProperty);
        public static SelectionList GetSelectedItems(this Selectable bindable) => (SelectionList)bindable.GetValue(SelectedItemsProperty);
        public static IList<int> GetSelectedIndices(this Selectable bindable) => (IList<int>)bindable.GetValue(SelectedIndicesProperty);
        public static ICommand GetSelectionChangedCommand(this Selectable bindable) => (ICommand)bindable.GetValue(SelectionChangedCommandProperty);
        public static object GetSelectionChangedCommandParameter(this Selectable bindable) => bindable.GetValue(SelectionChangedCommandParameterProperty);

        public static void SetIsSelected(this VisualElement bindable, bool value) => bindable.SetValue(IsSelectedProperty, value);
        public static void SetSelectionMode(this Selectable bindable, SelectionMode value) => bindable.SetValue(SelectionModeProperty, value);
        public static void SetSelectedItems(this Selectable bindable, SelectionList value) => bindable.SetValue(SelectedItemsProperty, value);
        public static void SetSelectedIndices(this Selectable bindable, IList<int> value) => bindable.SetValue(SelectedIndicesProperty, value);
        public static void SetSelectionChangedCommand(this Selectable bindable, ICommand value) => bindable.SetValue(SelectionChangedCommandProperty, value);
        public static void SetSelectionChangedCommandParameter(this Selectable bindable, object value) => bindable.SetValue(SelectionChangedCommandParameterProperty, value);

        private static readonly Command ToggleSelectedCommand = new Command(sender =>
        {
            VisualElement visualElement = (VisualElement)sender;



            return;

            if (!visualElement.HasVisualStateGroups())
            {
                return;
            }

            var groups = (IList<VisualStateGroup>)visualElement.GetValue(VisualStateManager.VisualStateGroupsProperty);

            foreach (VisualStateGroup group in groups)
            {
                if (group.CurrentState?.Name == VisualStateManager.CommonStates.Normal)
                {
                    VisualStateManager.GoToState(visualElement, VisualStateManager.CommonStates.Selected);
                    return;
                }
                else if (group.CurrentState?.Name == VisualStateManager.CommonStates.Selected)
                {
                    VisualStateManager.GoToState(visualElement, VisualStateManager.CommonStates.Normal);
                    return;
                }
            }
        });
    }
}
