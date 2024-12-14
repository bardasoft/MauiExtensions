using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui.Controls
{
    public class DrawerPage : FlyoutPage
    {
        public static readonly BindableProperty CollapsedProperty = BindableProperty.Create("Collapsed", typeof(bool), typeof(DrawerPage));

        public bool Collapsed
        {
            get { return (bool)GetValue(CollapsedProperty); }
            private set { SetValue(CollapsedProperty, value); }
        }

        /*new public Page Detail
        {
            get => BackupDetail;
            set
            {
                base.Detail = BackupDetail = value;
                //CollapsedNavigation = value as NavigationPage ?? new NavigationPage(value);
            }
        }*/

        //private NavigationPage CollapsedNavigation;
        private Page BackupDetail;
        private Page DetailRoot;
        private Page MasterRoot;

        new public bool IsPresented
        {
            get => Collapsed ? ((Detail as NavigationPage)?.Navigation.NavigationStack.Contains(MasterRoot) ?? false) : IsNativeDrawerShowing;
            set
            {
                if (value == IsPresented)
                {
                    //return;
                }

                //Print.Log("trying to set ispresented to " + value, Collapsed);

                if (value != IsPresented && Collapsed)
                {
                    if (value)
                    {
                        PushAll(Detail as NavigationPage, AllPages(base.Flyout));
                        (base.Flyout as NavigationPage)?.PopToRootAsync();
                        //Navigation.PushAsync(Master, false);
                    }
                    else
                    {
                        (Detail as NavigationPage)?.PopToRootAsync(false);
                    }
                }
                else
                {
                    IsNativeDrawerShowing = value;
                }
            }
        }

        private bool IsNativeDrawerShowing
        {
            get => base.IsPresented;
            set
            {
                base.IsPresented = !value;
                base.IsPresented = value;
            }
        }

        public DrawerPage()
        {
            CollapsedChanged(Device.Idiom != TargetIdiom.Tablet);
        }

        public void CollapsedChanged(bool collapsed)
        {
            if (collapsed == Collapsed)
            {
                return;
            }
            
            Print.Log("app layout changed", collapsed, IsPresented);

            bool wasPresented = IsPresented;

            if (collapsed)
            {
                Combine();
            }
            else
            {
                Separate();
            }

            /*if (collapsed && IsPresented)
            {
                IsPresented = false;
                //FullNavigation.PushAsync(new SettingsPage());
            }
            else if (!collapsed && Detail.CurrentPage == Master)
            {
                Detail.PopAsync();
            }
            else
            {
                //Root.IsPresented = true;
                return;
            }

            ShowSettings();*/

            //Print.Log("is presented before changed", IsPresented, Collapsed, Navigation.NavigationStack.Contains(MasterRoot), (Detail as NavigationPage).Navigation.NavigationStack.Contains(MasterRoot), MasterRoot, Navigation);
            if (IsPresented)
            {
                IsPresented = false;
            }

            Collapsed = collapsed;

            if (wasPresented)
            {
                IsPresented = true;
            }
            
            //Print.Log("set collapsed to " + collapsed, Collapsed);
            //IsLayoutCondensed = condensed;
            //UILayoutChanged?.Invoke(this, new ToggledEventArgs(condensed));
        }

        public Task PushAsync(Page page) => Collapsed ? (Detail as NavigationPage).PushAsync(page) : (Flyout as NavigationPage).PushAsync(page);

        private Page RootPage(Page page) => (page as NavigationPage)?.RootPage ?? page;
        private IEnumerable<Page> AllPages(Page page)
        {
            if (page is NavigationPage nav)
            {
                foreach(Page page1 in nav.Navigation.NavigationStack)
                {
                    yield return page1;
                }
            }
            else
            {
                yield return page;
            }
        }

        private void PushAll(NavigationPage nav, IEnumerable<Page> pages, bool animated = true)
        {
            foreach(Page page in pages)
            {
                nav.PushAsync(page, animated);
            }
        }

        protected virtual void Combine()
        {
            MasterRoot = RootPage(Flyout);
            DetailRoot = RootPage(BackupDetail = Detail);

            NavigationPage collapsedNavigation = new NavigationPage();
            //collapsedNavigation.Navigation.PushModalAsync(DetailRoot);
            //Detail = collapsedNavigation;
            //(Detail as NavigationPage).PopAsync(false);
            //collapsedNavigation.PushAsync(DetailRoot, false);
            //collapsedNavigation.PushAsync(new ContentPage { Title = "test", BackgroundColor = Color.Purple }, false);

            PushAll(collapsedNavigation, AllPages(Detail));
            (Detail as NavigationPage)?.PopToRootAsync(false);
            //(Detail as NavigationPage)?.ReplaceCurrentPageAsync(new ContentPage { Title = "test" });
            //(Detail as NavigationPage)?.Navigation.RemovePage((Detail as NavigationPage).CurrentPage);
            //Print.Log("stack count", (Detail as NavigationPage).Navigation.NavigationStack.Count);
            //PushAll(collapsedNavigation, AllPages(Master));

            //Detail = collapsedNavigation;
        }

        protected virtual void Separate()
        {
            if (Detail is NavigationPage collapsedNavigation)
            {
                NavigationPage pushTo = BackupDetail as NavigationPage;
                foreach (Page page in collapsedNavigation.Navigation.NavigationStack)
                {
                    Print.Log("found page" + page);
                    if (page == MasterRoot)
                    {
                        pushTo = Flyout as NavigationPage;
                    }
                    else if (page != DetailRoot && pushTo != null)
                    {
                        Print.Log("pushing page " + page + " to " + (pushTo == BackupDetail ? "Detail" : "Master"));
                        pushTo.PushAsync(page, false);
                    }
                }

                collapsedNavigation.PopToRootAsync(false);
                //collapsedNavigation.ReplaceCurrentPageAsync(new ContentPage { Title = "test" });
            }

            //Print.Log("pages", (BackupDetail as NavigationPage)?.CurrentPage, (Master as NavigationPage)?.CurrentPage);
            Detail = BackupDetail;

            Page master = Flyout;
            Flyout = new ContentPage { Title = "test" };
            Flyout = master;
        }
    }
}
