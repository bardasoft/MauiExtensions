using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Extensions
{
    public static class NavigationPageExtensions
    {
        public static Task PopToPageAsync(this NavigationPage navigation, Page target, bool animated = false)
        {
            Task result = Task.CompletedTask;

            while (navigation.CurrentPage != target)
            {
                result = new Task(async () =>
                {
                    await result;
                    await navigation.PopAsync();
                });
            }

            return result;
        }

        public static Task ReplaceCurrentPageAsync(this NavigationPage navigation, Page replacement)
        {
            navigation.Navigation.InsertPageBefore(replacement, navigation.CurrentPage);
            return navigation.PopAsync();
        }
    }
}
