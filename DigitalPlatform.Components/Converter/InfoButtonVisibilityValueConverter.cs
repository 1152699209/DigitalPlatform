using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace DigitalPlatform.Components.Converter
{
    public class InfoButtonVisibilityValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool stateBool = bool.TryParse(values[0].ToString(), out bool s1);
                bool stateInt = int.TryParse(values[1].ToString(), out int s2);
                if (stateBool && stateInt && s1 && s2 > 0)
                    return Visibility.Visible;

                return Visibility.Collapsed;
            }
            catch { return Visibility.Collapsed; }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
