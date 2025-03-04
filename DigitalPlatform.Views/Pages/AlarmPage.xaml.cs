using DigitalPlatform.ViewModels;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DigitalPlatform.Views.Pages
{
    /// <summary>
    /// AlarmPage.xaml 的交互逻辑
    /// </summary>
    public partial class AlarmPage : UserControl
    {
        public AlarmPage()
        {
            InitializeComponent();
            this.Unloaded += AlarmPage_Unloaded;
        }

        //销毁容器 让每次打开页面 都能通过构造器 获取到最新的报警数据
        private void AlarmPage_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModelLocator.Cleanup<AlarmViewModel>();
        }
    }
}
