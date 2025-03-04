using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace DigitalPlatform.Common
{
    public class MouseActionCommandParameter
    {

        public MouseEventArgs MouseEventArgs { get; set; }
        public object Container { get; set; }
        public object Device { get; set; }
    }
}
