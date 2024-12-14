using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
    public class ObservableConverter : IMultiValueConverter
    {
        public IValueConverter Converter { get; set; }
        public ObservableCollection<object> Collection { get; } = new ObservableCollection<object>();

        private object Result;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            /*for (int i = 0; i < Collection.Count; i++)
            {
                if (!values.Contains(Collection[i]))
                {
                    Collection.RemoveAt(i--);
                }
            }

            int index = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (i < Collection.Count && values[i] == Collection[i])
                {
                    index++;
                }
                //if (values[i] != null && (i >= Collection.Count || values[i] != Collection[i]))
                else if (values[i] != null)
                {
                    Collection.Insert(index++, values[i]);
                }
            }*/

            int index = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == null)
                {
                    continue;
                }

                // Find where in the Collection the current value is 
                // TODO: This should start searching at 'index' - the current value should (in theory)
                // never occur before
                int pos = Collection.IndexOf(values[i]);

                // Needs to be added to 'Collection'
                if (pos == -1)
                {
                    // This first conidition is not needed, but better to do a replace than add + delete
                    if (index < Collection.Count && !values.Contains(Collection[index]))
                    {
                        Collection[index] = values[i];
                    }
                    else
                    {
                        Collection.Insert(index, values[i]);
                    }
                }
                else
                {
                    // We know values[i] is in Collection, so remove values until we get to it
                    while (!Collection[index].Equals(values[i]))
                    {
                        Collection.RemoveAt(index);
                    }
                }

                index++;
            }

            if (Collection.Count == 0)
            {
                Result = null;
            }
            else if (Result == null)
            {
                Result = Converter?.Convert(Collection, targetType, parameter, culture) ?? Collection;
            }
            else //if (Result == Collection)
            {
                Result = Binding.DoNothing;
            }

            return Result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
