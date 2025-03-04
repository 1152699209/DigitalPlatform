using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Models
{
    public class DataGridColumnModel:ObservableObject
    {
        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set { Set(ref _isSelected, value); }
        }
        public int Index { get; set; }
        //列名
        public string Header { get; set; }
        //绑定哪一列
        public string BindingPath { get; set; }
        public int ColumnWidth { get; set; }
    }
}
