    using DigitalPlatform.Assets;
using DigitalPlatform.Views.Dialog;
using DigitalPlatform.Views.DialogView;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DigitalPlatform.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            if (new LoginView().ShowDialog() != true)
            {
                Application.Current.Shutdown();
            }
            InitializeComponent();
            //注册 一个返回值的方法
            ActionManager.Register<object>("AAA", 
                new Func<object, bool>(ShowConfigView));
            ActionManager.Register<object>("ShowRight", 
                new Func<object, bool>(ShowRightView));
            ActionManager.Register<object>("ShowTrendAxis", 
                new Func<object, bool>(ShowTrendAxisDialog));


            ActionManager.Register<object>("ShowTrendVar",
                new Func<object, bool>(ShowTrendDeviceVars));
        }

        private bool ShowConfigView(object obj)
        {
            return ShowDialog(new ComponentConfigView() { Owner = this });
        }

        private bool ShowTrendDeviceVars(object obj)
        {
            return ShowDialog(new TrendDeviceChooseDialog() { Owner = this, DataContext = obj });
        }
        private bool ShowTrendAxisDialog(object obj)
        {
            return ShowDialog(new TrendAxisEditDialog() { Owner = this ,DataContext=obj});
        }

        private bool ShowDialog(Window dialog)
        {
            this.Effect = new BlurEffect() { Radius = 5 };
            bool state=dialog.ShowDialog() == true;
            this.Effect = null;
            return state;
        }

        private bool ShowRightView(object obj)
        {
            //判断 这个窗口的结果
            return ShowDialog(new RightRemindDialog() { Owner = this });
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
