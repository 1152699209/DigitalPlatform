using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace DigitalPlatform.Common.Converter
{
    public class MouseMoveConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //0号参数 是 鼠标事件   1号参数是容器  2号参数是设备
            return new MouseActionCommandParameter
            {
                MouseEventArgs = values[0] as MouseEventArgs,
                Container = values[1],
                Device = values[2]
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
