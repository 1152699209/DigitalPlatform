using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;

namespace DigitalPlatform.Common.Converter
{
    /// <summary>
    /// 趋势图列表中 每一个Item都有一个TextBox。但TextBox会抢占ListBoxItem的焦点 使得Item无法选中，
    /// 通过这个事件让TextBox获得焦点的事件处理中，把Item也选中。
    /// </summary>
    public class FocuseToSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //通过这个转换器 给TextBox挂载一个事件
            (value as TextBoxBase).GotFocus -= FocuseToSelectedConverter_GotFocus;
            (value as TextBoxBase).GotFocus += FocuseToSelectedConverter_GotFocus;


            return Visibility.Visible;
        }

        private void FocuseToSelectedConverter_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            ((sender as TextBoxBase).Tag as ListBoxItem).IsSelected = true;
            //.SetValue(ListViewItem.IsSelectedProperty, true);
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

    }
}
