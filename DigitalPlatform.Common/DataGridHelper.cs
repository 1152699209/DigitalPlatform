using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace DigitalPlatform.Common
{
    public class DataGridHelper
    {
        public static ObservableCollection<DataGridColumn> GetColumns(DependencyObject obj)
        {
            return (ObservableCollection<DataGridColumn>)obj.GetValue(ColumnsProperty);
        }

        public static void SetColumns(DependencyObject obj, ObservableCollection<DataGridColumn> value)
        {
            obj.SetValue(ColumnsProperty, value);
        }

        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.RegisterAttached("Columns", typeof(ObservableCollection<DataGridColumn>), typeof(DataGridHelper), new PropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        //这里用Behavir库 事件转命令也能写
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //为集合绑定对应的事件
            DataGrid dataGrid = (d as DataGrid);
            DataGridHelper.GetColumns(d).CollectionChanged += (se, ev) =>
            {
                dataGrid = (d as DataGrid);
                dataGrid.Columns.Clear();

                foreach (var item in DataGridHelper.GetColumns(dataGrid))
                {
                    dataGrid.Columns.Add(item);
                }
            };
            //ReportViewModel中的代码会先执行，执行后把默认的两个列添加到列表中，才后才会到这里
            //如果不写这个初始化一下dataGrid中的Columns 就不会显示结果
            foreach (var item in DataGridHelper.GetColumns(d))
            {
                //这里会引发异常：如果切换后再打开这个页面 就会报错。
                //ViewModel没有销毁
                //原因 VM中与这里对应的集合元素 重复用于进行添加
                //要让view和viewmodel同步销毁
                dataGrid.Columns.Add(item);
            }
        }
    }
}
