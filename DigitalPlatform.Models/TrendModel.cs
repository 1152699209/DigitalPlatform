using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DigitalPlatform.Models
{
    public class TrendModel : ObservableObject
    {
        public string TNum { get; set; } = "T" + DateTime.Now.ToString("yyyyMMddHHmmssFFFF");

        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set { Set(ref _isSelected, value); }
        }

        private string _trendHeader = "新建趋势图";

        public string TrendHeader
        {
            get { return _trendHeader; }
            set { Set(ref _trendHeader, value); }
        }

        // 图例处理
        private bool _isShowLegend;

        public bool IsShowLegend
        {
            get { return _isShowLegend; }
            set
            {
                Set(ref _isShowLegend, value);
                if (value)
                    LegendLocation = LegendLocation.Top;
                else
                    LegendLocation = LegendLocation.None;

                this.RaisePropertyChanged("LegendLocation");
            }
        }

        public LegendLocation LegendLocation
        {
            get; set;
        } = LegendLocation.None;


        // Y轴集合
        private ObservableCollection<TrendAxisModel> _axisList =
            new ObservableCollection<TrendAxisModel>() {
                new TrendAxisModel() { IsShowSeperator = true }
            };
        public ObservableCollection<TrendAxisModel> AxisList
        {
            get => _axisList;
            set
            {
                _axisList = value;

                YAxis.Clear();
                foreach (var item in value)
                {
                    YAxis.Add(item.Axis);
                }
            }
        }

        public AxesCollection YAxis { get; set; } = new AxesCollection();
        public AxesCollection XAxis { get; set; } = new AxesCollection();// 动态更新

        public RelayCommand AddAxisCommand { get; set; }
        public RelayCommand<TrendAxisModel> DeleteAxisCommand { get; set; }

        // 图表序列
        private ObservableCollection<TrendSeriesModel> _series =
           new ObservableCollection<TrendSeriesModel>();
        public ObservableCollection<TrendSeriesModel> Series
        {
            get => _series;
            set
            {
                _series = value;

                ChartSeries.Clear();
                foreach (var item in value)
                {
                    ChartSeries.Add(item.Series);
                }
            }
        }

        public SeriesCollection ChartSeries { get; set; } =
            new SeriesCollection();


        public TrendModel()
        {
            // X轴的初始化
            Axis xAxis = new Axis();
            XAxis.Add(xAxis);
            xAxis.Separator = new Separator { Step = 1, StrokeThickness = 0 };
            xAxis.Labels = new List<string>();
            xAxis.LabelsRotation = -45;

            // Y轴可以有多条
            YAxis = new AxesCollection();
            YAxis.Add(AxisList[0].Axis);

            AddAxisCommand = new RelayCommand(() =>
            {
                TrendAxisModel tmodel = new TrendAxisModel();
                AxisList.Add(tmodel);
                YAxis.Add(tmodel.Axis);
            });

            DeleteAxisCommand = new RelayCommand<TrendAxisModel>(model =>
            {
                /// 删除的时候，保留一个纵轴
                if (AxisList.Count == 1) return;
                //model.Axis.Clean();
                AxisList.Remove(model);

                YAxis.Remove(model.Axis);
            });

            Series.CollectionChanged += Series_CollectionChanged;
        }

        private void Series_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.Dispose();
            cts = new CancellationTokenSource();
            this.ChartScan();
        }

        public int ScanRate { get; set; } = 1000;
        CancellationTokenSource cts = new CancellationTokenSource();
        List<Task> tasks = new List<Task>();

        public void ChartScan()
        {
            // 根据维护所有序列信息，获取对应的设备和变量
            // 1、通过配置的设备编号获取所有设备
            List<DeviceItemModel> scanDevices = null;
            Messenger.Default.Send<Action<Func<string[], List<DeviceItemModel>>>>(
                new Action<Func<string[], List<DeviceItemModel>>>(f =>
                {
                    var dns = this.Series.Select(s => s.DNum).ToArray();
                    scanDevices = f.Invoke(dns);
                }), "DeviceInfo");

            if (scanDevices == null || scanDevices.Count == 0) return;

            var task = Task.Run(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    await Task.Delay(ScanRate);
                    // 添加一个X轴的标签：显示的时间（时：分：秒）
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        XAxis[0].Labels.Add(DateTime.Now.ToString("HH:mm:ss"));
                        if (XAxis[0].Labels.Count > 30)
                            XAxis[0].Labels.RemoveAt(0);
                    });

                    foreach (var item in Series)
                    {
                        var d = scanDevices.FirstOrDefault(d => d.DeviceNum == item.DNum);
                        if (d == null) continue;
                        var v = d.VariableList.FirstOrDefault(v => v.VarNum == item.VNum);
                        if (v == null) continue;

                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            item.Series.Values.Add(double.Parse(v.Value.ToString()));
                            if (item.Series.Values.Count > 30)
                                item.Series.Values.RemoveAt(0);
                        }));
                    }
                }
            }, cts.Token);
            tasks.Add(task);
        }
        public void Dispose()
        {
            cts.Cancel();
            Task.WaitAll(tasks.ToArray(), 500);
        }
    }
}
