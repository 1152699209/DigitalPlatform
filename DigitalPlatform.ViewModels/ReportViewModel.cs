using DigitalPlatform.DataAccess;
using DigitalPlatform.IDAL;
using DigitalPlatform.Models;
using Entity;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DigitalPlatform.ViewModels
{
    public class ReportViewModel:ViewModelBase
    {
        //获取数据库的数据
        public ObservableCollection<RecordReadEntity> AllDatas { get; set; } = 
            new ObservableCollection<RecordReadEntity>();
        //所有可供选择的列
        public ObservableCollection<DataGridColumnModel> AllColumn { get; set; } = 
            new ObservableCollection<DataGridColumnModel>();

        //这个是跟DataGrid对接的列
        public ObservableCollection<DataGridColumn> Columns { get; set; } =
            new ObservableCollection<DataGridColumn>();
        public ICommand RefreshCommand
        {
            get => new RelayCommand(() =>
            {
                Refresh();
            });
        }
        public RelayCommand<DataGridColumnModel> ChooseColumnCommand { get; set; }

        private void Refresh()
        {
            // 从数据库来的
            var datas = _localDataAccess.GetRecords();

            // 第一种方式
            //AllDatas.Clear();
            //datas.ForEach(d => AllDatas.Add(d));

            // 第二种方式
            AllDatas = new ObservableCollection<RecordReadEntity>(datas);
            this.RaisePropertyChanged(nameof(AllDatas));

        }
        private ILocalDataAccess _localDataAccess;
        public ReportViewModel(ILocalDataAccess localDataAccess)
        {
            _localDataAccess = localDataAccess;
            var data = _localDataAccess.GetRecords();
            AllDatas=new ObservableCollection<RecordReadEntity>(data);

            ChooseColumnCommand = new RelayCommand<DataGridColumnModel>(ChangedColumn);
            // 初始化列列表
            AllColumn.Add(new DataGridColumnModel { Header = "设备编号", BindingPath = "DeviceNum", ColumnWidth = 70 });
            AllColumn.Add(new DataGridColumnModel { IsSelected = true, Header = "设备名称", BindingPath = "DeviceName", ColumnWidth = 90 });
            AllColumn.Add(new DataGridColumnModel { Header = "变量编号", BindingPath = "VariableNum", ColumnWidth = 70 });
            AllColumn.Add(new DataGridColumnModel { IsSelected = true, Header = "变量名称", BindingPath = "VariableName", ColumnWidth = 90 });
            AllColumn.Add(new DataGridColumnModel { Header = "最新值", BindingPath = "LastValue", ColumnWidth = 90 });
            AllColumn.Add(new DataGridColumnModel { Header = "平均值", BindingPath = "AvgValue", ColumnWidth = 90 });
            AllColumn.Add(new DataGridColumnModel { Header = "最大值", BindingPath = "MaxValue", ColumnWidth = 90 });
            AllColumn.Add(new DataGridColumnModel { Header = "最小值", BindingPath = "MinValue", ColumnWidth = 90 });
            AllColumn.Add(new DataGridColumnModel { Header = "报警触发次数", BindingPath = "AlarmCount", ColumnWidth = 90 });
            AllColumn.Add(new DataGridColumnModel { Header = "联控触发次数", BindingPath = "UnionCount", ColumnWidth = 90 });
            AllColumn.Add(new DataGridColumnModel { Header = "记录次数", BindingPath = "RecordCount", ColumnWidth = 80 });
            AllColumn.Add(new DataGridColumnModel { Header = "最后记录时间", BindingPath = "LastTime", ColumnWidth = 120 });
            // 添加默认列
            foreach (var item in AllColumn)
                ChangedColumn(item);

            // 获取所有数据
            Refresh();
        }

        private int index = 0;

        private void Exeport()
        {
            //用这个选路径
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            //拼接文件名
            saveFileDialog.FileName = "Report-" + DateTime.Now.ToString("yyyyMMddHHmmssFFF") + ".csv";
            if (saveFileDialog.ShowDialog() == true)
            {
                // 灵活配置的列，列没有顺序.因此按照添加顺序 去取得列的顺序
                List<DataGridColumnModel> cs = AllColumn.Where(c => c.IsSelected).OrderBy(c => c.Index).ToList();
                // 列头
                string datas = string.Join(",", cs.Select(c => c.Header)) + "\r\n";
                // 数据
                // 遍历所有数据行,一条数据就是一行
                foreach (var item in AllDatas)
                {
                    // 遍历所有数据列，
                    foreach (var col in cs)
                    {
                        // 确定当前列绑定的属性名称
                        // 需求：根据对应列的绑定属性名，称获取对应名称的属性值
                        PropertyInfo pi = item.GetType().GetProperty(col.BindingPath, BindingFlags.Instance | BindingFlags.Public); 
                        var v = pi.GetValue(item);
                        //如果数据为空 就拼接一个,。否则拼接 值+,
                        datas += v == null ? "," : v.ToString() + ",";
                    }
                    datas.Remove(datas.Length - 1);
                    datas += "\r\n";
                }
                System.IO.File.WriteAllText(saveFileDialog.FileName, datas, Encoding.UTF8);
            }
        }
        private void ChangedColumn(DataGridColumnModel model)
        {
            if (model.IsSelected)
            {
                model.Index = index++;
                Style style = new System.Windows.Style(typeof(TextBlock));
                style.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
                style.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));
                //遍历所有的列 当被选中后 就把对应的列添加给 对应View中DatGrid的Columns集合中
                Columns.Add(new DataGridTextColumn
                {
                    Header = model.Header,
                    Binding = new Binding(model.BindingPath),
                    MinWidth = model.ColumnWidth,
                    ElementStyle = style
                });
            }
            else
            {
                var column = Columns.FirstOrDefault(c => c.Header.ToString() == model.Header);
                Columns.Remove(column);
            }
        }

    }
}
