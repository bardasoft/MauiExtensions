using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
    public static class PropertyExtensions
    {
        public static bool IsProperty(this System.ComponentModel.PropertyChangedEventArgs e, params BindableProperty[] properties) => e.PropertyName.IsProperty(properties);

        public static bool IsProperty(this string propertyName, params BindableProperty[] properties)
        {
            foreach (BindableProperty property in properties)
            {
                if (propertyName == property.PropertyName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
