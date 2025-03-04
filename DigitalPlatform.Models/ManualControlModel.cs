using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Models
{
    public class ManualControlModel
    {
        //名称
        public string ControlHeader { get; set; }
        //控制的地址
        public string ControlAddress { get; set; }
        //写入值
        public string Value { get; set; }
    }
}
