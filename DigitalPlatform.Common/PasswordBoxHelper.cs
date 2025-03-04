using System;
using System.Windows.Controls;
using System.Windows;

namespace DigitalPlatform.Common
{
    public class PasswordBoxHelper
    {
        #region 密码值的附加属性
        public static readonly DependencyProperty PasswordProperty = DependencyProperty.RegisterAttached("Password", typeof(string), typeof(PasswordBoxHelper),
              new PropertyMetadata(new PropertyChangedCallback(OnPropertyChanged)));

        public static string GetPassword(DependencyObject d)
        {
            return (string)d.GetValue(PasswordProperty);
        }
        public static void SetPassword(DependencyObject d, string value)
        {
            d.SetValue(PasswordProperty, value);
        }
        #endregion

        #region 挂载密码变化事件的附加属性
        public static readonly DependencyProperty AttachProperty = DependencyProperty.RegisterAttached("Attach", typeof(string), typeof(PasswordBoxHelper),
            new PropertyMetadata(new PropertyChangedCallback(OnAttachChanged)));

        public static string GetAttach(DependencyObject d)
        {
            return (string)d.GetValue(PasswordProperty);
        }
        public static void SetAttach(DependencyObject d, string value)
        {
            d.SetValue(PasswordProperty, value);
        }
        #endregion

        // 当密码输入变化时处理的逻辑
        static bool _isUpdating = false;
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox pb = (d as PasswordBox);
            pb.PasswordChanged -= Pb_PasswordChanged;
            if (!_isUpdating)
               (d as PasswordBox).Password = e.NewValue.ToString();
            pb.PasswordChanged += Pb_PasswordChanged;
        }
        // 挂载事件变化逻辑
        private static void OnAttachChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox pb = (d as PasswordBox);
            pb.PasswordChanged += Pb_PasswordChanged;
        }

        private static void Pb_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox pb = (sender as PasswordBox);
            _isUpdating = true;
            SetPassword(pb, pb.Password);
            _isUpdating = false;
        }
    }
}
