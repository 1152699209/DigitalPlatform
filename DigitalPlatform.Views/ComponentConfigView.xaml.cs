using DigitalPlatform.Assets;
using DigitalPlatform.Views.DialogView;
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
using System.Windows.Shapes;

namespace DigitalPlatform.Views
{
    /// <summary>
    /// ComponentConfigView.xaml 的交互逻辑
    /// </summary>
    public partial class ComponentConfigView : Window
    {
        public ComponentConfigView()
        {
            InitializeComponent();
            ActionManager.Register<object>("AlarmCondition", new Action<object>(obj =>
            {
                new VariableAlarmConditionDialog()
                {
                    Owner = this,
                    DataContext = obj
                }.ShowDialog();
            })); 

            ActionManager.Register<object>("UnionCondition", new Action<object>(obj =>
            {
                new VariableUnionConditionDialog()
                {
                    Owner = this,
                    DataContext = obj
                }.ShowDialog();
            }));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            ActionManager.Unregister("AlarmCondition");
            ActionManager.Unregister("UnionCondition");

        }
    }
}
