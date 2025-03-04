using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Models
{
    public class TrendDeviceVarModel:ObservableObject
    {
        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                Set(ref _isSelected, value);
                //SelectedChanged?.Invoke(this);
            }
        }

        public string DNum { get; set; }
        public string VNum { get; set; }
        public string VarName { get; set; }
        public string VarType { get; set; }

        private string _axisNum;

        public string AxisNum
        {
            get { return _axisNum; }
            set { Set(ref _axisNum, value); }
        }

        private string _color;

        public string Color
        {
            get { return _color; }
            set { Set(ref _color, value); }
        }
    }
}
