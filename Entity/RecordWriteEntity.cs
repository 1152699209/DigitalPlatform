using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zhaoxi.DigitaPlatform.Entities
{
    public class RecordWriteEntity
    {
        public string DeviceNum { get; set; }
        public string DeviceName { get; set; }
        public string VarNum { get; set; }
        public string VarName { get; set; }
        public string RecordValue { get; set; }
        public string AlarmNum { get; set; }
        public string UnionNum { get; set; }
        public string RecordTime { get; set; }
        public string UserName { get; set; }
    }
}
