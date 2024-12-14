using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
    /*public class MenuItem<T> : MenuItem
        where T : Enum
    {
        public T Action { get; set; }

        public MenuItem() { }

        public MenuItem(MenuItem<T> other) : this()
        {
            Text = other.Text;
            IsDestructive = other.IsDestructive;
            Action = other.Action;
            IconImageSource = other.IconImageSource;
        }
    }*/

    //public delegate void ContextActionEventHandler<T>(object sender, EventArgs<T> e) where T : Enum;

    public class MenuItemTemplate
    {
        private Func<MenuItem> LoadTemplate;

        public MenuItemTemplate(Func<MenuItem> loadTemplate)
        {
            LoadTemplate = loadTemplate;
        }

        public MenuItem CreateContent() => LoadTemplate();
    }

    public class ActionableListView : ListView
    {
        public event EventHandler<EventArgs<MenuItem>> ContextActionClicked;

        public IList<MenuItemTemplate> ContextActions = new List<MenuItemTemplate>();

        public ActionableListView() : base() { }

        public ActionableListView(ListViewCachingStrategy strategy) : base(strategy) { }

        protected override void SetupContent(Cell content, int index)
        {
            base.SetupContent(content, index);
            
            foreach (MenuItemTemplate template in ContextActions)
            {
                MenuItem item = template.CreateContent();
                item.SetBinding(MenuItem.CommandParameterProperty, ".");
                item.Clicked += OnContextActionClicked;

                content.ContextActions.Add(item);
            }
        }

        private void OnContextActionClicked(object sender, EventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ContextActionClicked?.Invoke(item.CommandParameter, new EventArgs<MenuItem>(item));
        }
    }
}
