using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class RecordReadEntity
    {
        [Column("var_num")]
        public string VariableNum { get; set; }
        [Column("var_name")]
        public string VariableName { get; set; }
        [Column("device_num")]
        public string DeviceNum { get; set; }
        [Column("device_name")]
        public string DeviceName { get; set; }
        [Column("last_value")]
        public string LastValue { get; set; }//最新值
        [Column("avg")]
        public string AvgValue { get; set; }// 平均值
        [Column("max")]
        public string MaxValue { get; set; }// 记录中的最大值
        [Column("min")]
        public string MinValue { get; set; }// 记录中的最小值
        [Column("alarm_count")]
        public string AlarmCount { get; set; }// 报警触发次数
        [Column("union_count")]
        public string UnionCount { get; set; }// 联控触发次数
        [Column("last_time")]
        public string LastTime { get; set; }// 最新记录时间
        [Column("record_count")]
        public string RecordCount { get; set; }// 总记录数
    }
}
