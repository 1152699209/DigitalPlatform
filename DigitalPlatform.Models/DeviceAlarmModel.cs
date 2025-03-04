using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Models
{
    public class DeviceAlarmModel
    {
        //报警编码
        public string ANum { get; set; }
        //条件编码
        public string CNum { get; set; }
        //设备编码
        public string DNum { get; set; }
        //变量编码
        public string VNum { get; set; }
        //报警内容
        public string AlarmContent { get; set; }
        //报警时间
        public string DateTime { get; set; }
        //等级
        public int Level { get; set; }
        //状态
        public int State { get; set; }
        //报警值
        public string AlarmValue { get; set; }
    }
}
