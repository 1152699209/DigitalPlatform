using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Models
{
    public class TrendDeviceModel
    {
        public string Header { get; set; }

        public List<TrendDeviceVarModel> VarList { get; set; }
    }
}
