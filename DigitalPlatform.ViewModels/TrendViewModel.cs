using DigitalPlatform.Assets;
using DigitalPlatform.Entities;
using DigitalPlatform.IDAL;
using DigitalPlatform.Models;
using Entity;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DigitalPlatform.ViewModels
{
    public class TrendViewModel : ViewModelBase
    {
        public ObservableCollection<TrendModel> Trends { get; set; } =
            new ObservableCollection<TrendModel>() { new TrendModel() { IsSelected = true } };

        public TrendModel CurrentTrend { get; set; }

        public RelayCommand AddTrendCommand { get; set; }
        public RelayCommand<TrendModel> DelTrendCommand { get; set; }

        public RelayCommand ShowAxisDialogCommand { get; set; }
        public RelayCommand ShowDeviceVarDialogCommand { get; set; }

        public RelayCommand SaveCommand { get; set; }
        public RelayCommand<object> SaveToImageCommand { get; set; }

        public List<string> BrushList
        {
            get
            {
                return typeof(Brushes).GetProperties().Select(p => p.Name).ToList();
            }
        }

        // 这种方式可以实现需求，但是增加了耦合度，推荐使用Messenger
        MainViewModel _mainViewModel;
        ILocalDataAccess _localDataAccess;
        public TrendViewModel(MainViewModel mainViewModel, ILocalDataAccess localDataAccess)
        {
            _mainViewModel = mainViewModel;
            _localDataAccess = localDataAccess;

            AddTrendCommand = new RelayCommand(AddTrend);
            DelTrendCommand = new RelayCommand<TrendModel>(DelTrend);

            ShowAxisDialogCommand = new RelayCommand(() =>
            {
                if (CurrentTrend == null) return;

                ActionManager.ExecuteAndResult("ShowTrendAxis",
                    new TrendAxisDialogViewModel
                    {
                        Trend = CurrentTrend,
                        BrushList = BrushList
                    });
            });
            ShowDeviceVarDialogCommand = new RelayCommand(ShowDeviceVarDialog);

            SaveCommand = new RelayCommand(Save);
            SaveToImageCommand = new RelayCommand<object>(SaveToImage);

            // 图表配置的保存
            ChartInit();
            // 图表配置的重载
            // 开始启动图表数据刷新
            foreach (var item in Trends)
            {
                item.ChartScan();
            }
        }

        private void Save()
        {
            var trends = this.Trends.Select(t => new TrendEntity
            {
                TNum = t.TNum,
                THeader = t.TrendHeader,
                ShowLegend = t.IsShowLegend,

                Axes = t.AxisList.Select(a => new AxisEntity
                {
                    ANum = a.ANum,
                    Title = a.Title,
                    IsShowTitle = a.IsShowTitle,
                    Minimum = a.Minimum.ToString(),
                    Maximum = a.Maximum.ToString(),
                    IsShowSeperator = a.IsShowSeperator,
                    LabelFormatter = a.LabelFormater,
                    Position = a.Position,

                    Sections = a.Sections.Select(s => new SectionEntity
                    {
                        Value = s.Value.ToString(),
                        Color = s.Color
                    }).ToList()
                }).ToList(),

                Series = t.Series.Select(s => new SeriesEntity
                {
                    DNum = s.DNum,
                    VNum = s.VNum,
                    Title = s.Title,
                    Color = s.Color,
                    AxisNum = s.AxisNum
                }).ToList()
            }).ToList();
            _localDataAccess.SaveTrend(trends);
        }

        private void SaveToImage(object obj)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "Chart" + DateTime.Now.ToString("yyyyMMddHHmmssFFF") + ".png";
            saveFileDialog.CheckPathExists = true;
            if (saveFileDialog.ShowDialog() == true)
            {
                CreateBitmapFromVisual((obj as Visual), saveFileDialog.FileName);
            }
        }
        private void CreateBitmapFromVisual(Visual target, string fileName)
        {
            if (target == null || string.IsNullOrEmpty(fileName)) return;

            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);

            RenderTargetBitmap renderTarget = new RenderTargetBitmap((Int32)bounds.Width, (Int32)bounds.Height, 96, 96, PixelFormats.Pbgra32);

            DrawingVisual visual = new DrawingVisual();

            using (DrawingContext context = visual.RenderOpen())
            {
                VisualBrush visualBrush = new VisualBrush(target);
                context.DrawRectangle(visualBrush, null, new Rect(new Point(), bounds.Size));
            }

            renderTarget.Render(visual);
            PngBitmapEncoder bitmapEncoder = new PngBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTarget));
            using (Stream stm = File.Create(fileName))
            {
                bitmapEncoder.Save(stm);
            }
        }

        public override void Cleanup()
        {
            foreach (var item in Trends)
            {
                item.Dispose();
            }
        }
        private void ChartInit()
        {
            var tes = _localDataAccess.GetTrends();
            this.Trends = new ObservableCollection<TrendModel>(tes.Select(t => new TrendModel
            {
                TNum = t.TNum,
                TrendHeader = t.THeader,
                IsShowLegend = t.ShowLegend,

                AxisList = new ObservableCollection<TrendAxisModel>(t.Axes.Select(a => new TrendAxisModel
                {
                    ANum = a.ANum,
                    Title = a.Title,
                    IsShowTitle = a.IsShowTitle,
                    Minimum = string.IsNullOrEmpty(a.Minimum) ? 0 : double.Parse(a.Minimum),
                    Maximum = string.IsNullOrEmpty(a.Maximum) ? 100 : double.Parse(a.Maximum),
                    IsShowSeperator = a.IsShowSeperator,
                    LabelFormater = a.LabelFormatter,
                    Position = a.Position,

                    Sections = new ObservableCollection<TrendSectionModel>(a.Sections.Select(s => new TrendSectionModel
                    {
                        Value = string.IsNullOrEmpty(s.Value) ? 0 : double.Parse(s.Value),
                        Color = s.Color
                    }))
                })),

                Series = new ObservableCollection<TrendSeriesModel>(t.Series.Select(s => new TrendSeriesModel
                {
                    DNum = s.DNum,
                    VNum = s.VNum,
                    Title = s.Title,
                    Color = s.Color,
                    AxisNum = string.IsNullOrEmpty(s.AxisNum) ? t.Axes[0].ANum : s.AxisNum
                }))
            }));

            if (this.Trends.Count > 0)
                this.Trends[0].IsSelected = true;
        }


        private void ShowDeviceVarDialog()
        {
            if (CurrentTrend == null) return;
            ActionManager.ExecuteAndResult("ShowTrendVars",
                new TrendDeviceDialogViewModel(CurrentTrend, BrushList, _mainViewModel));
        }


        private void AddTrend()
        {
            Trends.Add(new TrendModel() { IsSelected = true });
        }
        private void DelTrend(TrendModel model)
        {
            if (Trends.Count == 1) return;

            int index = Math.Max(0, Trends.IndexOf(model) - 1);

            Trends.Remove(model);
            if (model.IsSelected)
            {
                Trends[index].IsSelected = true;
            }
        }
    }
}
