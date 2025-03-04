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
    /// ReportPage.xaml 的交互逻辑
    /// </summary>
    public partial class ReportPage : UserControl
    {
        public ReportPage()
        {
            InitializeComponent();
            this.Unloaded += ReportPage_Unloaded;
        }

        private void ReportPage_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModelLocator.Cleanup<ReportViewModel>();
        }
    }
}
