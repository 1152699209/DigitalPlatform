using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Models
{
    public class MenuModel : ObservableObject
    {
        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set { Set(ref _isSelected, value); }
        }

        public int Key { get; set; }
        public string MenuHeader { get; set; }
        public string TargetView { get; set; }
        public string MenuIcon { get; set; }
    }
}
