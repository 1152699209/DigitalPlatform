using DigitalPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.ViewModels
{
    public class TrendDeviceDialogViewModel
    {
        public TrendModel Trend { get; set; }
        public List<TrendDeviceModel> ChooseDevicesList { get; set; }
        public List<string> BrushList { get; set; }


        public TrendDeviceDialogViewModel(TrendModel trend, List<string> brushList,MainViewModel model)
        {
            Trend = trend;
            BrushList = brushList;
            ChooseDevicesList = model.DeviceList
               .Where(d => d.VariableList.Count > 0)
               .Select(d => new TrendDeviceModel
               {
                   Header = d.Header,
                   //为设备找出 其对应的变量列表
                   VarList = d.VariableList.Select(v => InitDevices(d, v)).ToList()

               }).ToList();
        }

        private TrendDeviceVarModel InitDevices(DeviceItemModel dm, VariableModel vm)
        {
            // 检查当前图标中是否已经存在对应设备的对应变量的曲线图
            
            var item = Trend.Series.ToList()
                .FirstOrDefault(ts => ts.DNum == dm.DeviceNum && ts.VNum == vm.VarNum);

            TrendDeviceVarModel varModel = new TrendDeviceVarModel();   

            varModel.IsSelected = item != null;
            varModel.DNum = dm.DeviceNum;
            varModel.VNum = vm.VarNum;
            varModel.VarName = vm.VarName;
            varModel.VarType = vm.VarType;

            varModel.AxisNum = item == null ? Trend.AxisList[0].ANum : item.AxisNum;
            varModel.Color = item == null ? BrushList[new Random().Next(0, BrushList.Count)] : item.Color;

            //deviceModel.SelectedChanged = SeriesChanged;
            //为设备变量趋势对象绑定一个属性变换事件
            varModel.PropertyChanged += (se, ev) =>
            {
                if (ev.PropertyName == "IsSelected")
                {
                    SeriesChanged(se as TrendDeviceVarModel);
                }
                else if (ev.PropertyName == "Color" || ev.PropertyName == "AxisNum")
                {
                    var m = se as TrendDeviceVarModel;
                    if (m == null) return;
                    var si = Trend.Series.FirstOrDefault(ts => ts.DNum == m.DNum && ts.VNum == m.VNum);
                    if (si == null) return;
                    si.Color = m.Color;
                    si.AxisNum = m.AxisNum;
                }
            };
            return varModel;
        }

        private void SeriesChanged(TrendDeviceVarModel model)
        {
            if (Trend == null) return;
            if (model.IsSelected)
            {
                //如果选中 就在两个列表中分别添加对应的序列元素
                // 添加序列
                TrendSeriesModel tsModel = new TrendSeriesModel
                {
                    DNum = model.DNum,
                    VNum = model.VNum,

                    Title = model.VarName,
                    Color = model.Color,
                    AxisIndexFunc = num => Trend.AxisList.ToList().FindIndex(a => a.ANum == num)
                };
                Trend.Series.Add(tsModel);
                Trend.ChartSeries.Add(tsModel.Series);
            }
            else
            {
                // 移除序列
                int index = Trend.Series.ToList().FindIndex(s => s.DNum == model.DNum && s.VNum == model.VNum);
                Trend.Series.RemoveAt(index);
            }
        }
    }
}
