using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Models
{
    public class ConditionModel
    {
        // 作用后续报警重复提示的区分
        public string CNum { get; set; }
        public string Operator { get; set; }
        public string CompareValue { get; set; }
        public string AlarmContent { get; set; }

        public ObservableCollection<UnionDeivceModel> UnionDeviceList { get; set; } = new ObservableCollection<UnionDeivceModel>();

        public RelayCommand AddDeviceCommand
        {
            get => new RelayCommand(() =>
            {
                UnionDeviceList.Add(new UnionDeivceModel() { ValueType = "UInt16", UNum = "U" + DateTime.Now.ToString("yyyyMMddHHmmssFFFF") });
            });
        }
        public RelayCommand<UnionDeivceModel> DelDeviceCommand
        {
            get => new RelayCommand<UnionDeivceModel>(model =>
            {
                UnionDeviceList.Remove(model);
            });
        }
    }
}
