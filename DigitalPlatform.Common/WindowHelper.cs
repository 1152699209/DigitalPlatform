using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DigitalPlatform.Common
{
    public class WindowHelper
    {
        /// <summary>
        /// 定义一个附加属性 来判断窗口是否关闭
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool GetClose(DependencyObject obj)
        {
            return (bool)obj.GetValue(CloseProperty);
        }

        public static void SetClose(DependencyObject obj, bool value)
        {
            obj.SetValue(CloseProperty, value);
        }

        // Using a DependencyProperty as the backing store for Close.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CloseProperty =
            DependencyProperty.RegisterAttached("Close", typeof(bool), typeof(WindowHelper),
                new PropertyMetadata(false, (s, e) =>
                {
                    if ((bool)e.NewValue)
                    {
                        (s as Window).Close();
                    }
                }));
    }
}
