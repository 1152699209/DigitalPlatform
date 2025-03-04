using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Models
{
    public class SettingsInfoModel:ObservableObject
    {
        // 基础信息的编号
        public string BaseNum { get; set; }
        public string Header { get; set; }
        public string Desc { get; set; }
        private string _deviceNum;

        public string DeviceNum
        {
            get { return _deviceNum; }
            set
            {
                _deviceNum = value;
                if (string.IsNullOrEmpty(value)) return;

                // 动态的可选变量列表
                VariableNum = "";
                VarList.Clear();
                var device = DeviceList.FirstOrDefault(d => d.DeviceNum == value);
                if (device == null) return;

                device.VariableList.ToList().ForEach(v => VarList.Add(v));

                if (VarList.Count > 0)
                    VariableNum = VarList[0].VarNum;
            }
        }

        private string _variableNum;

        public string VariableNum
        {
            get { return _variableNum; }
            set { Set(ref _variableNum, value); }
        }


        // 选择下拉
        public List<DeviceItemModel> DeviceList { get; set; }
        public ObservableCollection<VariableModel> VarList { get; set; } =
            new ObservableCollection<VariableModel>();
    }
}
