using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.DeviceAccess.Base
{
    public class VariableProperty
    {
        public string VarId { get; set; }
        public string VarAddr { get; set; }
        public Type ValueType { get; set; }// 数据类型
        public byte[] ValueBytes { get; set; }// 数据值


    }
}
